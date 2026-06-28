import { Component, inject, OnInit, signal, computed, effect } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, FormArray } from '@angular/forms';
import { SlicePipe } from '@angular/common';
import { InstructorService } from '../../core/services/instructor.service';
import { UiBtn } from '../../shared/ui/ui-button.directive';
import { UiCard } from '../../shared/ui/ui-card';
import {
  CourseDetailDto, CourseTopicDto, LearningOutcomeDto,
  MappingMatrixDto, MappingCellDto,
  SurveyQuestionDto, GeneralSurveyQuestionDto
} from '../../core/models/course.models';
import { extractErrorMessage } from '../../core/utils/http-error.util';

type Tab = 'info' | 'topics' | 'outcomes' | 'mapping' | 'survey';

@Component({
  selector: 'app-course-content-detail',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, SlicePipe, UiBtn, UiCard],
  templateUrl: './course-content-detail.html',
  styleUrl: './course-content-detail.css',
})
export class CourseContentDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private svc = inject(InstructorService);
  private fb = inject(FormBuilder);

  courseId = signal(0);
  course = signal<CourseDetailDto | null>(null);
  loading = signal(true);
  activeTab = signal<Tab>('info');

  // ── Onay durumu ───────────────────────────────────────────────────────────

  submitLoading = signal(false);
  submitError = signal<string | null>(null);
  submitSuccess = signal(false);

  isEditable = computed(() => {
    const c = this.course();
    if (!c) return true;
    return c.contentStatus === 'Draft' || c.contentStatus === 'RevisionRequested';
  });

  statusLabel = computed(() => {
    const labels: Record<string, string> = {
      Draft: 'Taslak',
      PendingApproval: 'Onay Bekliyor',
      Approved: 'Onaylandı',
      RevisionRequested: 'Revize İstendi',
    };
    return labels[this.course()?.contentStatus ?? ''] ?? '';
  });

  statusClass = computed(() => {
    const classes: Record<string, string> = {
      Draft: 'badge status-draft',
      PendingApproval: 'badge status-pending',
      Approved: 'badge status-approved',
      RevisionRequested: 'badge status-revision',
    };
    return classes[this.course()?.contentStatus ?? ''] ?? 'badge status-draft';
  });

  // ── General info form ─────────────────────────────────────────────────────

  infoSaving = signal(false);
  infoSuccess = signal(false);
  infoError = signal<string | null>(null);
  infoForm = this.fb.group({ description: [''], objective: [''] });

  topicsLoaded = signal(false);
  outcomesLoaded = signal(false);
  topicCount = computed(() => this.topicsLoaded() ? this.topics().length : (this.course()?.topicCount ?? 0));
  outcomeCount = computed(() => this.outcomesLoaded() ? this.outcomes().length : (this.course()?.learningOutcomeCount ?? 0));

  // Topics
  topics = signal<CourseTopicDto[]>([]);
  topicFormVisible = signal(false);
  editingTopicId = signal<number | null>(null);
  topicSaving = signal(false);
  topicForm = this.fb.group({
    title: ['', Validators.required],
    description: [''],
  });

  // Learning Outcomes
  outcomes = signal<LearningOutcomeDto[]>([]);
  outcomeFormVisible = signal(false);
  editingOutcomeId = signal<number | null>(null);
  outcomeSaving = signal(false);
  bloomLevels = ['Hatırlama', 'Anlama', 'Uygulama', 'Analiz', 'Değerlendirme', 'Yaratma'];
  components = ['Bilgi', 'Beceri', 'Yetkinlik'];
  outcomeForm = this.fb.group({
    description: ['', Validators.required],
    bloomLevel: [''],
    component: [''],
  });

  // LO-PO Mapping
  matrix = signal<MappingMatrixDto | null>(null);
  poLegendOpen = signal(false);
  togglePoLegend(): void { this.poLegendOpen.update(v => !v); }
  loLegendOpen = signal(false);
  toggleLoLegend(): void { this.loLegendOpen.update(v => !v); }
  mappingMap = computed(() => {
    const m = new Map<string, number>();
    this.matrix()?.mappings.forEach(c => m.set(`${c.learningOutcomeId}-${c.programOutcomeId}`, c.contributionLevel));
    return m;
  });
  readonly contributionLabels = ['—', '1 Çok Düşük', '2 Düşük', '3 Orta', '4 Yüksek', '5 Çok Yüksek'];

  // Survey Questions
  generalSurveyQuestions = signal<GeneralSurveyQuestionDto[]>([]);
  surveyQuestions = signal<SurveyQuestionDto[]>([]);
  surveyFormVisible = signal(false);
  editingSurveyId = signal<number | null>(null);
  surveySaving = signal(false);
  surveyError = signal<string | null>(null);
  surveyForm = this.fb.group({
    questionText: ['', Validators.required],
    isActive: [true],
    loWeights: this.fb.array([]),
  });

  get loWeightsArray(): FormArray {
    return this.surveyForm.get('loWeights') as FormArray;
  }

  addLOWeightRow(learningOutcomeId: number | null = null, weightPercentage: number = 0): void {
    this.loWeightsArray.push(this.fb.group({
      learningOutcomeId: [learningOutcomeId, Validators.required],
      weightPercentage: [weightPercentage, [Validators.required, Validators.min(1), Validators.max(100)]],
    }));
  }

  removeLOWeightRow(index: number): void {
    this.loWeightsArray.removeAt(index);
  }

  constructor() {
    effect(() => {
      if (this.isEditable()) {
        this.infoForm.enable();
      } else {
        this.infoForm.disable();
      }
    });
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('courseId'));
    this.courseId.set(id);
    this.loadCourse();
  }

  loadCourse(): void {
    this.loading.set(true);
    this.svc.getCourseContentDetail(this.courseId()).subscribe({
      next: (c) => {
        this.course.set(c);
        this.loading.set(false);
        this.infoForm.patchValue({ description: c.description ?? '', objective: c.objective ?? '' });
      },
      error: () => this.loading.set(false),
    });
  }

  setTab(tab: Tab): void {
    this.activeTab.set(tab);
    if (tab === 'topics' && this.topics().length === 0) this.loadTopics();
    if (tab === 'outcomes' && this.outcomes().length === 0) this.loadOutcomes();
    if (tab === 'mapping') this.loadMatrix();
    if (tab === 'survey') this.loadSurveyTab();
  }

  // ── Onay akışı ────────────────────────────────────────────────────────────

  submitForReview(): void {
    this.submitLoading.set(true);
    this.submitError.set(null);
    this.svc.submitCourseContentForReview(this.courseId()).subscribe({
      next: () => {
        this.submitLoading.set(false);
        this.submitSuccess.set(true);
        this.course.update(c => c ? { ...c, contentStatus: 'PendingApproval', isLocked: true } : c);
        setTimeout(() => this.submitSuccess.set(false), 3000);
      },
      error: (err) => {
        this.submitLoading.set(false);
        this.submitError.set(extractErrorMessage(err, 'Bir hata oluştu. Lütfen tekrar deneyin.'));
      },
    });
  }

  // ── General Info ──────────────────────────────────────────────────────────

  saveInfo(): void {
    if (!this.isEditable()) return;
    this.infoSaving.set(true);
    this.infoError.set(null);
    const raw = this.infoForm.getRawValue();
    this.svc.updateCourseContentInfo(this.courseId(), {
      description: raw.description ?? null,
      objective: raw.objective ?? null,
    }).subscribe({
      next: () => {
        this.course.update(c => c ? { ...c, description: raw.description, objective: raw.objective } : c);
        this.infoSaving.set(false);
        this.flashSuccess();
      },
      error: (err) => {
        this.infoSaving.set(false);
        this.infoError.set(extractErrorMessage(err, 'Bilgiler kaydedilemedi.'));
      },
    });
  }

  private flashSuccess(): void {
    this.infoSuccess.set(true);
    setTimeout(() => this.infoSuccess.set(false), 2500);
  }

  // ── Topics ────────────────────────────────────────────────────────────────

  loadTopics(): void {
    this.svc.getTopics(this.courseId()).subscribe(t => { this.topics.set(t); this.topicsLoaded.set(true); });
  }

  openTopicForm(topic?: CourseTopicDto): void {
    if (!this.isEditable()) return;
    this.editingTopicId.set(topic?.id ?? null);
    this.topicForm.reset({ title: topic?.title ?? '', description: topic?.description ?? '' });
    this.topicFormVisible.set(true);
  }

  cancelTopicForm(): void { this.topicFormVisible.set(false); this.editingTopicId.set(null); }

  saveTopicForm(): void {
    if (this.topicForm.invalid) { this.topicForm.markAllAsTouched(); return; }
    this.topicSaving.set(true);
    const raw = this.topicForm.getRawValue();
    const id = this.editingTopicId();
    if (id) {
      const existing = this.topics().find(t => t.id === id)!;
      this.svc.updateTopic(this.courseId(), id, { title: raw.title!, description: raw.description || null, orderNumber: existing.orderNumber }).subscribe({
        next: () => { this.topics.update(ts => ts.map(t => t.id === id ? { ...t, title: raw.title!, description: raw.description || null } : t)); this.cancelTopicForm(); this.topicSaving.set(false); },
        error: () => this.topicSaving.set(false),
      });
    } else {
      const nextOrder = this.topics().length + 1;
      this.svc.addTopic(this.courseId(), { title: raw.title!, description: raw.description || null, orderNumber: nextOrder }).subscribe({
        next: (t) => { this.topics.update(ts => [...ts, t]); this.cancelTopicForm(); this.topicSaving.set(false); },
        error: () => this.topicSaving.set(false),
      });
    }
  }

  deleteTopic(id: number): void {
    if (!this.isEditable()) return;
    if (!confirm('Bu konuyu silmek istiyor musunuz?')) return;
    this.svc.deleteTopic(this.courseId(), id).subscribe(() =>
      this.topics.update(ts => ts.filter(t => t.id !== id)));
  }

  // ── Learning Outcomes ─────────────────────────────────────────────────────

  loadOutcomes(): void {
    this.svc.getLearningOutcomes(this.courseId()).subscribe(o => { this.outcomes.set(o); this.outcomesLoaded.set(true); });
  }

  openOutcomeForm(outcome?: LearningOutcomeDto): void {
    if (!this.isEditable()) return;
    this.editingOutcomeId.set(outcome?.id ?? null);
    this.outcomeForm.reset({ description: outcome?.description ?? '', bloomLevel: outcome?.bloomLevel ?? '', component: outcome?.component ?? '' });
    this.outcomeFormVisible.set(true);
  }

  cancelOutcomeForm(): void { this.outcomeFormVisible.set(false); this.editingOutcomeId.set(null); }

  saveOutcomeForm(): void {
    if (this.outcomeForm.invalid) { this.outcomeForm.markAllAsTouched(); return; }
    this.outcomeSaving.set(true);
    const raw = this.outcomeForm.getRawValue();
    const req = { description: raw.description!, bloomLevel: raw.bloomLevel || null, component: raw.component || null };
    const id = this.editingOutcomeId();
    if (id) {
      this.svc.updateLearningOutcome(this.courseId(), id, req).subscribe({
        next: () => { this.outcomes.update(os => os.map(o => o.id === id ? { ...o, ...req } : o)); this.cancelOutcomeForm(); this.outcomeSaving.set(false); },
        error: () => this.outcomeSaving.set(false),
      });
    } else {
      this.svc.addLearningOutcome(this.courseId(), req).subscribe({
        next: (o) => { this.outcomes.update(os => [...os, o]); this.cancelOutcomeForm(); this.outcomeSaving.set(false); },
        error: () => this.outcomeSaving.set(false),
      });
    }
  }

  deleteOutcome(id: number): void {
    if (!this.isEditable()) return;
    if (!confirm('Bu öğrenme çıktısını silmek istiyor musunuz?')) return;
    this.svc.deleteLearningOutcome(this.courseId(), id).subscribe(() =>
      this.outcomes.update(os => os.filter(o => o.id !== id)));
  }

  // ── LO-PO Mapping ─────────────────────────────────────────────────────────

  loadMatrix(): void {
    this.svc.getMatrix(this.courseId()).subscribe(m => this.matrix.set(m));
  }

  getContribution(loId: number, poId: number): number {
    return this.mappingMap().get(`${loId}-${poId}`) ?? 0;
  }

  cycleContribution(loId: number, poId: number): void {
    if (!this.isEditable()) return;
    const current = this.getContribution(loId, poId);
    const next = (current + 1) % 6;
    const cell: MappingCellDto = { learningOutcomeId: loId, programOutcomeId: poId, contributionLevel: next };
    this.svc.updateMappingCell(this.courseId(), cell).subscribe(() => {
      this.matrix.update(m => {
        if (!m) return m;
        const existing = m.mappings.find(c => c.learningOutcomeId === loId && c.programOutcomeId === poId);
        if (existing) { existing.contributionLevel = next; return { ...m, mappings: [...m.mappings] }; }
        return { ...m, mappings: [...m.mappings, cell] };
      });
    });
  }

  contributionClass(level: number): string {
    return ['level-0', 'level-1', 'level-2', 'level-3', 'level-4', 'level-5'][level] ?? 'level-0';
  }

  // ── Survey Questions ──────────────────────────────────────────────────────

  private surveyTabLoaded = false;

  loadSurveyTab(): void {
    if (!this.surveyTabLoaded) {
      this.surveyTabLoaded = true;
      this.svc.getGeneralSurveyQuestions().subscribe(q => this.generalSurveyQuestions.set(q));
      this.svc.getSurveyQuestions(this.courseId()).subscribe(q => this.surveyQuestions.set(q));
      if (this.outcomes().length === 0) this.loadOutcomes();
    }
  }

  openSurveyForm(question?: SurveyQuestionDto): void {
    if (!this.isEditable()) return;
    this.editingSurveyId.set(question?.id ?? null);
    this.loWeightsArray.clear();
    if (question) {
      this.surveyForm.patchValue({ questionText: question.questionText, isActive: question.isActive });
      question.loWeights.forEach(w => this.addLOWeightRow(w.learningOutcomeId, w.weightPercentage));
    } else {
      this.surveyForm.reset({ questionText: '', isActive: true });
    }
    this.surveyFormVisible.set(true);
  }

  cancelSurveyForm(): void {
    this.surveyFormVisible.set(false);
    this.editingSurveyId.set(null);
    this.surveyError.set(null);
    this.loWeightsArray.clear();
  }

  saveSurveyForm(): void {
    this.surveyError.set(null);
    if (this.surveyForm.invalid) { this.surveyForm.markAllAsTouched(); return; }
    this.surveySaving.set(true);
    const raw = this.surveyForm.getRawValue();
    const req = {
      questionText: raw.questionText!,
      isActive: raw.isActive ?? true,
      loWeights: (raw.loWeights as any[]).map(w => ({
        learningOutcomeId: Number(w.learningOutcomeId),
        weightPercentage: Number(w.weightPercentage),
      })),
    };
    const id = this.editingSurveyId();
    if (id) {
      this.svc.updateSurveyQuestion(this.courseId(), id, req).subscribe({
        next: () => {
          const updatedWeights = req.loWeights.map(w => {
            const lo = this.outcomes().find(o => o.id === w.learningOutcomeId);
            return { learningOutcomeId: w.learningOutcomeId, learningOutcomeCode: lo?.code ?? '', weightPercentage: w.weightPercentage };
          });
          this.surveyQuestions.update(qs => qs.map(q => q.id === id
            ? { ...q, questionText: req.questionText, isActive: req.isActive, loWeights: updatedWeights }
            : q));
          this.cancelSurveyForm();
          this.surveySaving.set(false);
        },
        error: (err) => {
          this.surveySaving.set(false);
          this.surveyError.set(extractErrorMessage(err, 'Anket sorusu kaydedilemedi.'));
        },
      });
    } else {
      this.svc.addSurveyQuestion(this.courseId(), req).subscribe({
        next: (q) => { this.surveyQuestions.update(qs => [...qs, q]); this.cancelSurveyForm(); this.surveySaving.set(false); },
        error: (err) => {
          this.surveySaving.set(false);
          this.surveyError.set(extractErrorMessage(err, 'Anket sorusu kaydedilemedi.'));
        },
      });
    }
  }

  getOutcomeTooltip(learningOutcomeId: number): string {
    const o = this.outcomes().find(o => o.id === learningOutcomeId);
    return o ? `${o.code}: ${o.description}` : '';
  }

  deleteSurveyQuestion(id: number): void {
    if (!this.isEditable()) return;
    if (!confirm('Bu soruyu silmek istiyor musunuz?')) return;
    this.svc.deleteSurveyQuestion(this.courseId(), id).subscribe(() =>
      this.surveyQuestions.update(qs => qs.filter(q => q.id !== id)));
  }
}
