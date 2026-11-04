import { Directive, ElementRef, inject, output } from '@angular/core';

@Directive({ selector: 'input[appBarcodeScan]', host: { '(keydown)': 'onKeydown($event)' } })
export class BarcodeScanDirective {
  readonly appBarcodeScan = output<string>();
  private readonly input = inject(ElementRef<HTMLInputElement>);
  private lastCharacterAt = 0;
  private fastScan = false;

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      const value = this.input.nativeElement.value.trim();
      if (this.fastScan && value.length >= 3) {
        event.preventDefault();
        this.appBarcodeScan.emit(value);
      }
      this.fastScan = false;
      return;
    }
    if (event.key.length !== 1 || event.ctrlKey || event.altKey || event.metaKey) return;
    const now = performance.now();
    if (this.lastCharacterAt && now - this.lastCharacterAt < 90) this.fastScan = true;
    else this.fastScan = false;
    this.lastCharacterAt = now;
  }
}
