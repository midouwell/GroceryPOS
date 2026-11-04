import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideApiBaseUrl } from './core/config/api.config';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { PRODUCT_DATA_PORT, MockProductDataService } from './features/products/services/product-data.service';
import { INVENTORY_DATA_PORT, MockInventoryDataService } from './features/inventory/services/inventory-data.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideApiBaseUrl(),
    { provide: PRODUCT_DATA_PORT, useClass: MockProductDataService },
    { provide: INVENTORY_DATA_PORT, useClass: MockInventoryDataService },
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor, errorInterceptor]))
  ]
};
