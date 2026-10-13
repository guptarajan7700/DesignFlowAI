import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Issue, ProjectListItem, TeamMember } from '../../core/models';
import { errorMessage, prettyDate, tone } from '../../core/format';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-issues',
  imports: [FormsModule, RouterLink],
  templateUrl: './issues.component.html'
})
export class IssuesComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);

  readonly issues = signal<Issue[]>([]);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly people = signal<TeamMember[]>([]);
  readonly selected = signal<Issue | null>(null);
  readonly loading = signal(true);
  severity = '';
  status = '';
  projectId = '';
  assigneeId = '';

  constructor() {
    this.load();
    this.api.projects().subscribe({
      next: items => this.projects.set(this.auth.isDesigner() ? items.filter(item => this.auth.leads(item)) : items),
      error: () => this.projects.set([])
    });
    if (this.auth.isManager()) {
      this.api.users().subscribe({
        next: items => this.people.set(items.filter(person => person.roles.includes('Designer'))),
        error: () => this.people.set([])
      });
    }
  }

  load(): void {
    this.loading.set(true);
    this.api.issues({
      projectId: this.projectId || undefined,
      severity: this.severity || undefined,
      status: this.status || undefined
    }).subscribe({
      next: items => {
        const visible = this.auth.isDesigner()
          ? items.filter(item => this.auth.isMine(item.assignedToName))
          : items;
        this.issues.set(visible);
        this.loading.set(false);
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(errorMessage(error));
      }
    });
  }

  open(issue: Issue): void {
    this.selected.set(issue);
    this.assigneeId = issue.assignedToId ?? '';
  }

  assign(): void {
    const issue = this.selected();
    if (!issue || !this.assigneeId) {
      return;
    }
    this.api.assignIssue(issue.issueId, this.assigneeId).subscribe({
      next: updated => {
        this.selected.set(updated);
        this.issues.update(items => items.map(item => item.issueId === updated.issueId ? updated : item));
        this.toast.success(`Assigned to ${updated.assignedToName}.`);
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }

  tone = tone;
  prettyDate = prettyDate;
}
