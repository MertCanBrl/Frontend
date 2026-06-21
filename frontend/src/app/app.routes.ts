import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { AdminDashboard } from './features/admin-dashboard/admin-dashboard';
import { InstructorShell } from './features/instructor-shell/instructor-shell';
import { MyCourses } from './features/my-courses/my-courses';
import { CourseDetail } from './features/course-detail/course-detail';
import { CourseContents } from './features/course-contents/course-contents';
import { CourseContentDetail } from './features/course-content-detail/course-content-detail';
import { TermCourses } from './features/term-courses/term-courses';
import { TermCourseDetail } from './features/term-course-detail/term-course-detail';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'admin', component: AdminDashboard },
  {
    path: 'instructor',
    component: InstructorShell,
    children: [
      { path: '', redirectTo: 'course-contents', pathMatch: 'full' },
      { path: 'course-contents', component: CourseContents },
      { path: 'course-contents/:courseId', component: CourseContentDetail },
      { path: 'term-courses', component: TermCourses },
      { path: 'term-courses/:courseId', component: TermCourseDetail },
      // Geriye dönük uyumluluk
      { path: 'course/:id', component: CourseDetail },
    ]
  },
  { path: '**', redirectTo: 'login' }
];
