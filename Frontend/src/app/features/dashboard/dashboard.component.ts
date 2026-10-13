import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Issue, Milestone, ProjectDetail, ProjectListItem, StudioNotification } from '../../core/models';
import { labelize, money, prettyDate, relativeTime, tone } from '../../core/format';

interface Slice { label: string; count: number; color: string; }
interface MileView extends Milestone { project: string; projectId: string; }

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);

  readonly loading = signal(true);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly issues = signal<Issue[]>([]);
  readonly notes = signal<StudioNotification[]>([]);
  readonly milestones = signal<MileView[]>([]);
  readonly insight = signal('Pulling a summary from studio records…');

  readonly openIssues = computed(() => this.issues().filter(item => !['Resolved', 'Closed', 'WontFix'].includes(item.status)).length);
  readonly overdue = computed(() => this.projects().reduce((sum, item) => sum + item.overdueTaskCount, 0));
  readonly proposed = computed(() => this.projects().reduce((sum, item) => sum + item.proposedTaskCount, 0));
  readonly budget = computed(() => this.projects().reduce((sum, item) => sum + item.budget, 0));
  readonly spent = computed(() => this.projects().reduce((sum, item) => sum + item.spent, 0));
  readonly attention = computed(() => this.projects().filter(item => item.status === 'NeedsAttention').length);
  readonly nextDue = computed(() => {
    const dates = this.projects().map(item => item.dueDate).filter((value): value is string => !!value).sort();
    return dates[0] ?? null;
  });
  readonly currency = computed(() => this.projects()[0]?.currency || 'USD');
  readonly slices = computed(() => this.statusSlices(this.projects()));
  readonly donut = computed(() => this.conic(this.slices()));
  readonly severities = computed(() => ['Critical', 'High', 'Medium', 'Low'].map(level => ({
    level,
    count: this.issues().filter(item => item.severity === level).length
  })));
  readonly maxSeverity = computed(() => Math.max(1, ...this.severities().map(item => item.count)));
  readonly workload = computed(() => {
    const counts = new Map<string, number>();
    for (const issue of this.issues()) {
      const name = issue.assignedToName || 'Unassigned';
      counts.set(name, (counts.get(name) ?? 0) + 1);
    }
    return [...counts.entries()].map(([name, count]) => ({ name, count }));
  });

  constructor() {
    forkJoin({
      projects: this.api.projects(),
      issues: this.api.issues(),
      notes: this.api.notifications()
    }).subscribe({
      next: result => {
        const projects = this.auth.isDesigner()
          ? result.projects.filter(item => this.auth.leads(item))
          : result.projects;
        const projectIds = new Set(projects.map(item => item.projectId));
        const issues = this.auth.isDesigner()
          ? result.issues.filter(item => this.auth.isMine(item.assignedToName))
          : result.issues.filter(item => projectIds.has(item.projectId));
        this.projects.set(projects);
        this.issues.set(issues);
        this.notes.set(result.notes);
        this.loading.set(false);
        this.loadMilestones(projects.slice(0, 4));
      },
      error: () => this.loading.set(false)
    });
    if (this.auth.isStaff()) {
      const question = this.auth.isDesigner()
        ? 'Which of my assigned issues are still open, and which project needs attention?'
        : 'What is the validation pass rate, and which projects need attention?';
      this.api.ask(question).subscribe({
        next: response => this.insight.set(response.answer),
        error: () => this.insight.set('Sign in with the API running to load a summary from stored validation results.')
      });
    }
  }

  money = money;
  prettyDate = prettyDate;
  relative = relativeTime;
  tone = tone;
  labelize = labelize;

  private loadMilestones(projects: ProjectListItem[]): void {
    if (!projects.length) {
      return;
    }
    forkJoin(projects.map(item => this.api.project(item.projectId))).subscribe({
      next: details => {
        const miles = details.flatMap((detail: ProjectDetail) =>
          detail.milestones.map(mile => ({ ...mile, project: detail.code, projectId: detail.projectId }))
        );
        miles.sort((a, b) => (a.dueDate ?? '9999').localeCompare(b.dueDate ?? '9999'));
        this.milestones.set(miles.filter(item => item.status !== 'Completed').slice(0, 5));
      }
    });
  }

  private statusSlices(projects: ProjectListItem[]): Slice[] {
    const colors = ['#4f46e5', '#14b8a6', '#d97706', '#e11d48', '#64748b', '#7c3aed'];
    const counts = new Map<string, number>();
    for (const project of projects) {
      counts.set(project.status, (counts.get(project.status) ?? 0) + 1);
    }
    return [...counts.entries()].map(([label, count], index) => ({
      label,
      count,
      color: colors[index % colors.length]
    }));
  }

  private conic(slices: Slice[]): string {
    const total = slices.reduce((sum, item) => sum + item.count, 0) || 1;
    let cursor = 0;
    const parts = slices.map(item => {
      const start = cursor;
      cursor += (item.count / total) * 100;
      return `${item.color} ${start}% ${cursor}%`;
    });
    return `conic-gradient(${parts.join(',') || '#e7e5e4 0 100%'})`;
  }
}
