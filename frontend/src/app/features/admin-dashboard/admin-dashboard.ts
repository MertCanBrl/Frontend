import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard {
  private authService = inject(AuthService);

  fullName = this.authService.getFullName();

  logout(): void {
    this.authService.logout();
  }
}