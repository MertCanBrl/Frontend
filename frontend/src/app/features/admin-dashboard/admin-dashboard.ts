import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { AdminService } from '../../core/services/admin.service';
import { InstructorService } from '../../core/services/instructor.service';
import { UserDto, CreateUserRequest, CreateCourseRequest, ApprovalListItemDto, AdminCourseContentDto } from '../../core/models/admin.models';
import { ProgramOutcomeDto, SaveProgramOutcomeRequest } from '../../core/models/course.models';

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
  courses = signal<{ id: number; code: string; name: string; semester: string; credit: number; isMandatory: boolean; classYear: number; instructorId: number | null; instructorName: string | null }[]>([]);

  usersLoading = signal(false);
  coursesLoading = signal(false);

  userFormVisible = signal(false);
  courseFormVisible = signal(false);

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
    this.userForm.reset({ title: 'Prof. Dr.' });
    this.userFormError.set('');
    this.userFormSuccess.set('');
    this.userFormVisible.set(true);
  }

  cancelUserForm(): void {
    this.userFormVisible.set(false);
    this.userFormError.set('');
  }

  submitUserForm(): void {
    if (this.userForm.invalid) { this.userForm.markAllAsTouched(); return; }

    this.userFormLoading.set(true);
    this.userFormError.set('');

    const raw = this.userForm.getRawValue();
    const req: CreateUserRequest = { title: raw.title!, fullName: raw.fullName!, email: raw.email! };

    this.adminService.createUser(req).subscribe({
      next: (user) => {
        this.userFormLoading.set(false);
        this.userFormSuccess.set(`${user.fullName} başarıyla tanımlandı. Giriş bilgileri e-posta ile gönderildi.`);
        this.users.update((list) => [...list, user].sort((a, b) => a.fullName.localeCompare(b.fullName)));
        this.userFormVisible.set(false);
      },
      error: (err) => {
        this.userFormLoading.set(false);
        this.userFormError.set(err.status === 409 ? 'Bu e-posta adresi zaten kayıtlı.' : 'Bir hata oluştu, lütfen tekrar deneyin.');
      },
    });
  }

  openCourseForm(): void {
    this.courseForm.reset({ credit: 3, isMandatory: true, classYear: null, instructorId: null });
    this.courseFormError.set('');
    this.courseFormSuccess.set('');
    this.courseFormVisible.set(true);
  }

  cancelCourseForm(): void {
    this.courseFormVisible.set(false);
    this.courseFormError.set('');
  }

  submitCourseForm(): void {
    if (this.courseForm.invalid) { this.courseForm.markAllAsTouched(); return; }

    this.courseFormLoading.set(true);
    this.courseFormError.set('');

    const raw = this.courseForm.getRawValue();
    const req: CreateCourseRequest = {
      code: raw.code!,
      name: raw.name!,
      semester: raw.semester!,
      credit: raw.credit!,
      isMandatory: raw.isMandatory!,
      classYear: Number(raw.classYear!),
      instructorId: raw.instructorId ? Number(raw.instructorId) : null,
    };

    this.adminService.createCourse(req).subscribe({
      next: (course) => {
        this.courseFormLoading.set(false);
        this.courseFormSuccess.set(`"${course.name}" dersi başarıyla tanımlandı.`);
        this.courses.update((list) => [...list, course].sort((a, b) => a.code.localeCompare(b.code)));
        this.courseFormVisible.set(false);
      },
      error: (err) => {
        this.courseFormLoading.set(false);
        this.courseFormError.set(err.status === 409 ? 'Bu ders kodu zaten kullanımda.' : 'Bir hata oluştu, lütfen tekrar deneyin.');
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
    this.outcomeForm.reset();
    this.outcomeFormError.set('');
    this.outcomeFormSuccess.set('');
    this.outcomeFormVisible.set(true);
  }

  cancelOutcomeForm(): void {
    this.outcomeFormVisible.set(false);
    this.outcomeFormError.set('');
  }

  submitOutcomeForm(): void {
    if (this.outcomeForm.invalid) { this.outcomeForm.markAllAsTouched(); return; }
    this.outcomeFormLoading.set(true);
    this.outcomeFormError.set('');
    const raw = this.outcomeForm.getRawValue();
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
        this.outcomeFormError.set(err.status === 409 ? 'Bu kod zaten kullanımda.' : 'Bir hata oluştu, lütfen tekrar deneyin.');
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
