import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AutomationJob, ProjectListItem } from '../../core/models';
import { errorMessage, prettyDate, tone } from '../../core/format';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-automation',
  imports: [FormsModule],
  templateUrl: './automation.component.html'
})
export class AutomationComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  readonly jobs = signal<AutomationJob[]>([]);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly loading = signal(true);
  readonly selected = signal<AutomationJob | null>(null);
  projectId = '';
  jobType = 'TranslateModel';
  readonly types = ['TranslateModel', 'DesignAutomation', 'SheetExport', 'Validation'];

  constructor() {
    this.refresh();
    this.api.projects().subscribe({ next: items => this.projects.set(items), error: () => this.projects.set([]) });
  }

  refresh(): void {
    this.api.jobs().subscribe({
      next: items => {
        this.jobs.set(items);
        this.loading.set(false);
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(errorMessage(error));
      }
    });
  }

  enqueue(): void {
    this.api.enqueueJob({
      projectId: this.projectId || null,
      jobType: this.jobType,
      requestPayload: 'Queued from the studio UI'
    }).subscribe({
      next: job => {
        this.jobs.update(items => [job, ...items]);
        this.selected.set(job);
        this.toast.success('Job queued. The worker will pick it up.');
        this.watch(job.automationJobId);
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }

  watch(id: string): void {
    const timer = window.setInterval(() => {
      this.api.job(id).subscribe({
        next: job => {
          this.jobs.update(items => items.map(item => item.automationJobId === id ? job : item));
          if (this.selected()?.automationJobId === id) {
            this.selected.set(job);
          }
          if (job.status === 'Succeeded' || job.status === 'Failed') {
            window.clearInterval(timer);
          }
        }
      });
    }, 2000);
  }

  tone = tone;
  prettyDate = prettyDate;
}
