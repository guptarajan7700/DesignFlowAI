import { Injectable, signal } from '@angular/core';
import { ChatMessage, CommandPreview, StoredCheck } from './models';

@Injectable({ providedIn: 'root' })
export class ChatStore {
  readonly messages = signal<ChatMessage[]>([
    {
      id: 'welcome',
      role: 'assistant',
      text: 'Ask about overdue work, open issues, or the studio pass rate. Model searches stay read-only until you confirm them.',
      source: 'studio'
    }
  ]);
  readonly busy = signal(false);

  push(message: Omit<ChatMessage, 'id'>): ChatMessage {
    const next: ChatMessage = { ...message, id: crypto.randomUUID() };
    this.messages.update(items => [...items, next]);
    return next;
  }

  attachPreview(id: string, preview: CommandPreview): void {
    this.messages.update(items => items.map(item => item.id === id ? { ...item, preview } : item));
  }

  clearPreview(id: string): void {
    this.messages.update(items => items.map(item => item.id === id ? { ...item, preview: undefined } : item));
  }

  attachResults(id: string, results: StoredCheck[], text: string): void {
    this.messages.update(items => items.map(item => item.id === id ? { ...item, results, text, preview: undefined } : item));
  }
}
