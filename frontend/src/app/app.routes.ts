import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { AdminDashboard } from './features/admin-dashboard/admin-dashboard';
import { InstructorDashboard } from './features/instructor-dashboard/instructor-dashboard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'admin', component: AdminDashboard },
  { path: 'instructor', component: InstructorDashboard },
  { path: '**', redirectTo: 'login' }
];