import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DecimalPipe, SlicePipe } from '@angular/common';
import { InstructorService } from '../../core/services/instructor.service';
import {
  CourseDetailDto, StudentCourseResultDto, StudentComponentScoreDto, LearningOutcomeDto,
  ExamDto, ExamDetailDto, ExamQuestionDto, SaveExamRequest, SaveExamQuestionRequest,
  ExamGradeEntryDto, SaveExamGradeEntryRequest,
  AssessmentComponentDto, SaveAssessmentComponentRequest,
  ComponentGradeEntryDto, SaveComponentGradeEntryRequest,
  RiskAnalysisDto, CourseStatisticsDto,
  LearningOutcomeStatusDto, ComponentReportItemDto,
  LearningOutcomeWeight,
  AttendanceMatrixDto, AttendanceStatus, SaveAttendanceRequest
} from '../../core/models/course.models';

type Tab = 'info' | 'students' | 'exams' | 'components' | 'attendance' | 'risk' | 'outcomes' | 'reports';

interface GradeRow {
  studentId: number;
  studentNumber: string;
  fullName: string;
  scores: Record<number, number | null>; // questionId -> score
}

interface QuestionRow {
  questionNumber: number;
  description: string;
  score: number | null;
  difficulty: string;
  bookletA: number | null;
  bookletB: number | null;
  bookletC: number | null;
  bookletD: number | null;
  loWeights: { loId: number; weight: number }[];
}

