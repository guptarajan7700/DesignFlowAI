import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Client, ProjectListItem, TeamMember } from '../../core/models';
import { errorMessage, labelize, money, prettyDate, tone } from '../../core/format';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-projects',
  imports: [FormsModule, RouterLink],
  templateUrl: './projects.component.html'
})
export class ProjectsComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly clients = signal<Client[]>([]);
  readonly people = signal<TeamMember[]>([]);
  readonly open = signal(false);
  readonly view = signal<'cards' | 'table'>(localStorage.getItem('df_projects_view') === 'table' ? 'table' : 'cards');
  query = '';
  status = 'All';

  clientId = '';
  code = '';
  name = '';
  description = '';
  budget = 0;
  dueDate = '';
  projectManagerId = '';
  leadDesignerId = '';

  constructor() {
    this.refresh();
    if (this.auth.isManager()) {
      this.api.clients().subscribe({ next: items => this.clients.set(items), error: () => this.clients.set([]) });
      this.api.users().subscribe({ next: items => this.people.set(items), error: () => this.people.set([]) });
    }
  }

  get filtered(): ProjectListItem[] {
    const q = this.query.trim().toLowerCase();
    return this.projects().filter(project => {
      const matchesRole = !this.auth.isDesigner() || this.auth.leads(project);
      const matchesQuery = !q || `${project.name} ${project.code} ${project.clientName}`.toLowerCase().includes(q);
      const matchesStatus = this.status === 'All' || project.status === this.status;
      return matchesRole && matchesQuery && matchesStatus;
    });
  }

  setView(view: 'cards' | 'table'): void {
    this.view.set(view);
    localStorage.setItem('df_projects_view', view);
  }

  spentRatio(project: ProjectListItem): number {
    if (!project.budget) {
      return 0;
    }
    return Math.min(100, Math.round((project.spent / project.budget) * 100));
  }

  save(): void {
    this.api.createProject({
      clientId: this.clientId,
      code: this.code,
      name: this.name,
      description: this.description || null,
      status: 'Draft',
      dueDate: this.dueDate || null,
      budget: Number(this.budget) || 0,
      spent: 0,
      currency: 'USD',
      projectManagerId: this.projectManagerId || null,
      leadDesignerId: this.leadDesignerId || null
    }).subscribe({
      next: project => {
        this.open.set(false);
        this.toast.success(`${project.code} is on the board.`);
        void this.router.navigate(['/projects', project.projectId]);
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }

  money = money;
  prettyDate = prettyDate;
  tone = tone;
  labelize = labelize;
  managers = () => this.people().filter(person => person.roles.includes('ProjectManager') || person.roles.includes('Admin'));
  designers = () => this.people().filter(person => person.roles.includes('Designer'));

  private refresh(): void {
    this.api.projects().subscribe({
      next: items => {
        this.projects.set(items);
        this.loading.set(false);
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(errorMessage(error));
      }
    });
  }
}
