import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-instructor-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './instructor-shell.html',
  styleUrl: './instructor-shell.css',
})
export class InstructorShell {
  private authService = inject(AuthService);
  fullName = this.authService.getFullName();

  logout(): void {
    this.authService.logout();
  }
}