@Component({
  selector: 'app-term-course-detail',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, DecimalPipe, SlicePipe],
  templateUrl: './term-course-detail.html',
  styleUrl: './term-course-detail.css',
})
export class TermCourseDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private svc = inject(InstructorService);
  private fb = inject(FormBuilder);

  courseId = signal(0);
  course = signal<CourseDetailDto | null>(null);
  loading = signal(true);
  activeTab = signal<Tab>('info');

  // Students
  students = signal<StudentCourseResultDto[]>([]);
  studentsLoading = signal(false);

  // Learning Outcomes (for exam question LO selection)
  learningOutcomes = signal<LearningOutcomeDto[]>([]);
  learningOutcomesLoaded = signal(false);

  // Exams
  exams = signal<ExamDto[]>([]);
  examFormVisible = signal(false);
  editingExamId = signal<number | null>(null);
  examSaving = signal(false);
  examLoadError = signal<string | null>(null);
  examValidationErrors = signal<string[]>([]);

  examTypes = ['Vize', 'Final', 'Bütünleme', 'Quiz'];
  examMethods = ['Klasik', 'Test', 'Karma'];
  difficulties = ['Kolay', 'Orta', 'Zor'];

  examForm = this.fb.group({
    examType: ['', Validators.required],
    examMethod: ['', Validators.required],
    date: [''],
    questionCount: [null as number | null, [Validators.min(1), Validators.max(200)]],
    description: [''],
    weightPercentage: [null as number | null, [Validators.min(0), Validators.max(100)]],
  });

  questionRows = signal<QuestionRow[]>([]);

  get totalScore(): number {
    return this.questionRows().reduce((sum, r) => sum + (Number(r.score) || 0), 0);
  }

  get totalScoreClass(): string {
    const t = this.totalScore;
    if (t === 100) return 'score-ok';
    if (t > 100) return 'score-over';
    return 'score-under';
  }

  get difficultyCounts(): { kolay: number; orta: number; zor: number } {
    const rows = this.questionRows();
    return {
      kolay: rows.filter(r => r.difficulty === 'Kolay').length,
      orta:  rows.filter(r => r.difficulty === 'Orta').length,
      zor:   rows.filter(r => r.difficulty === 'Zor').length,
    };
  }

  // Grade Entry
  gradeEntryVisible = signal(false);
  gradeEntryLoading = signal(false);
  gradeEntrySaving = signal(false);
  gradeEntryData = signal<ExamGradeEntryDto | null>(null);
  gradeEntryError = signal<string | null>(null);
  gradeValidationErrors = signal<string[]>([]);
  gradeRows = signal<GradeRow[]>([]);

  // Assessment Components
  components = signal<AssessmentComponentDto[]>([]);
  componentFormVisible = signal(false);
  editingComponentId = signal<number | null>(null);
  componentSaving = signal(false);
  componentTypes = ['Ödev', 'Proje', 'Sunum', 'Laboratuvar', 'Kısa Sınav', 'Katılım'];
  gradeGroups = [
    { value: 'Midterm', label: 'Vize Grubu' },
    { value: 'Final', label: 'Final Grubu' },
    { value: 'Makeup', label: 'Bütünleme Grubu' },
  ];
  loWeights = signal<{ loId: number; weight: number }[]>([]);

  readonly availableCompLOs = computed(() => {
    const selected = this.loWeights().map(w => w.loId);
    return this.learningOutcomes().filter(lo => !selected.includes(lo.id));
  });
  componentForm = this.fb.group({
    name: ['', Validators.required],
    type: ['', Validators.required],
    weight: [0, [Validators.min(0), Validators.max(100)]],
    date: [''],
    description: [''],
    maxScore: [100, [Validators.required, Validators.min(1)]],
    isIncludedInAverage: [false],
    gradeGroup: [''],
    groupWeightPercentage: [0, [Validators.min(0), Validators.max(100)]],
  });

  // Component Grade Entry
  compGradeVisible = signal(false);
  compGradeLoading = signal(false);
  compGradeSaving = signal(false);
  compGradeData = signal<ComponentGradeEntryDto | null>(null);
  compGradeScores = signal<Record<number, number | null>>({});   // studentId -> score
  compGradeError = signal<string | null>(null);

  // Risk Analysis
  riskData = signal<RiskAnalysisDto[]>([]);
  riskLoading = signal(false);

  // Statistics
  stats = signal<CourseStatisticsDto | null>(null);
  statsLoading = signal(false);

  // LO Status (ÖÇ Durum Tablosu)
  loStatus = signal<LearningOutcomeStatusDto[]>([]);
  loStatusLoading = signal(false);
  expandedLoId = signal<number | null>(null);

  // Component Report (Dönem Sonu)
  componentReport = signal<ComponentReportItemDto[]>([]);
  componentReportLoading = signal(false);

  // Attendance (Devam)
  attendanceData = signal<AttendanceMatrixDto | null>(null);
  attendanceLoading = signal(false);
  attendanceSaving = signal(false);
  attendanceSettingsSaving = signal(false);
  attendanceError = signal<string | null>(null);
  // Lokal düzenleme durumu: studentId -> devamsız hafta numaraları kümesi
  attendanceAbsences = signal<Record<number, Set<number>>>({});
  // Ayar formu (kaydedilene kadar matris sütunlarını etkilemez)
  attendanceTotalWeeks = signal(14);
  attendanceLimitPercent = signal(30);

  // Matris sütunları için hafta dizisi (kaydedilmiş hafta sayısına göre)
  readonly attendanceWeeks = computed(() => {
    const n = this.attendanceData()?.totalWeeks ?? 0;
    return Array.from({ length: n }, (_, i) => i + 1);
  });

  // Lokal hücre değişikliklerine göre anlık özet (kaç kaldı / kaç risk)
  readonly attendanceLocalSummary = computed(() => {
    const data = this.attendanceData();
    if (!data) return { totalStudents: 0, failedCount: 0, riskCount: 0 };
    const absences = this.attendanceAbsences();
    let failedCount = 0, riskCount = 0;
    for (const s of data.students) {
      const count = absences[s.studentId]?.size ?? 0;
      const status = this.computeAttendanceStatus(count, data.totalWeeks, data.limitPercent);
      if (status === 'Failed') failedCount++;
      else if (status === 'Risk') riskCount++;
    }
    return { totalStudents: data.students.length, failedCount, riskCount };
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('courseId'));
    this.courseId.set(id);
    this.loadCourse();
  }

  loadCourse(): void {
    this.loading.set(true);
    this.svc.getTermCourseDetail(this.courseId()).subscribe({
      next: (c) => { this.course.set(c); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  setTab(tab: Tab): void {
    this.activeTab.set(tab);
    if (tab === 'students') {
      if (this.students().length === 0) this.loadStudents();
      if (this.components().length === 0) this.loadComponents();
    }
    if (tab === 'exams') {
      if (this.exams().length === 0) this.loadExams();
      if (!this.learningOutcomesLoaded()) this.loadLearningOutcomes();
      if (this.components().length === 0) this.loadComponents();
    }
    if (tab === 'components') {
      if (this.components().length === 0) this.loadComponents();
      if (!this.learningOutcomesLoaded()) this.loadLearningOutcomes();
    }
    if (tab === 'attendance' && this.attendanceData() === null) this.loadAttendance();
    if (tab === 'risk' && this.riskData().length === 0) this.loadRisk();
    if (tab === 'outcomes' && this.loStatus().length === 0) this.loadLoStatus();
    if (tab === 'reports') {
      if (this.stats() === null) this.loadStatistics();
      if (this.componentReport().length === 0) this.loadComponentReport();
      if (!this.learningOutcomesLoaded()) this.loadLearningOutcomes();
    }
  }

  // ── Students ──────────────────────────────────────────────────────────────

  loadStudents(): void {
    this.studentsLoading.set(true);
    this.svc.getStudents(this.courseId()).subscribe({
      next: (s) => { this.students.set(s); this.studentsLoading.set(false); },
      error: () => this.studentsLoading.set(false),
    });
  }

  // ── Learning Outcomes ─────────────────────────────────────────────────────

  loadLearningOutcomes(): void {
    this.svc.getLearningOutcomes(this.courseId()).subscribe({
      next: (los) => { this.learningOutcomes.set(los); this.learningOutcomesLoaded.set(true); },
      error: () => this.learningOutcomesLoaded.set(true),
    });
  }

  // ── Exams ─────────────────────────────────────────────────────────────────

  loadExams(): void {
    this.svc.getExams(this.courseId()).subscribe({
      next: (e) => this.exams.set(e),
      error: () => {},
    });
  }

  openExamForm(exam?: ExamDto): void {
    this.examLoadError.set(null);
    this.examValidationErrors.set([]);
    if (exam) {
      this.editingExamId.set(exam.id);
      this.examForm.reset({
        examType: exam.examType,
        examMethod: exam.examMethod,
        date: exam.date ? exam.date.substring(0, 10) : '',
        questionCount: exam.questionCount ?? null,
        description: exam.description ?? '',
        weightPercentage: exam.weightPercentage ?? null,
      });
      this.examFormVisible.set(true);
      this.svc.getExamDetail(this.courseId(), exam.id).subscribe({
        next: (detail) => {
          this.questionRows.set(detail.questions.map(q => this.questionDtoToRow(q)));
        },
        error: () => this.questionRows.set([]),
      });
    } else {
      this.editingExamId.set(null);
      this.examForm.reset({ examType: '', examMethod: '', date: '', questionCount: null, description: '', weightPercentage: null });
      this.questionRows.set([]);
      this.examFormVisible.set(true);
    }
  }

  cancelExamForm(): void {
    this.examFormVisible.set(false);
    this.editingExamId.set(null);
    this.questionRows.set([]);
    this.examLoadError.set(null);
    this.examValidationErrors.set([]);
  }

  onQuestionCountChange(): void {
    const count = this.examForm.get('questionCount')?.value;
    if (!count || count < 1) return;
    const current = this.questionRows();
    if (count > current.length) {
      const toAdd = count - current.length;
      const newRows: QuestionRow[] = Array.from({ length: toAdd }, (_, i) => ({
        questionNumber: current.length + i + 1,
        description: '',
        score: null,
        difficulty: '',
        bookletA: current.length + i + 1,
        bookletB: null,
        bookletC: null,
        bookletD: null,
        loWeights: [],
      }));
      this.questionRows.set([...current, ...newRows]);
    } else if (count < current.length) {
      if (confirm(`Son ${current.length - count} soru silinecek. Devam edilsin mi?`)) {
        this.questionRows.set(current.slice(0, count));
      } else {
        this.examForm.get('questionCount')?.setValue(current.length);
      }
    }
  }

  addQuestionRow(): void {
    const rows = this.questionRows();
    const newRow: QuestionRow = {
      questionNumber: rows.length + 1,
      description: '',
      score: null,
      difficulty: '',
      bookletA: rows.length + 1,
      bookletB: null,
      bookletC: null,
      bookletD: null,
      loWeights: [],
    };
    this.questionRows.set([...rows, newRow]);
    this.examForm.get('questionCount')?.setValue(rows.length + 1);
  }

  removeQuestionRow(index: number): void {
    if (!confirm('Bu soruyu silmek istiyor musunuz?')) return;
    const rows = [...this.questionRows()];
    rows.splice(index, 1);
    rows.forEach((r, i) => { r.questionNumber = i + 1; });
    this.questionRows.set(rows);
    this.examForm.get('questionCount')?.setValue(rows.length || null);
  }

  updateQuestionField(index: number, field: keyof QuestionRow, value: unknown): void {
    const rows = [...this.questionRows()];
    rows[index] = { ...rows[index], [field]: value };
    this.questionRows.set(rows);
  }

  toggleLO(rowIndex: number, loId: number): void {
    const rows = [...this.questionRows()];
    const row = { ...rows[rowIndex] };
    const loWeights = [...row.loWeights];
    const idx = loWeights.findIndex(w => w.loId === loId);
    if (idx >= 0) {
      loWeights.splice(idx, 1);
      if (loWeights.length === 1) loWeights[0] = { ...loWeights[0], weight: 100 };
    } else {
      const defaultWeight = loWeights.length === 0 ? 100 : 0;
      loWeights.push({ loId, weight: defaultWeight });
    }
    row.loWeights = loWeights;
    rows[rowIndex] = row;
    this.questionRows.set(rows);
  }

  isLOSelected(rowIndex: number, loId: number): boolean {
    return this.questionRows()[rowIndex]?.loWeights.some(w => w.loId === loId) ?? false;
  }

  updateQuestionLoWeight(rowIndex: number, loId: number, weight: number): void {
    const rows = [...this.questionRows()];
    const row = { ...rows[rowIndex] };
    row.loWeights = row.loWeights.map(w => w.loId === loId ? { ...w, weight } : w);
    rows[rowIndex] = row;
    this.questionRows.set(rows);
  }

  getQuestionLoWeightTotal(rowIndex: number): number {
    const row = this.questionRows()[rowIndex];
    if (!row) return 0;
    return Math.round(row.loWeights.reduce((sum, w) => sum + (w.weight || 0), 0) * 100) / 100;
  }

  getAvailableLOsForQuestion(rowIndex: number): LearningOutcomeDto[] {
    const selected = this.questionRows()[rowIndex]?.loWeights.map(w => w.loId) ?? [];
    return this.learningOutcomes().filter(lo => !selected.includes(lo.id));
  }

  private validateQuestions(): string[] {
    const errors: string[] = [];
    const rows = this.questionRows();

    if (rows.length === 0) return errors;

    rows.forEach((r, i) => {
      const num = i + 1;
      if (!r.description?.trim()) errors.push(`${num}. soru için açıklama girilmelidir.`);
      if (r.score === null || r.score === undefined || isNaN(Number(r.score))) {
        errors.push(`${num}. soru için puan girilmelidir.`);
      }
      if (!r.difficulty) errors.push(`${num}. soru için zorluk seviyesi seçilmelidir.`);
      if (r.loWeights.length === 0 && this.learningOutcomes().length > 0) {
        errors.push(`Soru ${num} için en az bir Öğrenme Çıktısı seçilmelidir.`);
      } else if (r.loWeights.length > 0) {
        const total = Math.round(r.loWeights.reduce((sum, w) => sum + (w.weight || 0), 0) * 100) / 100;
        if (Math.abs(total - 100) > 0.01) {
          errors.push(`Soru ${num} için ÖÇ ağırlıkları toplamı 100 olmalıdır (şu an: ${total}%).`);
        }
      }
    });

    if (rows.length > 0 && this.totalScore !== 100) {
      errors.push(`Toplam puan 100 olmalıdır. Şu an toplam: ${this.totalScore}.`);
    }

    return errors;
  }

  saveExamForm(): void {
    this.examValidationErrors.set([]);
    this.examLoadError.set(null);

    if (this.examForm.invalid) {
      this.examForm.markAllAsTouched();
      return;
    }

    const validationErrors = this.validateQuestions();
    if (validationErrors.length > 0) {
      this.examValidationErrors.set(validationErrors);
      return;
    }

    this.examSaving.set(true);
    const raw = this.examForm.getRawValue();
    const req: SaveExamRequest = {
      examType: raw.examType!,
      examMethod: raw.examMethod!,
      date: raw.date || null,
      questionCount: raw.questionCount ?? null,
      description: raw.description || null,
      weightPercentage: raw.weightPercentage ?? null,
      questions: this.questionRows().map(r => ({
        questionNumber: r.questionNumber,
        description: r.description,
        score: Number(r.score) || 0,
        difficulty: r.difficulty,
        bookletAQuestionNumber: r.bookletA ?? null,
        bookletBQuestionNumber: r.bookletB ?? null,
        bookletCQuestionNumber: r.bookletC ?? null,
        bookletDQuestionNumber: r.bookletD ?? null,
        learningOutcomeWeights: r.loWeights.map(w => ({
          learningOutcomeId: w.loId,
          weightPercentage: w.weight,
        } satisfies LearningOutcomeWeight)),
      } satisfies SaveExamQuestionRequest)),
    };

    const id = this.editingExamId();
    const obs = id
      ? this.svc.updateExam(this.courseId(), id, req)
      : this.svc.addExam(this.courseId(), req);

    obs.subscribe({
      next: (detail) => {
        this.exams.update(es => {
          const existing = id ? es.find(e => e.id === id) : undefined;
          const summaryDto: ExamDto = {
            id: detail.id,
            examType: detail.examType,
            examMethod: detail.examMethod,
            date: detail.date,
            questionCount: detail.questionCount,
            description: detail.description,
            totalScore: detail.totalScore,
            weightPercentage: detail.weightPercentage ?? null,
            hasGrades: existing?.hasGrades ?? false,
            gradedStudentCount: existing?.gradedStudentCount ?? 0,
            totalStudentCount: existing?.totalStudentCount ?? 0,
          };
          if (id) return es.map(e => e.id === id ? summaryDto : e);
          return [...es, summaryDto];
        });
        this.cancelExamForm();
        this.examSaving.set(false);
      },
      error: (err) => {
        this.examLoadError.set(this.getErrorMessage(err));
        this.examSaving.set(false);
      },
    });
  }

  private getErrorMessage(error: unknown): string {
    if (!error) return 'Bilinmeyen bir hata oluştu.';
    if (typeof error === 'string') return error;

    const err = error as Record<string, unknown>;

    if (err['error']) {
      const inner = err['error'];
      if (typeof inner === 'string') return inner;

      const innerObj = inner as Record<string, unknown>;
      if (typeof innerObj['message'] === 'string') return innerObj['message'];
      if (typeof innerObj['status'] === 'number' && (innerObj['status'] as number) >= 500)
        return 'Sunucu hatası oluştu. Lütfen tekrar deneyin.';
      if (typeof innerObj['title'] === 'string') {
        let msg = innerObj['title'] as string;
        if (innerObj['errors'] && typeof innerObj['errors'] === 'object') {
          const vals = Object.values(innerObj['errors'] as Record<string, unknown[]>)
            .flat()
            .filter((v): v is string => typeof v === 'string');
          if (vals.length) msg += ' ' + vals.join(' ');
        }
        return msg;
      }
      if (innerObj['errors'] && typeof innerObj['errors'] === 'object') {
        const vals = Object.values(innerObj['errors'] as Record<string, unknown[]>)
          .flat()
          .filter((v): v is string => typeof v === 'string');
        if (vals.length) return vals.join(' ');
      }
    }

    if (typeof err['message'] === 'string') return err['message'];
    if (typeof err['status'] === 'number') {
      if (err['status'] === 403) return 'Bu işlem için yetkiniz yok veya ders henüz onaylanmamış.';
      if (err['status'] === 404) return 'İlgili kayıt bulunamadı.';
      if (err['status'] === 400) return 'Gönderilen veriler geçersiz. Alanları kontrol edin.';
      if ((err['status'] as number) >= 500) return 'Sunucu hatası oluştu. Lütfen tekrar deneyin.';
    }

    return 'İşlem sırasında bir hata oluştu.';
  }

  private questionDtoToRow(q: ExamQuestionDto): QuestionRow {
    return {
      questionNumber: q.questionNumber,
      description: q.description,
      score: q.score,
      difficulty: q.difficulty,
      bookletA: q.bookletAQuestionNumber,
      bookletB: q.bookletBQuestionNumber,
      bookletC: q.bookletCQuestionNumber,
      bookletD: q.bookletDQuestionNumber,
      loWeights: q.learningOutcomeWeights.map(w => ({ loId: w.learningOutcomeId, weight: w.weightPercentage })),
    };
  }

  deleteExam(id: number): void {
    if (!confirm('Bu sınavı silmek istiyor musunuz?')) return;
    this.svc.deleteExam(this.courseId(), id).subscribe(() =>
      this.exams.update(es => es.filter(e => e.id !== id)));
  }

  // ── Grade Entry ───────────────────────────────────────────────────────────

  openGradeEntry(exam: ExamDto): void {
    this.gradeEntryVisible.set(true);
    this.gradeEntryLoading.set(true);
    this.gradeEntryData.set(null);
    this.gradeRows.set([]);
    this.gradeEntryError.set(null);
    this.gradeValidationErrors.set([]);

    this.svc.getExamGradeEntry(this.courseId(), exam.id).subscribe({
      next: (data) => {
        this.gradeEntryData.set(data);
        this.gradeRows.set(data.students.map(s => ({
          studentId: s.studentId,
          studentNumber: s.studentNumber,
          fullName: s.fullName,
          scores: data.questions.reduce<Record<number, number | null>>((acc, q) => {
            const existing = s.questionScores.find(qs => qs.questionId === q.id);
            acc[q.id] = existing?.score ?? null;
            return acc;
          }, {}),
        })));
        this.gradeEntryLoading.set(false);
      },
      error: (err) => {
        this.gradeEntryError.set(this.getErrorMessage(err));
        this.gradeEntryLoading.set(false);
      },
    });
  }

  closeGradeEntry(): void {
    this.gradeEntryVisible.set(false);
    this.gradeEntryData.set(null);
    this.gradeRows.set([]);
    this.gradeEntryError.set(null);
    this.gradeValidationErrors.set([]);
  }

  updateGradeScore(studentIdx: number, questionId: number, value: string): void {
    const rows = [...this.gradeRows()];
    const row = { ...rows[studentIdx] };
    row.scores = { ...row.scores, [questionId]: value === '' ? null : Number(value) };
    rows[studentIdx] = row;
    this.gradeRows.set(rows);
  }

  getRowTotal(studentIdx: number): number {
    const row = this.gradeRows()[studentIdx];
    if (!row) return 0;
    return Object.values(row.scores).reduce((sum: number, s) => sum + (Number(s) || 0), 0);
  }

  isRowComplete(studentIdx: number): boolean {
    const row = this.gradeRows()[studentIdx];
    const data = this.gradeEntryData();
    if (!row || !data || data.questions.length === 0) return false;
    return data.questions.every(q => typeof row.scores[q.id] === 'number');
  }

  isScoreOver(score: number | null | undefined, maxScore: number): boolean {
    return score !== null && score !== undefined && Number(score) > maxScore;
  }

  getRowTotalClass(studentIdx: number): string {
    const total = this.getRowTotal(studentIdx);
    if (total >= 70) return 'grade-pass';
    if (total >= 50) return 'grade-mid';
    if (total > 0) return 'grade-fail';
    return '';
  }

  private validateGrades(): string[] {
    const errors: string[] = [];
    const data = this.gradeEntryData();
    if (!data) return ['Not girişi verisi yüklenemedi.'];

    this.gradeRows().forEach((row) => {
      data.questions.forEach((q) => {
        const score = row.scores[q.id];
        if (score !== null && score !== undefined) {
          if (Number(score) < 0)
            errors.push(`${row.fullName} için ${q.questionNumber}. soru puanı negatif olamaz.`);
          else if (Number(score) > q.maxScore)
            errors.push(`${row.fullName} için ${q.questionNumber}. soru puanı ${q.maxScore} puanı geçemez.`);
        }
      });
    });

    return errors;
  }

  saveGradeEntry(): void {
    this.gradeValidationErrors.set([]);
    this.gradeEntryError.set(null);

    const data = this.gradeEntryData();
    if (!data) return;

    const errors = this.validateGrades();
    if (errors.length > 0) {
      this.gradeValidationErrors.set(errors);
      return;
    }

    this.gradeEntrySaving.set(true);
    const req: SaveExamGradeEntryRequest = {
      students: this.gradeRows().map(row => ({
        studentId: row.studentId,
        questionScores: data.questions.map(q => ({
          questionId: q.id,
          score: Number(row.scores[q.id]) || 0,
        })),
      })),
    };

    this.svc.saveExamGradeEntry(this.courseId(), data.examId, req).subscribe({
      next: (result) => {
        this.exams.update(es => es.map(e =>
          e.id === data.examId
            ? { ...e, hasGrades: true, gradedStudentCount: result.gradedStudentCount }
            : e
        ));
        // Öğrenci listesi zaten yüklenmişse güncel notları çek
        if (this.students().length > 0) this.loadStudents();
        this.gradeEntrySaving.set(false);
        this.closeGradeEntry();
      },
      error: (err) => {
        this.gradeEntryError.set(this.getErrorMessage(err));
        this.gradeEntrySaving.set(false);
      },
    });
  }

  onPlaceholderAction(label: string): void {
    alert(`"${label}" özelliği daha sonra eklenecektir.`);
  }

  // ── Assessment Components ─────────────────────────────────────────────────

  loadComponents(): void {
    this.svc.getAssessmentComponents(this.courseId()).subscribe(c => this.components.set(c));
  }

  get totalWeight(): number {
    return this.components().reduce((sum, c) => sum + Number(c.weight), 0);
  }

  openComponentForm(component?: AssessmentComponentDto): void {
    this.editingComponentId.set(component?.id ?? null);
    this.loWeights.set(
      (component?.learningOutcomeWeights ?? []).map(w => ({ loId: w.learningOutcomeId, weight: w.weightPercentage }))
    );
    this.componentForm.reset({
      name: component?.name ?? '',
      type: component?.type ?? '',
      weight: component?.weight ?? 0,
      date: component?.date ? component.date.substring(0, 10) : '',
      description: component?.description ?? '',
      maxScore: component?.maxScore ?? 100,
      isIncludedInAverage: component?.isIncludedInAverage ?? false,
      gradeGroup: component?.gradeGroup ?? '',
      groupWeightPercentage: component?.groupWeightPercentage ?? 0,
    });
    if (this.learningOutcomes().length === 0) this.loadLearningOutcomes();
    this.componentFormVisible.set(true);
  }

  cancelComponentForm(): void {
    this.componentFormVisible.set(false);
    this.editingComponentId.set(null);
    this.loWeights.set([]);
  }

  addCompLoWeight(loIdStr: string): void {
    const loId = Number(loIdStr);
    if (!loId) return;
    const current = this.loWeights();
    if (current.some(w => w.loId === loId)) return;
    const weight = current.length === 0 ? 100 : 0;
    this.loWeights.set([...current, { loId, weight }]);
  }

  removeCompLoWeight(index: number): void {
    this.loWeights.update(ws => {
      const newWs = ws.filter((_, i) => i !== index);
      if (newWs.length === 1) newWs[0] = { ...newWs[0], weight: 100 };
      return newWs;
    });
  }

  updateCompLoWeight(index: number, weight: number): void {
    this.loWeights.update(ws => ws.map((w, i) => i === index ? { ...w, weight } : w));
  }

  getCompLoWeightTotal(): number {
    return Math.round(this.loWeights().reduce((sum, w) => sum + (w.weight || 0), 0) * 100) / 100;
  }

  getLoDescription(loId: number): string {
    return this.learningOutcomes().find(lo => lo.id === loId)?.description ?? '';
  }

  saveComponentForm(): void {
    if (this.componentForm.invalid) { this.componentForm.markAllAsTouched(); return; }

    const loWeightsArr = this.loWeights();
    const raw = this.componentForm.getRawValue();
    if ((raw.isIncludedInAverage ?? false) && loWeightsArr.length === 0) {
      alert('Ortalamaya dahil edilen ölçme bileşeni en az bir Öğrenme Çıktısı ile eşleştirilmelidir.');
      return;
    }
    if (loWeightsArr.length > 0) {
      const total = this.getCompLoWeightTotal();
      if (Math.abs(total - 100) > 0.01) {
        alert(`ÖÇ ağırlıkları toplamı 100 olmalıdır (şu an: ${total}%).`);
        return;
      }
    }

    this.componentSaving.set(true);
    const req: SaveAssessmentComponentRequest = {
      name: raw.name!,
      type: raw.type!,
      weight: raw.weight ?? 0,
      date: raw.date || null,
      description: raw.description || null,
      maxScore: raw.maxScore ?? 100,
      isIncludedInAverage: raw.isIncludedInAverage ?? false,
      gradeGroup: raw.gradeGroup || null,
      groupWeightPercentage: raw.groupWeightPercentage ?? 0,
      learningOutcomeWeights: loWeightsArr.map(w => ({
        learningOutcomeId: w.loId,
        weightPercentage: w.weight,
      } satisfies LearningOutcomeWeight)),
    };
    const id = this.editingComponentId();
    if (id) {
      this.svc.updateAssessmentComponent(this.courseId(), id, req).subscribe({
        next: () => {
          this.components.update(cs => cs.map(c => c.id === id ? { ...c, ...req } : c));
          this.cancelComponentForm();
          this.componentSaving.set(false);
        },
        error: () => this.componentSaving.set(false),
      });
    } else {
      this.svc.addAssessmentComponent(this.courseId(), req).subscribe({
        next: (c) => { this.components.update(cs => [...cs, c]); this.cancelComponentForm(); this.componentSaving.set(false); },
        error: () => this.componentSaving.set(false),
      });
    }
  }

  deleteComponent(id: number): void {
    if (!confirm('Bu bileşeni silmek istiyor musunuz?')) return;
    this.svc.deleteAssessmentComponent(this.courseId(), id).subscribe(() =>
      this.components.update(cs => cs.filter(c => c.id !== id)));
  }

  // ── Component Grade Entry ─────────────────────────────────────────────────

  openCompGradeEntryFromExams(component: AssessmentComponentDto): void {
    this.setTab('components');
    this.openCompGradeEntry(component);
  }

  openCompGradeEntry(component: AssessmentComponentDto): void {
    this.compGradeVisible.set(true);
    this.compGradeLoading.set(true);
    this.compGradeData.set(null);
    this.compGradeScores.set({});
    this.compGradeError.set(null);

    this.svc.getComponentGradeEntry(this.courseId(), component.id).subscribe({
      next: (data) => {
        this.compGradeData.set(data);
        const scores: Record<number, number | null> = {};
        data.students.forEach(s => { scores[s.studentId] = s.score; });
        this.compGradeScores.set(scores);
        this.compGradeLoading.set(false);
      },
      error: (err) => {
        this.compGradeError.set(this.getErrorMessage(err));
        this.compGradeLoading.set(false);
      },
    });
  }

  closeCompGradeEntry(): void {
    this.compGradeVisible.set(false);
    this.compGradeData.set(null);
    this.compGradeScores.set({});
    this.compGradeError.set(null);
  }

  updateCompScore(studentId: number, value: string): void {
    this.compGradeScores.update(s => ({ ...s, [studentId]: value === '' ? null : Number(value) }));
  }

  isCompScoreOver(studentId: number): boolean {
    const data = this.compGradeData();
    if (!data) return false;
    const score = this.compGradeScores()[studentId];
    return score !== null && score !== undefined && score > data.maxScore;
  }

  saveCompGradeEntry(): void {
    this.compGradeError.set(null);
    const data = this.compGradeData();
    if (!data) return;

    const scores = this.compGradeScores();
    const hasOver = data.students.some(s => this.isCompScoreOver(s.studentId));
    if (hasOver) {
      this.compGradeError.set(`Puan ${data.maxScore} değerini geçemez.`);
      return;
    }

    this.compGradeSaving.set(true);
    const req: SaveComponentGradeEntryRequest = {
      students: data.students.map(s => ({ studentId: s.studentId, score: scores[s.studentId] ?? null })),
    };

    this.svc.saveComponentGradeEntry(this.courseId(), data.componentId, req).subscribe({
      next: () => {
        if (this.students().length > 0) this.loadStudents();
        this.compGradeSaving.set(false);
        this.closeCompGradeEntry();
      },
      error: (err) => {
        this.compGradeError.set(this.getErrorMessage(err));
        this.compGradeSaving.set(false);
      },
    });
  }

  // ── Risk ─────────────────────────────────────────────────────────────────

  loadRisk(): void {
    this.riskLoading.set(true);
    this.svc.getRiskAnalysis(this.courseId()).subscribe({
      next: (r) => { this.riskData.set(r); this.riskLoading.set(false); },
      error: () => this.riskLoading.set(false),
    });
  }

  riskClass(level: string): string {
    const map: Record<string, string> = { 'Yüksek': 'risk-high', 'Orta': 'risk-mid', 'Düşük': 'risk-low' };
    return map[level] ?? '';
  }

  getLoCode(loId: number): string {
    return this.learningOutcomes().find(lo => lo.id === loId)?.code ?? `ÖÇ${loId}`;
  }

  getComponentScore(student: StudentCourseResultDto, componentId: number): number | null {
    return student.componentScores?.find(cs => cs.componentId === componentId)?.score ?? null;
  }

  gradeClass(grade: number): string {
    if (grade >= 70) return 'grade-pass';
    if (grade >= 50) return 'grade-mid';
    return 'grade-fail';
  }

  // ── Statistics ─────────────────────────────────────────────────────────────

  loadStatistics(): void {
    this.statsLoading.set(true);
    this.svc.getStatistics(this.courseId()).subscribe({
      next: (s) => { this.stats.set(s); this.statsLoading.set(false); },
      error: () => this.statsLoading.set(false),
    });
  }

  bucketPercent(count: number): number {
    const max = Math.max(...this.stats()!.distribution.map(d => d.count), 1);
    return Math.round((count / max) * 100);
  }

  // ── LO Status ─────────────────────────────────────────────────────────────

  loadLoStatus(): void {
    this.loStatusLoading.set(true);
    this.svc.getLearningOutcomeStatus(this.courseId()).subscribe({
      next: (data) => { this.loStatus.set(data); this.loStatusLoading.set(false); },
      error: () => this.loStatusLoading.set(false),
    });
  }

  toggleLoExpand(loId: number): void {
    this.expandedLoId.update(curr => curr === loId ? null : loId);
  }

  successClass(pct: number | null): string {
    if (pct === null) return 'grade-empty';
    if (pct >= 70) return 'grade-pass';
    if (pct >= 50) return 'grade-mid';
    return 'grade-fail';
  }

  // Tabloyu CSV olarak indir (TR Excel uyumu: ; ayraç, ondalık virgül, UTF-8 BOM)
  downloadLoStatusCsv(): void {
    const rows = this.loStatus();
    if (rows.length === 0) return;

    const D = ';';
    const esc = (v: string) => `"${(v ?? '').replace(/"/g, '""')}"`;
    const num = (n: number | null | undefined) =>
      (n === null || n === undefined) ? '-' : String(n).replace('.', ',');

    const headers = ['ÖÇ Kodu', 'Öğrenme Çıktısı', 'Ölçme Sayısı', 'Ölçme Ağırlığı (%)', 'Bu Dönem (%)'];

    const lines = [headers.map(esc).join(D)];
    for (const lo of rows) {
      lines.push([
        esc(lo.code),
        esc(lo.description),
        lo.measurementCount > 0 ? String(lo.measurementCount) : '-',
        num(lo.measurementWeightPercentage),
        num(lo.averageSuccess),
      ].join(D));
    }

    const bom = String.fromCharCode(0xFEFF);   // Excel'in UTF-8'i tanıması için
    const csv = bom + lines.join('\r\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `oc-durum-tablosu-${this.course()?.code ?? 'ders'}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  // ── Component Report ───────────────────────────────────────────────────────

  loadComponentReport(): void {
    this.componentReportLoading.set(true);
    this.svc.getComponentReport(this.courseId()).subscribe({
      next: (data) => { this.componentReport.set(data); this.componentReportLoading.set(false); },
      error: () => this.componentReportLoading.set(false),
    });
  }

  gradeGroupLabel(gradeGroup: string | null): string {
    if (gradeGroup === 'Midterm') return 'Vize';
    if (gradeGroup === 'Final') return 'Final';
    if (gradeGroup === 'Makeup') return 'Bütünleme';
    return '—';
  }

  // ── Attendance (Devam) ─────────────────────────────────────────────────────

  loadAttendance(): void {
    this.attendanceLoading.set(true);
    this.attendanceError.set(null);
    this.svc.getAttendance(this.courseId()).subscribe({
      next: (data) => {
        this.attendanceData.set(data);
        const map: Record<number, Set<number>> = {};
        data.students.forEach(s => { map[s.studentId] = new Set(s.absentWeeks); });
        this.attendanceAbsences.set(map);
        this.attendanceTotalWeeks.set(data.totalWeeks);
        this.attendanceLimitPercent.set(data.limitPercent);
        this.attendanceLoading.set(false);
      },
      error: (err) => {
        this.attendanceError.set(this.getErrorMessage(err));
        this.attendanceLoading.set(false);
      },
    });
  }

  isAbsent(studentId: number, week: number): boolean {
    return this.attendanceAbsences()[studentId]?.has(week) ?? false;
  }

  toggleAbsent(studentId: number, week: number): void {
    this.attendanceAbsences.update(map => {
      const next = { ...map };
      const set = new Set(next[studentId] ?? []);
      if (set.has(week)) set.delete(week); else set.add(week);
      next[studentId] = set;
      return next;
    });
  }

  getAbsentCount(studentId: number): number {
    return this.attendanceAbsences()[studentId]?.size ?? 0;
  }

  // Backend AttendanceCalculator ile aynı kural — anlık geri bildirim için.
  private computeAbsenceRate(absentCount: number, totalWeeks: number): number {
    if (totalWeeks <= 0) return 0;
    const clamped = Math.min(Math.max(absentCount, 0), totalWeeks);
    return Math.round((clamped / totalWeeks) * 100 * 10) / 10;
  }

  private computeAttendanceStatus(absentCount: number, totalWeeks: number, limitPercent: number): AttendanceStatus {
    if (totalWeeks <= 0) return 'Safe';
    const rate = this.computeAbsenceRate(absentCount, totalWeeks);
    if (rate > limitPercent) return 'Failed';
    if (rate >= limitPercent * 0.8) return 'Risk';
    return 'Safe';
  }

  getAbsenceRate(studentId: number): number {
    const data = this.attendanceData();
    if (!data) return 0;
    return this.computeAbsenceRate(this.getAbsentCount(studentId), data.totalWeeks);
  }

  getAttendanceStatus(studentId: number): AttendanceStatus {
    const data = this.attendanceData();
    if (!data) return 'Safe';
    return this.computeAttendanceStatus(this.getAbsentCount(studentId), data.totalWeeks, data.limitPercent);
  }

  attendanceStatusLabel(status: AttendanceStatus): string {
    if (status === 'Failed') return 'Devamsızlıktan Kaldı';
    if (status === 'Risk') return 'Risk';
    return 'Devam Ediyor';
  }

  attendanceStatusClass(status: AttendanceStatus): string {
    const map: Record<AttendanceStatus, string> = {
      Failed: 'att-failed', Risk: 'att-risk', Safe: 'att-safe',
    };
    return map[status];
  }

  setAttendanceTotalWeeks(value: string): void {
    this.attendanceTotalWeeks.set(Number(value) || 0);
  }

  setAttendanceLimitPercent(value: string): void {
    this.attendanceLimitPercent.set(Number(value) || 0);
  }

  saveAttendanceSettings(): void {
    this.attendanceError.set(null);
    const totalWeeks = this.attendanceTotalWeeks();
    const limitPercent = this.attendanceLimitPercent();
    if (totalWeeks < 1 || totalWeeks > 30) {
      this.attendanceError.set('Toplam hafta sayısı 1 ile 30 arasında olmalıdır.');
      return;
    }
    if (limitPercent < 0 || limitPercent > 100) {
      this.attendanceError.set('Devamsızlık sınırı 0 ile 100 arasında olmalıdır.');
      return;
    }
    this.attendanceSettingsSaving.set(true);
    this.svc.saveAttendanceSettings(this.courseId(), { totalWeeks, limitPercent }).subscribe({
      next: () => { this.attendanceSettingsSaving.set(false); this.loadAttendance(); },
      error: (err) => {
        this.attendanceError.set(this.getErrorMessage(err));
        this.attendanceSettingsSaving.set(false);
      },
    });
  }

  saveAttendance(): void {
    const data = this.attendanceData();
    if (!data) return;
    this.attendanceError.set(null);
    this.attendanceSaving.set(true);

    const absences = this.attendanceAbsences();
    const req: SaveAttendanceRequest = {
      students: data.students.map(s => ({
        studentId: s.studentId,
        absentWeeks: Array.from(absences[s.studentId] ?? []).sort((a, b) => a - b),
      })),
    };

    this.svc.saveAttendance(this.courseId(), req).subscribe({
      next: () => { this.attendanceSaving.set(false); this.loadAttendance(); },
      error: (err) => {
        this.attendanceError.set(this.getErrorMessage(err));
        this.attendanceSaving.set(false);
      },
    });
  }

  readonly reportCards = [
    { icon: '📋', title: 'Ders Genel Raporu', desc: 'Dersin genel istatistikleri ve özeti' },
    { icon: '🎯', title: 'Öğrenme Çıktıları Raporu', desc: 'Öğrenme çıktılarının başarı durumu analizi' },
    { icon: '🔗', title: 'Program Çıktıları Katkı Raporu', desc: 'Programın çıktılarına katkı düzeyleri' },
    { icon: '📊', title: 'Başarı Dağılımı Raporu', desc: 'Öğrenci not dağılımı ve istatistikler' },
  ];
}
