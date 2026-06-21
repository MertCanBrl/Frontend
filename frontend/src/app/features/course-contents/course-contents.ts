import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InstructorService } from '../../core/services/instructor.service';
import { InstructorCourseDto } from '../../core/models/course.models';

@Component({
  selector: 'app-course-contents',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './course-contents.html',
  styleUrl: './course-contents.css',
})
export class CourseContents implements OnInit {
  private svc = inject(InstructorService);

  courses = signal<InstructorCourseDto[]>([]);
  loading = signal(true);
  hasError = signal(false);
  search = signal('');

  ngOnInit(): void {
    this.svc.getCourseContents().subscribe({
      next: (c) => { this.courses.set(c); this.loading.set(false); },
      error: () => { this.hasError.set(true); this.loading.set(false); },
    });
  }

  filtered(): InstructorCourseDto[] {
    const q = this.search().toLowerCase().trim();
    if (!q) return this.courses();
    return this.courses().filter(c =>
      c.name.toLowerCase().includes(q) || c.code.toLowerCase().includes(q)
    );
  }

  onSearch(e: Event): void {
    this.search.set((e.target as HTMLInputElement).value);
  }
}
