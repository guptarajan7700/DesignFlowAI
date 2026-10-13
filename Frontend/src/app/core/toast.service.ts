import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  text: string;
  tone: 'good' | 'bad' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  readonly items = signal<Toast[]>([]);

  success(text: string): void {
    this.push(text, 'good');
  }

  error(text: string): void {
    this.push(text, 'bad');
  }

  info(text: string): void {
    this.push(text, 'info');
  }

  dismiss(id: number): void {
    this.items.update(items => items.filter(item => item.id !== id));
  }

  private push(text: string, tone: Toast['tone']): void {
    const id = this.nextId++;
    this.items.update(items => [...items, { id, text, tone }]);
    window.setTimeout(() => this.dismiss(id), 4200);
  }
}
