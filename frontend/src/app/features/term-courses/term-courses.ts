import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InstructorService } from '../../core/services/instructor.service';
import { InstructorCourseDto } from '../../core/models/course.models';

@Component({
  selector: 'app-term-courses',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './term-courses.html',
  styleUrl: './term-courses.css',
})
export class TermCourses implements OnInit {
  private svc = inject(InstructorService);

  courses = signal<InstructorCourseDto[]>([]);
  semesters = signal<string[]>([]);
  selectedSemester = signal<string>('');
  loading = signal(true);
  hasError = signal(false);

  ngOnInit(): void {
    this.svc.getSemesters().subscribe({
      next: (semesters) => {
        this.semesters.set(semesters);
        if (semesters.length > 0) {
          this.selectedSemester.set(semesters[0]);
          this.loadCourses(semesters[0]);
        } else {
          this.loading.set(false);
        }
      },
      error: () => { this.hasError.set(true); this.loading.set(false); },
    });
  }

  loadCourses(semester: string): void {
    this.loading.set(true);
    this.svc.getTermCourses(semester).subscribe({
      next: (c) => { this.courses.set(c); this.loading.set(false); },
      error: () => { this.hasError.set(true); this.loading.set(false); },
    });
  }

  onSemesterChange(e: Event): void {
    const sem = (e.target as HTMLSelectElement).value;
    this.selectedSemester.set(sem);
    this.loadCourses(sem);
  }

  isApproved(course: InstructorCourseDto): boolean {
    return course.contentStatus === 'Approved';
  }

  statusLabel(status: string): string {
    const map: Record<string, string> = {
      'Draft': 'Taslak',
      'PendingApproval': 'Onay Bekliyor',
      'Approved': 'Onaylandı',
      'RevisionRequested': 'Revizyon İstendi',
    };
    return map[status] ?? status;
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      'Draft': 'status-draft',
      'PendingApproval': 'status-pending',
      'Approved': 'status-approved',
      'RevisionRequested': 'status-revision',
    };
    return map[status] ?? '';
  }
}
