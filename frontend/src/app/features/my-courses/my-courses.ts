import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InstructorService } from '../../core/services/instructor.service';
import { InstructorCourseDto } from '../../core/models/course.models';

@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './my-courses.html',
  styleUrl: './my-courses.css',
})
export class MyCourses implements OnInit {
  private svc = inject(InstructorService);

  courses = signal<InstructorCourseDto[]>([]);
  loading = signal(true);
  search = signal('');

  ngOnInit(): void {
    this.svc.getMyCourses().subscribe({
      next: (c) => { this.courses.set(c); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  filtered() {
    const q = this.search().toLowerCase();
    return this.courses().filter(c =>
      c.name.toLowerCase().includes(q) || c.code.toLowerCase().includes(q)
    );
  }

  completionPct(c: InstructorCourseDto): number {
    let score = 0;
    if (c.topicCount > 0) score += 50;
    if (c.learningOutcomeCount > 0) score += 50;
    return score;
  }

  onSearch(e: Event): void {
    this.search.set((e.target as HTMLInputElement).value);
  }
}
