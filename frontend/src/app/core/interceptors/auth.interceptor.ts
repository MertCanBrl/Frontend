import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !req.url.includes('/auth/login')) {
        authService.logout();
      } else if (err.status === 403) {
        console.error('[AUTH] Yetkisiz erişim:', req.url);
      } else if (err.status === 429) {
        console.warn('[RATE LIMIT] Çok fazla istek gönderildi, lütfen bekleyin.');
      } else if (err.status >= 500) {
        console.error('[API] Sunucu hatası:', err.status, req.url);
      }
      return throwError(() => err);
    })
  );
};
