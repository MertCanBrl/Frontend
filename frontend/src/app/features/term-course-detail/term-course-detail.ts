import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DecimalPipe, SlicePipe } from '@angular/common';
import { InstructorService } from '../../core/services/instructor.service';
import {
  CourseDetailDto, StudentCourseResultDto,
  ExamDto, SaveExamRequest,
  AssessmentComponentDto, SaveAssessmentComponentRequest,
  RiskAnalysisDto, CourseStatisticsDto
} from '../../core/models/course.models';

type Tab = 'info' | 'students' | 'exams' | 'components' | 'attendance' | 'risk' | 'outcomes' | 'reports';

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

  // Exams
  exams = signal<ExamDto[]>([]);
  examFormVisible = signal(false);
  editingExamId = signal<number | null>(null);
  examSaving = signal(false);
  examTypes = ['Vize', 'Final', 'Bütünleme', 'Quiz'];
  examMethods = ['Klasik', 'Test', 'Karma'];
  examForm = this.fb.group({
    examType: ['', Validators.required],
    examMethod: ['', Validators.required],
    date: [''],
    questionCount: [null as number | null],
    description: [''],
  });

  // Assessment Components
  components = signal<AssessmentComponentDto[]>([]);
  componentFormVisible = signal(false);
  editingComponentId = signal<number | null>(null);
  componentSaving = signal(false);
  componentTypes = ['Ödev', 'Proje', 'Sunum', 'Laboratuvar', 'Kısa Sınav', 'Katılım'];
  componentForm = this.fb.group({
    name: ['', Validators.required],
    type: ['', Validators.required],
    weight: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
    date: [''],
    description: [''],
  });

  // Risk Analysis
  riskData = signal<RiskAnalysisDto[]>([]);
  riskLoading = signal(false);

  // Statistics (Dönem Sonu Raporları)
  stats = signal<CourseStatisticsDto | null>(null);
  statsLoading = signal(false);

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
    if (tab === 'students' && this.students().length === 0) this.loadStudents();
    if (tab === 'exams' && this.exams().length === 0) this.loadExams();
    if (tab === 'components' && this.components().length === 0) this.loadComponents();
    if (tab === 'risk' && this.riskData().length === 0) this.loadRisk();
    if (tab === 'reports' && this.stats() === null) this.loadStatistics();
  }

  // ── Students ──────────────────────────────────────────────────────────────

  loadStudents(): void {
    this.studentsLoading.set(true);
    this.svc.getStudents(this.courseId()).subscribe({
      next: (s) => { this.students.set(s); this.studentsLoading.set(false); },
      error: () => this.studentsLoading.set(false),
    });
  }

  // ── Exams ─────────────────────────────────────────────────────────────────

  loadExams(): void {
    this.svc.getExams(this.courseId()).subscribe(e => this.exams.set(e));
  }

  openExamForm(exam?: ExamDto): void {
    this.editingExamId.set(exam?.id ?? null);
    this.examForm.reset({
      examType: exam?.examType ?? '',
      examMethod: exam?.examMethod ?? '',
      date: exam?.date ? exam.date.substring(0, 10) : '',
      questionCount: exam?.questionCount ?? null,
      description: exam?.description ?? '',
    });
    this.examFormVisible.set(true);
  }

  cancelExamForm(): void { this.examFormVisible.set(false); this.editingExamId.set(null); }

  saveExamForm(): void {
    if (this.examForm.invalid) { this.examForm.markAllAsTouched(); return; }
    this.examSaving.set(true);
    const raw = this.examForm.getRawValue();
    const req: SaveExamRequest = {
      examType: raw.examType!,
      examMethod: raw.examMethod!,
      date: raw.date || null,
      questionCount: raw.questionCount ?? null,
      description: raw.description || null,
    };
    const id = this.editingExamId();
    if (id) {
      this.svc.updateExam(this.courseId(), id, req).subscribe({
        next: () => { this.exams.update(es => es.map(e => e.id === id ? { ...e, ...req } : e)); this.cancelExamForm(); this.examSaving.set(false); },
        error: () => this.examSaving.set(false),
      });
    } else {
      this.svc.addExam(this.courseId(), req).subscribe({
        next: (e) => { this.exams.update(es => [...es, e]); this.cancelExamForm(); this.examSaving.set(false); },
        error: () => this.examSaving.set(false),
      });
    }
  }

  deleteExam(id: number): void {
    if (!confirm('Bu sınavı silmek istiyor musunuz?')) return;
    this.svc.deleteExam(this.courseId(), id).subscribe(() =>
      this.exams.update(es => es.filter(e => e.id !== id)));
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
    this.componentForm.reset({
      name: component?.name ?? '',
      type: component?.type ?? '',
      weight: component?.weight ?? 0,
      date: component?.date ? component.date.substring(0, 10) : '',
      description: component?.description ?? '',
    });
    this.componentFormVisible.set(true);
  }

  cancelComponentForm(): void { this.componentFormVisible.set(false); this.editingComponentId.set(null); }

  saveComponentForm(): void {
    if (this.componentForm.invalid) { this.componentForm.markAllAsTouched(); return; }
    this.componentSaving.set(true);
    const raw = this.componentForm.getRawValue();
    const req: SaveAssessmentComponentRequest = {
      name: raw.name!,
      type: raw.type!,
      weight: raw.weight ?? 0,
      date: raw.date || null,
      description: raw.description || null,
    };
    const id = this.editingComponentId();
    if (id) {
      this.svc.updateAssessmentComponent(this.courseId(), id, req).subscribe({
        next: () => { this.components.update(cs => cs.map(c => c.id === id ? { ...c, ...req } : c)); this.cancelComponentForm(); this.componentSaving.set(false); },
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

  gradeClass(grade: number): string {
    if (grade >= 70) return 'grade-pass';
    if (grade >= 50) return 'grade-mid';
    return 'grade-fail';
  }

  // ── Statistics (Dönem Sonu Raporları) ─────────────────────────────────────

  loadStatistics(): void {
    this.statsLoading.set(true);
    this.svc.getStatistics(this.courseId()).subscribe({
      next: (s) => { this.stats.set(s); this.statsLoading.set(false); },
      error: () => this.statsLoading.set(false),
    });
  }

  // Çubuk grafik için: en yüksek kova değerine göre yüzde (CSS bar yüksekliği)
  bucketPercent(count: number): number {
    const max = Math.max(...this.stats()!.distribution.map(d => d.count), 1);
    return Math.round((count / max) * 100);
  }

  readonly reportCards = [
    { icon: '📋', title: 'Ders Genel Raporu', desc: 'Dersin genel istatistikleri ve özeti' },
    { icon: '🎯', title: 'Öğrenme Çıktıları Raporu', desc: 'Öğrenme çıktılarının başarı durumu analizi' },
    { icon: '🔗', title: 'Program Çıktıları Katkı Raporu', desc: 'Programın çıktılarına katkı düzeyleri' },
    { icon: '📊', title: 'Başarı Dağılımı Raporu', desc: 'Öğrenci not dağılımı ve istatistikler' },
  ];
}
