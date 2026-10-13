import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { ProjectListItem, ProjectReport } from '../../core/models';
import { errorMessage, tone } from '../../core/format';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-reports',
  imports: [FormsModule],
  templateUrl: './reports.component.html'
})
export class ReportsComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);

  readonly projects = signal<ProjectListItem[]>([]);
  readonly report = signal<ProjectReport | null>(null);
  readonly loading = signal(false);
  projectId = '';

  constructor() {
    this.api.projects().subscribe({
      next: items => {
        const visible = this.auth.isDesigner() ? items.filter(item => this.auth.leads(item)) : items;
        this.projects.set(visible);
        if (visible[0]) {
          this.projectId = visible[0].projectId;
          this.load();
        }
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }

  load(): void {
    if (!this.projectId) {
      return;
    }
    this.loading.set(true);
    this.api.report(this.projectId).subscribe({
      next: report => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(errorMessage(error));
      }
    });
  }

  passed(): number {
    return (this.report()?.validationResults ?? []).filter(item => item.status === 'Pass').length;
  }

  failed(): number {
    return (this.report()?.validationResults ?? []).filter(item => item.status === 'Fail').length;
  }

  tone = tone;
}
