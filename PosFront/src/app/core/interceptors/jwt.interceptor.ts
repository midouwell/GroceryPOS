import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_BASE_URL } from '../config/api.config';
import { AuthStore } from '../state/auth.store';

export const jwtInterceptor: HttpInterceptorFn = (request, next) => {
  const baseUrl = inject(API_BASE_URL).replace(/\/$/, '');
  const token = inject(AuthStore).token();
  const eligible = request.url.startsWith(baseUrl);
  if (!eligible || !token?.trim()) return next(request);
  return next(request.clone({ setHeaders: { Authorization: 'Bearer ' + token } }));
};
