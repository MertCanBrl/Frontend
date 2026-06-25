import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { AdminService } from '../../core/services/admin.service';
import { InstructorService } from '../../core/services/instructor.service';
import { UserDto, CreateUserRequest, UpdateUserRequest, CreateCourseRequest, UpdateCourseRequest, CourseDto, ApprovalListItemDto, AdminCourseContentDto } from '../../core/models/admin.models';
import { ProgramOutcomeDto, SaveProgramOutcomeRequest, UpdateProgramOutcomeRequest } from '../../core/models/course.models';
import { extractErrorMessage, extractBlobErrorMessage } from '../../core/utils/http-error.util';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard implements OnInit {
  private authService = inject(AuthService);
  private adminService = inject(AdminService);
  private instructorService = inject(InstructorService);
  private fb = inject(FormBuilder);

  fullName = this.authService.getFullName();
  activeTab = signal<'users' | 'courses' | 'outcomes' | 'approvals'>('users');

  users = signal<UserDto[]>([]);
  courses = signal<CourseDto[]>([]);

  usersLoading = signal(false);
  coursesLoading = signal(false);

  userFormVisible = signal(false);
  editingUserId = signal<number | null>(null);
  deletingUserId = signal<number | null>(null);
  courseFormVisible = signal(false);
  editingCourseId = signal<number | null>(null);
  deletingCourseId = signal<number | null>(null);

  // Personelin dersleri modalı
  coursesModalVisible = signal(false);
  coursesModalUserName = signal('');
  userCourses = signal<CourseDto[]>([]);
  userCoursesLoading = signal(false);
  userCoursesError = signal('');

  userFormLoading = signal(false);
  courseFormLoading = signal(false);

  userFormError = signal('');
  courseFormError = signal('');
  userFormSuccess = signal('');
  courseFormSuccess = signal('');

  instructors = computed(() => this.users().filter((u) => u.role === 'Instructor'));

  readonly titles = [
    'Prof. Dr.',
    'Doç. Dr.',
    'Dr. Öğr. Üyesi',
    'Öğr. Gör. Dr.',
    'Öğr. Gör.',
    'Arş. Gör. Dr.',
    'Arş. Gör.',
  ];

  userForm = this.fb.group({
    title: ['Prof. Dr.', [Validators.required]],
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
  });

  courseForm = this.fb.group({
    code: ['', [Validators.required]],
    name: ['', [Validators.required]],
    semester: ['', [Validators.required]],
    credit: [3, [Validators.required, Validators.min(1), Validators.max(10)]],
    classYear: [null as number | null, [Validators.required, Validators.min(1), Validators.max(4)]],
    isMandatory: [true],
    instructorId: [null as number | null],
  });

  // Program Outcomes
  programOutcomes = signal<ProgramOutcomeDto[]>([]);
  outcomesLoading = signal(false);
  outcomeFormVisible = signal(false);
  outcomeFormLoading = signal(false);
  outcomeFormError = signal('');
  outcomeFormSuccess = signal('');
  editingOutcomeId = signal<number | null>(null);

  outcomeForm = this.fb.group({
    code: ['', [Validators.required]],
    description: ['', [Validators.required]],
    details: [''],
  });

  // ── Ders İçeriği Önizleme ─────────────────────────────────────────────────

  previewVisible = signal(false);
  previewLoading = signal(false);
  previewError = signal('');
  previewData = signal<AdminCourseContentDto | null>(null);
  previewTab = signal<'info' | 'topics' | 'outcomes' | 'mapping' | 'survey'>('info');

  readonly contributionLabels = ['—', 'Çok Düşük', 'Düşük', 'Orta', 'Yüksek', 'Çok Yüksek'];

  pdfDownloading = signal<number | null>(null);

  downloadPdf(courseId: number, courseCode: string): void {
    this.pdfDownloading.set(courseId);
    this.adminService.downloadCoursePdf(courseId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `ders-icerigi-${courseCode.replace('/', '-')}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
        this.pdfDownloading.set(null);
      },
      error: async (err) => {
        this.pdfDownloading.set(null);
        this.approvalsError.set(await extractBlobErrorMessage(err, 'PDF oluşturulamadı. Lütfen tekrar deneyin.'));
      },
    });
  }

  openPreview(courseId: number): void {
    this.previewData.set(null);
    this.previewError.set('');
    this.previewTab.set('info');
    this.previewVisible.set(true);
    this.previewLoading.set(true);
    this.adminService.getCourseContent(courseId).subscribe({
      next: (data) => { this.previewData.set(data); this.previewLoading.set(false); },
      error: () => { this.previewLoading.set(false); this.previewError.set('İçerik yüklenirken bir hata oluştu.'); },
    });
  }

  closePreview(): void {
    this.previewVisible.set(false);
    this.previewData.set(null);
  }

  getPreviewContribution(loId: number, poId: number): number {
    const matrix = this.previewData()?.matrix;
    if (!matrix) return 0;
    return matrix.mappings.find(m => m.learningOutcomeId === loId && m.programOutcomeId === poId)?.contributionLevel ?? 0;
  }

  previewContributionClass(level: number): string {
    return ['level-0', 'level-1', 'level-2', 'level-3', 'level-4', 'level-5'][level] ?? 'level-0';
  }

  // ── Onay Yönetimi ─────────────────────────────────────────────────────────

  approvals = signal<ApprovalListItemDto[]>([]);
  approvalsLoading = signal(false);
  approvalsSuccess = signal('');
  approvalsError = signal('');

  revisionModalVisible = signal(false);
  revisionTargetId = signal<number | null>(null);
  revisionTargetName = signal('');
  revisionLoading = signal(false);

  revisionForm = this.fb.group({
    note: ['', [Validators.required, Validators.minLength(1)]],
  });

  ngOnInit(): void {
    this.loadUsers();
    this.loadCourses();
    this.loadProgramOutcomes();
    this.loadApprovals();
  }

  loadUsers(): void {
    this.usersLoading.set(true);
    this.adminService.getUsers().subscribe({
      next: (users) => { this.users.set(users); this.usersLoading.set(false); },
      error: () => this.usersLoading.set(false),
    });
  }

  loadCourses(): void {
    this.coursesLoading.set(true);
    this.adminService.getCourses().subscribe({
      next: (courses) => { this.courses.set(courses); this.coursesLoading.set(false); },
      error: () => this.coursesLoading.set(false),
    });
  }

  openUserForm(): void {
    this.editingUserId.set(null);
    this.userForm.reset({ title: 'Prof. Dr.', phoneNumber: '' });
    this.userFormError.set('');
    this.userFormSuccess.set('');
    this.userFormVisible.set(true);
  }

  editUser(user: UserDto): void {
    this.editingUserId.set(user.id);
    this.userForm.reset({
      title: 'Prof. Dr.', // düzenlemede ünvan kullanılmaz (ad-soyad zaten tam ad)
      fullName: user.fullName,
      email: user.email,
      phoneNumber: user.phoneNumber ?? '',
    });
    this.userFormError.set('');
    this.userFormSuccess.set('');
    this.userFormVisible.set(true);
  }

  cancelUserForm(): void {
    this.userFormVisible.set(false);
    this.editingUserId.set(null);
    this.userFormError.set('');
  }

  submitUserForm(): void {
    if (this.userForm.invalid) { this.userForm.markAllAsTouched(); return; }

    this.userFormLoading.set(true);
    this.userFormError.set('');

    const raw = this.userForm.getRawValue();
    const phoneNumber = raw.phoneNumber?.trim() || null;

    const editingId = this.editingUserId();
    if (editingId !== null) {
      const req: UpdateUserRequest = { fullName: raw.fullName!, email: raw.email!, phoneNumber };
      this.adminService.updateUser(editingId, req).subscribe({
        next: (user) => {
          this.userFormLoading.set(false);
          this.userFormSuccess.set(`${user.fullName} bilgileri güncellendi.`);
          this.users.update((list) =>
            list.map((u) => (u.id === user.id ? user : u)).sort((a, b) => a.fullName.localeCompare(b.fullName)));
          this.userFormVisible.set(false);
          this.editingUserId.set(null);
        },
        error: (err) => {
          this.userFormLoading.set(false);
          this.userFormError.set(extractErrorMessage(err, 'Kullanıcı güncellenirken bir hata oluştu, lütfen tekrar deneyin.'));
        },
      });
      return;
    }

    const req: CreateUserRequest = { title: raw.title!, fullName: raw.fullName!, email: raw.email!, phoneNumber };
    this.adminService.createUser(req).subscribe({
      next: (user) => {
        this.userFormLoading.set(false);
        this.userFormSuccess.set(`${user.fullName} başarıyla tanımlandı. Giriş bilgileri e-posta ile gönderildi.`);
        this.users.update((list) => [...list, user].sort((a, b) => a.fullName.localeCompare(b.fullName)));
        this.userFormVisible.set(false);
      },
      error: (err) => {
        this.userFormLoading.set(false);
        this.userFormError.set(extractErrorMessage(err, 'Bir hata oluştu, lütfen tekrar deneyin.'));
      },
    });
  }

  deleteUser(user: UserDto): void {
    if (!confirm(`"${user.fullName}" kullanıcısını silmek istiyor musunuz? Bu işlem geri alınamaz.`)) return;

    this.deletingUserId.set(user.id);
    this.userFormError.set('');
    this.adminService.deleteUser(user.id).subscribe({
      next: () => {
        this.deletingUserId.set(null);
        this.users.update((list) => list.filter((u) => u.id !== user.id));
        this.userFormSuccess.set(`"${user.fullName}" silindi.`);
      },
      error: (err) => {
        this.deletingUserId.set(null);
        this.userFormSuccess.set('');
        this.userFormError.set(extractErrorMessage(err, 'Kullanıcı silinemedi, lütfen tekrar deneyin.'));
      },
    });
  }

  viewUserCourses(user: UserDto): void {
    this.coursesModalUserName.set(user.fullName);
    this.userCourses.set([]);
    this.userCoursesError.set('');
    this.userCoursesLoading.set(true);
    this.coursesModalVisible.set(true);
    this.adminService.getUserCourses(user.id).subscribe({
      next: (courses) => {
        this.userCourses.set(courses);
        this.userCoursesLoading.set(false);
      },
      error: (err) => {
        this.userCoursesLoading.set(false);
        this.userCoursesError.set(extractErrorMessage(err, 'Dersler yüklenirken bir hata oluştu.'));
      },
    });
  }

  closeCoursesModal(): void {
    this.coursesModalVisible.set(false);
    this.userCourses.set([]);
    this.userCoursesError.set('');
  }

  openCourseForm(): void {
    this.editingCourseId.set(null);
    this.courseForm.reset({ credit: 3, isMandatory: true, classYear: null, instructorId: null });
    this.courseFormError.set('');
    this.courseFormSuccess.set('');
    this.courseFormVisible.set(true);
  }

  editCourse(course: CourseDto): void {
    this.editingCourseId.set(course.id);
    this.courseForm.reset({
      code: course.code,
      name: course.name,
      semester: course.semester,
      credit: course.credit,
      classYear: course.classYear,
      isMandatory: course.isMandatory,
      instructorId: course.instructorId,
    });
    this.courseFormError.set('');
    this.courseFormSuccess.set('');
    this.courseFormVisible.set(true);
  }

  cancelCourseForm(): void {
    this.courseFormVisible.set(false);
    this.editingCourseId.set(null);
    this.courseFormError.set('');
  }

  submitCourseForm(): void {
    if (this.courseForm.invalid) { this.courseForm.markAllAsTouched(); return; }

    this.courseFormLoading.set(true);
    this.courseFormError.set('');

    const raw = this.courseForm.getRawValue();
    const payload = {
      code: raw.code!,
      name: raw.name!,
      semester: raw.semester!,
      credit: raw.credit!,
      isMandatory: raw.isMandatory!,
      classYear: Number(raw.classYear!),
      instructorId: raw.instructorId ? Number(raw.instructorId) : null,
    };

    const editingId = this.editingCourseId();
    if (editingId !== null) {
      this.adminService.updateCourse(editingId, payload as UpdateCourseRequest).subscribe({
        next: (course) => {
          this.courseFormLoading.set(false);
          this.courseFormSuccess.set(`"${course.name}" dersi başarıyla güncellendi.`);
          this.courses.update((list) =>
            list.map((c) => (c.id === course.id ? course : c)).sort((a, b) => a.code.localeCompare(b.code)));
          this.courseFormVisible.set(false);
          this.editingCourseId.set(null);
        },
        error: (err) => {
          this.courseFormLoading.set(false);
          this.courseFormError.set(extractErrorMessage(err, 'Ders güncellenirken bir hata oluştu, lütfen tekrar deneyin.'));
        },
      });
      return;
    }

    this.adminService.createCourse(payload as CreateCourseRequest).subscribe({
      next: (course) => {
        this.courseFormLoading.set(false);
        this.courseFormSuccess.set(`"${course.name}" dersi başarıyla tanımlandı.`);
        this.courses.update((list) => [...list, course].sort((a, b) => a.code.localeCompare(b.code)));
        this.courseFormVisible.set(false);
      },
      error: (err) => {
        this.courseFormLoading.set(false);
        this.courseFormError.set(extractErrorMessage(err, 'Ders tanımlanırken bir hata oluştu, lütfen tekrar deneyin.'));
      },
    });
  }

  deleteCourse(course: CourseDto): void {
    if (!confirm(`"${course.code} – ${course.name}" dersini silmek istiyor musunuz? Bu işlem geri alınamaz.`)) return;

    this.deletingCourseId.set(course.id);
    this.courseFormError.set('');
    this.adminService.deleteCourse(course.id).subscribe({
      next: () => {
        this.deletingCourseId.set(null);
        this.courses.update((list) => list.filter((c) => c.id !== course.id));
        this.courseFormSuccess.set(`"${course.name}" dersi silindi.`);
      },
      error: (err) => {
        this.deletingCourseId.set(null);
        this.courseFormSuccess.set('');
        this.courseFormError.set(extractErrorMessage(err, 'Ders silinemedi, lütfen tekrar deneyin.'));
      },
    });
  }

  loadProgramOutcomes(): void {
    this.outcomesLoading.set(true);
    this.instructorService.getProgramOutcomes().subscribe({
      next: (pos) => { this.programOutcomes.set(pos); this.outcomesLoading.set(false); },
      error: () => this.outcomesLoading.set(false),
    });
  }

  openOutcomeForm(): void {
    this.editingOutcomeId.set(null);
    this.outcomeForm.reset();
    this.outcomeFormError.set('');
    this.outcomeFormSuccess.set('');
    this.outcomeFormVisible.set(true);
  }

  editOutcome(po: ProgramOutcomeDto): void {
    this.editingOutcomeId.set(po.id);
    this.outcomeForm.reset({ code: po.code, description: po.description, details: po.details ?? '' });
    this.outcomeFormError.set('');
    this.outcomeFormSuccess.set('');
    this.outcomeFormVisible.set(true);
  }

  cancelOutcomeForm(): void {
    this.outcomeFormVisible.set(false);
    this.editingOutcomeId.set(null);
    this.outcomeFormError.set('');
  }

  submitOutcomeForm(): void {
    if (this.outcomeForm.invalid) { this.outcomeForm.markAllAsTouched(); return; }
    this.outcomeFormLoading.set(true);
    this.outcomeFormError.set('');
    const raw = this.outcomeForm.getRawValue();

    const editingId = this.editingOutcomeId();
    if (editingId !== null) {
      // Grup bilgisi formda yer almadığı için mevcut değeri koru.
      const existing = this.programOutcomes().find(p => p.id === editingId);
      const req: UpdateProgramOutcomeRequest = {
        code: raw.code!,
        description: raw.description!,
        details: raw.details || null,
        groupId: existing?.groupId ?? null,
      };
      this.instructorService.updateProgramOutcome(editingId, req).subscribe({
        next: (po) => {
          this.outcomeFormLoading.set(false);
          this.outcomeFormSuccess.set(`"${po.code}" program çıktısı güncellendi.`);
          this.programOutcomes.update(list =>
            list.map(p => (p.id === po.id ? po : p)).sort((a, b) => a.code.localeCompare(b.code)));
          this.outcomeFormVisible.set(false);
          this.editingOutcomeId.set(null);
        },
        error: (err) => {
          this.outcomeFormLoading.set(false);
          this.outcomeFormError.set(extractErrorMessage(err, 'Program çıktısı güncellenirken bir hata oluştu, lütfen tekrar deneyin.'));
        },
      });
      return;
    }

    const req: SaveProgramOutcomeRequest = { code: raw.code!, description: raw.description!, details: raw.details || null, groupId: null };
    this.instructorService.addProgramOutcome(req).subscribe({
      next: (po) => {
        this.outcomeFormLoading.set(false);
        this.outcomeFormSuccess.set(`"${po.code}" program çıktısı eklendi.`);
        this.programOutcomes.update(list => [...list, po].sort((a, b) => a.code.localeCompare(b.code)));
        this.outcomeFormVisible.set(false);
      },
      error: (err) => {
        this.outcomeFormLoading.set(false);
        this.outcomeFormError.set(extractErrorMessage(err, 'Program çıktısı eklenirken bir hata oluştu, lütfen tekrar deneyin.'));
      },
    });
  }

  deleteProgramOutcome(id: number, code: string): void {
    if (!confirm(`"${code}" program çıktısını silmek istiyor musunuz? Bu işlem geri alınamaz.`)) return;
    this.instructorService.deleteProgramOutcome(id).subscribe({
      next: () => {
        this.programOutcomes.update(list => list.filter(p => p.id !== id));
        this.outcomeFormSuccess.set(`"${code}" silindi.`);
      },
    });
  }

  // ── Onay Yönetimi metotları ───────────────────────────────────────────────

  loadApprovals(): void {
    this.approvalsLoading.set(true);
    this.approvalsError.set('');
    this.adminService.getApprovals().subscribe({
      next: (list) => { this.approvals.set(list); this.approvalsLoading.set(false); },
      error: () => { this.approvalsLoading.set(false); this.approvalsError.set('Liste yüklenirken bir hata oluştu.'); },
    });
  }

  approveCourse(courseId: number, courseName: string): void {
    if (!confirm(`"${courseName}" dersini onaylamak istiyor musunuz?`)) return;
    this.adminService.approveCourseContent(courseId).subscribe({
      next: () => {
        this.approvals.update(list => list.filter(a => a.courseId !== courseId));
        this.approvalsSuccess.set(`"${courseName}" başarıyla onaylandı.`);
        setTimeout(() => this.approvalsSuccess.set(''), 3500);
      },
      error: () => this.approvalsError.set('Onaylama işlemi başarısız. Lütfen tekrar deneyin.'),
    });
  }

  openRevisionModal(item: ApprovalListItemDto): void {
    this.revisionTargetId.set(item.courseId);
    this.revisionTargetName.set(`${item.courseCode} – ${item.courseName}`);
    this.revisionForm.reset();
    this.revisionModalVisible.set(true);
  }

  submitRevisionRequest(): void {
    if (this.revisionForm.invalid) { this.revisionForm.markAllAsTouched(); return; }
    const id = this.revisionTargetId();
    if (!id) return;
    this.revisionLoading.set(true);
    const note = this.revisionForm.getRawValue().note!;
    this.adminService.requestCourseRevision(id, note).subscribe({
      next: () => {
        this.revisionLoading.set(false);
        this.approvals.update(list => list.filter(a => a.courseId !== id));
        this.closeRevisionModal();
        this.approvalsSuccess.set('Revizyon isteği öğretim üyesine iletildi.');
        setTimeout(() => this.approvalsSuccess.set(''), 3500);
      },
      error: () => {
        this.revisionLoading.set(false);
        this.approvalsError.set('Revizyon isteği gönderilemedi. Lütfen tekrar deneyin.');
      },
    });
  }

  closeRevisionModal(): void {
    this.revisionModalVisible.set(false);
    this.revisionTargetId.set(null);
    this.revisionTargetName.set('');
    this.revisionForm.reset();
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleString('tr-TR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
