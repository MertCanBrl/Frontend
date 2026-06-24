import { Routes } from '@angular/router';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login').then(m => m.Login)
  },
  {
    path: 'admin',
    loadComponent: () => import('./features/admin-dashboard/admin-dashboard').then(m => m.AdminDashboard),
    canActivate: [roleGuard],
    data: { role: 'Admin' }
  },
  {
    path: 'instructor',
    loadComponent: () => import('./features/instructor-shell/instructor-shell').then(m => m.InstructorShell),
    canActivate: [roleGuard],
    data: { role: 'Instructor' },
    children: [
      { path: '', redirectTo: 'course-contents', pathMatch: 'full' },
      {
        path: 'course-contents',
        loadComponent: () => import('./features/course-contents/course-contents').then(m => m.CourseContents)
      },
      {
        path: 'course-contents/:courseId',
        loadComponent: () => import('./features/course-content-detail/course-content-detail').then(m => m.CourseContentDetail)
      },
      {
        path: 'term-courses',
        loadComponent: () => import('./features/term-courses/term-courses').then(m => m.TermCourses)
      },
      {
        path: 'term-courses/:courseId',
        loadComponent: () => import('./features/term-course-detail/term-course-detail').then(m => m.TermCourseDetail)
      },
      {
        path: 'course/:id',
        loadComponent: () => import('./features/course-detail/course-detail').then(m => m.CourseDetail)
      },
    ]
  },
  { path: '**', redirectTo: 'login' }
];
