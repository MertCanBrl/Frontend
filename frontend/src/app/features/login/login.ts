import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  errorMessage = '';
  isLoading = false;

  // Sol paneldeki ızgara motifi için: 9x6 = 54 hücre, bazıları "değerlendirilmiş" (altın)
  gridCells = Array.from({ length: 54 }, (_, i) => i);
  activeCells = new Set([3, 10, 16, 23, 29, 38, 44, 50]);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    this.isLoading = true;

    const { email, password } = this.loginForm.getRawValue();

    this.authService.login({ email: email!, password: password! }).subscribe({
      next: (response) => {
        this.isLoading = false;

        if (response.role === 'Admin') {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/instructor']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'E-posta veya şifre hatalı.';
        console.error('Login error:', err);
      }
    });
  }
}