import { signalStore, withState } from '@ngrx/signals';

export interface AppSettings {
  currency: string;
  locale: string;
  defaultTaxRate: number;
  storeName: string;
  storeId: string;
  useCompactProductTiles: boolean;
}
const defaultSettings: AppSettings = {
  currency: 'CAD',
  locale: 'en-CA',
  defaultTaxRate: 0.13,
  storeName: 'Grove Market',
  storeId: 'STORE-01',
  useCompactProductTiles: false,
};
export const AppSettingsStore = signalStore({ providedIn: 'root' }, withState(defaultSettings));
