import { Injectable, signal } from '@angular/core';

const KEY = 'df_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly mode = signal<'light' | 'dark'>(this.initial());

  constructor() {
    this.apply(this.mode());
  }

  toggle(): void {
    this.set(this.mode() === 'dark' ? 'light' : 'dark');
  }

  set(mode: 'light' | 'dark'): void {
    this.mode.set(mode);
    localStorage.setItem(KEY, mode);
    this.apply(mode);
  }

  private initial(): 'light' | 'dark' {
    const stored = localStorage.getItem(KEY);
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  private apply(mode: 'light' | 'dark'): void {
    document.documentElement.dataset['theme'] = mode;
  }
}
