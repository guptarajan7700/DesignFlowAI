import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { ChatStore } from '../../core/chat.store';
import { errorMessage, looksLikeRevitCommand } from '../../core/format';
import { ProjectListItem } from '../../core/models';
import { IconComponent } from '../../shared/icon.component';

@Component({
  selector: 'app-assistant',
  imports: [FormsModule, IconComponent],
  templateUrl: './assistant.component.html'
})
export class AssistantComponent {
  readonly embedded = input(false);
  readonly closed = output<void>();
  readonly chat = inject(ChatStore);
  private readonly api = inject(ApiService);

  draft = '';
  projectId = '';
  readonly projects = signal<ProjectListItem[]>([]);
  readonly prompts = [
    'Which projects have overdue tasks?',
    'What is the validation pass rate?',
    'Find doors missing a fire rating',
    'Find rooms without names'
  ];

  constructor() {
    this.api.projects().subscribe({
      next: items => {
        this.projects.set(items);
        if (!this.projectId && items[0]) {
          this.projectId = items[0].projectId;
        }
      },
      error: () => this.projects.set([])
    });
  }

  usePrompt(prompt: string): void {
    this.draft = prompt;
    this.send();
  }

  send(): void {
    const text = this.draft.trim();
    if (!text || this.chat.busy()) {
      return;
    }
    this.draft = '';
    this.chat.push({ role: 'user', text });
    this.chat.busy.set(true);
    if (looksLikeRevitCommand(text)) {
      this.api.previewCommand(text, this.projectId || undefined).subscribe({
        next: preview => {
          this.chat.push({
            role: 'assistant',
            text: `${preview.detectedCommand} is allowed. ${preview.impact}`,
            source: preview.provider,
            preview
          });
          this.chat.busy.set(false);
        },
        error: error => this.fail(error)
      });
      return;
    }

    this.api.ask(text).subscribe({
      next: response => {
        this.chat.push({ role: 'assistant', text: response.answer, source: response.source });
        this.chat.busy.set(false);
      },
      error: error => this.fail(error)
    });
  }

  confirm(previewId: string, messageId: string): void {
    this.chat.busy.set(true);
    this.api.executeCommand(previewId).subscribe({
      next: response => {
        const lines = response.storedResults.length
          ? response.storedResults.map(item => `• ${item.checkName}: ${item.message}${item.elementId ? ` Element ${item.elementId}.` : ''}`).join('\n')
          : response.message;
        this.chat.attachResults(messageId, response.storedResults, `${response.message}\n${lines}`);
        this.chat.busy.set(false);
      },
      error: error => this.fail(error)
    });
  }

  cancel(messageId: string): void {
    this.chat.clearPreview(messageId);
    this.chat.push({ role: 'assistant', text: 'Cancelled. Nothing was run against the model data.' });
  }

  private fail(error: unknown): void {
    this.chat.push({ role: 'assistant', text: errorMessage(error) });
    this.chat.busy.set(false);
  }
}
