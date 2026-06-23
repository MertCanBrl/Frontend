import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Rotaya beklenen rolü data: { role: 'Admin' } ile geçiyoruz
export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Önce giriş kontrolü
  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  const expectedRole = route.data['role'] as string | undefined;
  const userRole = authService.getRole();

  if (expectedRole && userRole !== expectedRole) {
    // Yanlış rol: kullanıcıyı kendi alanına gönder
    const fallback = userRole === 'Admin' ? '/admin' : '/instructor';
    return router.createUrlTree([fallback]);
  }

  return true;
};
