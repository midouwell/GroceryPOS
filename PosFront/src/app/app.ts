import { Component } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { afterNextRender, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { fromEvent } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
@Component({ selector: 'app-root', imports: [RouterOutlet], templateUrl: './app.html' })
export class App {
  private readonly document = inject(DOCUMENT);
  readonly isFullscreen = signal(false);
  readonly fullscreenError = signal('');

  constructor() {
    fromEvent(this.document, 'fullscreenchange')
      .pipe(takeUntilDestroyed())
      .subscribe(() => {
        this.isFullscreen.set(Boolean(this.document.fullscreenElement));
        if (!this.document.fullscreenElement) this.fullscreenError.set('');
      });
    afterNextRender(() => {
      void this.enterFullscreen();
    });
  }

  async toggleFullscreen() {
    if (this.document.fullscreenElement) {
      await this.document.exitFullscreen();
      return;
    }
    await this.enterFullscreen();
  }

  private async enterFullscreen() {
    const target = this.document.documentElement;
    if (this.document.fullscreenElement || !target.requestFullscreen) return;
    try {
      await target.requestFullscreen();
      this.fullscreenError.set('');
    } catch {
      this.fullscreenError.set('Select full screen to start the register in focused mode.');
    }
  }
}
