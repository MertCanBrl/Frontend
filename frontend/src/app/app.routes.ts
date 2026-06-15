import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { AdminDashboard } from './features/admin-dashboard/admin-dashboard';
import { InstructorShell } from './features/instructor-shell/instructor-shell';
import { MyCourses } from './features/my-courses/my-courses';
import { CourseDetail } from './features/course-detail/course-detail';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'admin', component: AdminDashboard },
  {
    path: 'instructor',
    component: InstructorShell,
    children: [
      { path: '', component: MyCourses },
      { path: 'course/:id', component: CourseDetail },
    ]
  },
  { path: '**', redirectTo: 'login' }
];
