import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { BoardTask, ProjectListItem } from '../../core/models';
import { errorMessage, labelize, prettyDate, tone } from '../../core/format';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-tasks',
  imports: [FormsModule],
  templateUrl: './tasks.component.html'
})
export class TasksComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly tasks = signal<BoardTask[]>([]);
  readonly lanes = ['ToDo', 'InProgress', 'InReview', 'Done'];
  projectId = 'all';
  private dragging: BoardTask | null = null;
  readonly over = signal('');

  constructor() {
    this.api.projects().subscribe({
      next: projects => {
        const scope = this.auth.isDesigner() ? projects.filter(project => this.auth.leads(project)) : projects;
        this.projects.set(scope);
        if (!scope.length) {
          this.loading.set(false);
          return;
        }
        forkJoin(scope.map(project => this.api.project(project.projectId))).subscribe({
          next: details => {
            const cards = details.flatMap(detail => detail.tasks
              .filter(task => ['ToDo', 'InProgress', 'InReview', 'Done'].includes(task.status))
              .filter(task => !this.auth.isDesigner() || this.auth.isMine(task.assigneeName))
              .map(task => ({ ...task, projectId: detail.projectId, projectCode: detail.code, projectName: detail.name })));
            this.tasks.set(cards);
            this.loading.set(false);
          },
          error: error => {
            this.loading.set(false);
            this.toast.error(errorMessage(error));
          }
        });
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(errorMessage(error));
      }
    });
  }

  laneTasks(status: string): BoardTask[] {
    return this.tasks().filter(task => task.status === status && (this.projectId === 'all' || task.projectId === this.projectId));
  }

  drag(task: BoardTask): void {
    this.dragging = task;
  }

  drop(status: string): void {
    const task = this.dragging;
    this.over.set('');
    this.dragging = null;
    if (!task || task.status === status) {
      return;
    }
    const previous = task.status;
    this.tasks.update(items => items.map(item => item.taskId === task.taskId ? { ...item, status } : item));
    this.api.updateTaskStatus(task.taskId, status).subscribe({
      error: error => {
        this.tasks.update(items => items.map(item => item.taskId === task.taskId ? { ...item, status: previous } : item));
        this.toast.error(errorMessage(error));
      }
    });
  }

  labelize = labelize;
  prettyDate = prettyDate;
  tone = tone;
}
