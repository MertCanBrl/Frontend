import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-instructor-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './instructor-dashboard.html',
  styleUrl: './instructor-dashboard.css'
})
export class InstructorDashboard {
  private authService = inject(AuthService);

  fullName = this.authService.getFullName();

  logout(): void {
    this.authService.logout();
  }
}