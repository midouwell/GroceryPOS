import { InjectionToken, Provider } from '@angular/core';

/** Future ASP.NET Core API base URL. Mock services do not send requests to it. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');
export const provideApiBaseUrl = (baseUrl = 'https://localhost:7001/api'): Provider => ({
  provide: API_BASE_URL,
  useValue: baseUrl,
});
