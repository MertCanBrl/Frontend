import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { InstructorService } from '../../core/services/instructor.service';
import { UiBtn } from '../../shared/ui/ui-button.directive';
import { UiCard } from '../../shared/ui/ui-card';
import {
  CourseDetailDto, CourseTopicDto, LearningOutcomeDto,
  MappingMatrixDto, MappingCellDto
} from '../../core/models/course.models';

type Tab = 'info' | 'topics' | 'outcomes' | 'mapping' | 'approval';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, UiBtn, UiCard],
  templateUrl: './course-detail.html',
  styleUrl: './course-detail.css',
})
export class CourseDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private svc = inject(InstructorService);
  private fb = inject(FormBuilder);

  courseId = signal(0);
  course = signal<CourseDetailDto | null>(null);
  loading = signal(true);
  activeTab = signal<Tab>('info');

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
  bloomLevels = ['Hatırlama', 'Anlama', 'Uygulama', 'Analiz', 'Sentez', 'Değerlendirme'];
  outcomeForm = this.fb.group({
    description: ['', Validators.required],
    bloomLevel: [''],
    component: [''],
  });

  // LO-PO Mapping
  matrix = signal<MappingMatrixDto | null>(null);
  mappingMap = computed(() => {
    const m = new Map<string, number>();
    this.matrix()?.mappings.forEach(c => m.set(`${c.learningOutcomeId}-${c.programOutcomeId}`, c.contributionLevel));
    return m;
  });

  readonly contributionLabels = ['—', '1 Çok Düşük', '2 Düşük', '3 Orta', '4 Yüksek', '5 Çok Yüksek'];

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.courseId.set(id);
    this.loadCourse();
  }

  loadCourse(): void {
    this.loading.set(true);
    this.svc.getCourseDetail(this.courseId()).subscribe({
      next: (c) => { this.course.set(c); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  setTab(tab: Tab): void {
    this.activeTab.set(tab);
    if (tab === 'topics' && this.topics().length === 0) this.loadTopics();
    if (tab === 'outcomes' && this.outcomes().length === 0) this.loadOutcomes();
    if (tab === 'mapping') this.loadMatrix();
  }

  // ── Topics ──────────────────────────────────────────────
  loadTopics(): void {
    this.svc.getTopics(this.courseId()).subscribe(t => this.topics.set(t));
  }

  openTopicForm(topic?: CourseTopicDto): void {
    this.editingTopicId.set(topic?.id ?? null);
    this.topicForm.reset({ title: topic?.title ?? '', description: topic?.description ?? '' });
    this.topicFormVisible.set(true);
  }

  cancelTopicForm(): void { this.topicFormVisible.set(false); this.editingTopicId.set(null); }

  saveTopicForm(): void {
    if (this.topicForm.invalid) { this.topicForm.markAllAsTouched(); return; }
    this.topicSaving.set(true);
    const raw = this.topicForm.getRawValue();
    const nextOrder = this.topics().length + 1;
    const req = { title: raw.title!, description: raw.description || null, orderNumber: this.editingTopicId() ? 0 : nextOrder };

    const id = this.editingTopicId();
    if (id) {
      const existing = this.topics().find(t => t.id === id)!;
      req.orderNumber = existing.orderNumber;
      this.svc.updateTopic(this.courseId(), id, req).subscribe({
        next: () => { this.topics.update(ts => ts.map(t => t.id === id ? { ...t, ...req } : t)); this.cancelTopicForm(); this.topicSaving.set(false); },
        error: () => this.topicSaving.set(false),
      });
    } else {
      this.svc.addTopic(this.courseId(), req).subscribe({
        next: (t) => { this.topics.update(ts => [...ts, t]); this.cancelTopicForm(); this.topicSaving.set(false); },
        error: () => this.topicSaving.set(false),
      });
    }
  }

  deleteTopic(id: number): void {
    if (!confirm('Bu konuyu silmek istiyor musunuz?')) return;
    this.svc.deleteTopic(this.courseId(), id).subscribe(() =>
      this.topics.update(ts => ts.filter(t => t.id !== id)));
  }

  // ── Learning Outcomes ────────────────────────────────────
  loadOutcomes(): void {
    this.svc.getLearningOutcomes(this.courseId()).subscribe(o => this.outcomes.set(o));
  }

  openOutcomeForm(outcome?: LearningOutcomeDto): void {
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
    if (!confirm('Bu öğrenme çıktısını silmek istiyor musunuz?')) return;
    this.svc.deleteLearningOutcome(this.courseId(), id).subscribe(() =>
      this.outcomes.update(os => os.filter(o => o.id !== id)));
  }

  // ── LO-PO Mapping ────────────────────────────────────────
  loadMatrix(): void {
    this.svc.getMatrix(this.courseId()).subscribe(m => this.matrix.set(m));
  }

  getContribution(loId: number, poId: number): number {
    return this.mappingMap().get(`${loId}-${poId}`) ?? 0;
  }

  cycleContribution(loId: number, poId: number): void {
    if (this.course()?.isLocked) return;
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

  // ── Approval ─────────────────────────────────────────────
  get approvalStatus(): { ok: boolean; message: string }[] {
    const c = this.course();
    if (!c) return [];
    return [
      { ok: this.topics().length > 0 || c.topicCount > 0, message: 'Ders konuları girilmiş' },
      { ok: this.outcomes().length > 0 || c.learningOutcomeCount > 0, message: 'Öğrenme çıktıları girilmiş' },
      { ok: (this.matrix()?.mappings.length ?? 0) > 0, message: 'ÖÇ-PÇ eşleştirme yapılmış' },
    ];
  }

  get canApprove(): boolean {
    return this.approvalStatus.every(s => s.ok) && !(this.course()?.isLocked ?? true);
  }
}
