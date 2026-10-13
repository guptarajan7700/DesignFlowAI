import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { AnalyzeBriefResponse, Issue, ProjectDetail, ProjectReport, TeamMember } from '../../core/models';
import { errorMessage, labelize, money, prettyDate, tone } from '../../core/format';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-project-detail',
  imports: [FormsModule, RouterLink],
  templateUrl: './project-detail.component.html'
})
export class ProjectDetailComponent {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);

  readonly project = signal<ProjectDetail | null>(null);
  readonly issues = signal<Issue[]>([]);
  readonly report = signal<ProjectReport | null>(null);
  readonly analysis = signal<AnalyzeBriefResponse | null>(null);
  readonly people = signal<TeamMember[]>([]);
  readonly missing = signal(false);
  readonly tab = signal('overview');
  readonly busy = signal(false);

  brief = '';
  comments = 'Approved from the studio board.';
  reqTitle = '';
  reqCategory = 'General';
  reqPriority = 'Medium';
  taskTitle = '';
  taskPriority = 'Medium';
  taskAssignee = '';
  taskDue = '';
  modelName = '';
  modelFile = '';

  get tabs(): string[] {
    const tabs = ['overview', 'requirements'];
    if (this.auth.isStaff()) {
      tabs.push('tasks');
    }
    tabs.push('models');
    if (this.auth.isStaff()) {
      tabs.push('issues');
    }
    tabs.push('reports');
    if (this.auth.isManager()) {
      tabs.push('approvals');
    }
    return tabs;
  }

  constructor() {
    this.load();
    if (this.auth.isStaff()) {
      this.api.users().subscribe({ next: items => this.people.set(items), error: () => this.people.set([]) });
    }
  }

  proposed() {
    return (this.project()?.tasks ?? []).filter(task => task.status === 'Proposed');
  }

  myIssues() {
    return this.issues().filter(issue => this.auth.isMine(issue.assignedToName));
  }

  activeTasks() {
    const tasks = (this.project()?.tasks ?? []).filter(task => task.status !== 'Proposed' && task.status !== 'Rejected');
    return this.auth.isDesigner() ? tasks.filter(task => this.auth.isMine(task.assigneeName)) : tasks;
  }

  designers() {
    return this.people().filter(person => person.roles.includes('Designer'));
  }

  analyze(): void {
    const project = this.project();
    if (!project || !this.brief.trim()) {
      return;
    }
    this.busy.set(true);
    this.api.analyze(this.brief.trim(), project.projectId).subscribe({
      next: result => {
        this.analysis.set(result);
        this.busy.set(false);
        this.toast.success('Brief analyzed and saved onto the project.');
        this.load();
      },
      error: error => {
        this.busy.set(false);
        this.toast.error(errorMessage(error));
      }
    });
  }

  plan(): void {
    const project = this.project();
    if (!project) {
      return;
    }
    this.busy.set(true);
    this.api.plan(project.projectId, this.brief.trim()).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success('Proposed tasks are waiting for a manager.');
        this.tab.set('approvals');
        this.load();
      },
      error: error => {
        this.busy.set(false);
        this.toast.error(errorMessage(error));
      }
    });
  }

  approve(accept: boolean): void {
    const project = this.project();
    if (!project) {
      return;
    }
    const call = accept
      ? this.api.approvePlan(project.projectId, this.comments)
      : this.api.rejectPlan(project.projectId, this.comments);
    call.subscribe({
      next: () => {
        this.toast.success(accept ? 'Plan approved. Tasks are now real work.' : 'Plan rejected.');
        this.load();
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }

  addRequirement(): void {
    const project = this.project();
    if (!project || !this.reqTitle.trim()) {
      return;
    }
    this.api.addRequirement(project.projectId, {
      title: this.reqTitle.trim(),
      category: this.reqCategory,
      priority: this.reqPriority,
      source: 'Manual'
    }).subscribe({
      next: () => {
        this.reqTitle = '';
        this.toast.success('Requirement added.');
        this.load();
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }

  addTask(): void {
    const project = this.project();
    if (!project || !this.taskTitle.trim()) {
      return;
    }
    this.api.createTask(project.projectId, {
      title: this.taskTitle.trim(),
      priority: this.taskPriority,
      assigneeId: this.taskAssignee || null,
      dueDate: this.taskDue || null
    }).subscribe({
      next: () => {
        this.taskTitle = '';
        this.toast.success('Task added to To Do.');
        this.load();
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }

  syncModel(): void {
    const project = this.project();
    if (!project || !this.modelName.trim() || !this.modelFile.trim()) {
      return;
    }
    this.api.syncModel({
      projectId: project.projectId,
      name: this.modelName.trim(),
      fileName: this.modelFile.trim()
    }).subscribe({
      next: () => {
        this.modelName = '';
        this.modelFile = '';
        this.toast.success('Model recorded.');
        this.load();
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }

  show(tab: string): void {
    this.tab.set(tab);
    if (tab === 'reports' && !this.report()) {
      this.loadReport();
    }
  }

  money = money;
  prettyDate = prettyDate;
  tone = tone;
  labelize = labelize;

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.missing.set(true);
      return;
    }
    this.api.project(id).subscribe({
      next: project => this.project.set(project),
      error: () => this.missing.set(true)
    });
    this.api.issues({ projectId: id }).subscribe({
      next: items => this.issues.set(items),
      error: () => this.issues.set([])
    });
  }

  private loadReport(): void {
    const id = this.project()?.projectId;
    if (!id) {
      return;
    }
    this.api.report(id).subscribe({
      next: report => this.report.set(report),
      error: error => this.toast.error(errorMessage(error))
    });
  }
}
