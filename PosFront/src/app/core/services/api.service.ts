import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  get<T>(path: string, params?: Record<string, string | number>): Observable<T> {
    const httpParams = new HttpParams({
      fromObject: params
        ? Object.fromEntries(Object.entries(params).map(([key, value]) => [key, String(value)]))
        : {},
    });
    return this.http.get<T>(this.url(path), { params: httpParams });
  } 
  post<TBody, TResult>(path: string, body: TBody): Observable<TResult> {
    return this.http.post<TResult>(this.url(path), body);
  }
  put<TBody, TResult>(path: string, body: TBody): Observable<TResult> {
    return this.http.put<TResult>(this.url(path), body);
  }
  delete<TResult>(path: string): Observable<TResult> {
    return this.http.delete<TResult>(this.url(path));
  }
  private url(path: string): string {
    return this.baseUrl.replace(/\/$/, '') + '/' + path.replace(/^\//, '');
  }
}
