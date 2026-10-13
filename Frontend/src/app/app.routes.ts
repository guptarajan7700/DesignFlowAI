import { Routes } from '@angular/router';
import { authGuard, guestGuard, managerGuard, staffGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell.component').then(m => m.ShellComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'clients', canActivate: [managerGuard], loadComponent: () => import('./features/clients/clients.component').then(m => m.ClientsComponent) },
      { path: 'clients/:id', canActivate: [managerGuard], loadComponent: () => import('./features/clients/client-detail.component').then(m => m.ClientDetailComponent) },
      { path: 'projects', loadComponent: () => import('./features/projects/projects.component').then(m => m.ProjectsComponent) },
      { path: 'projects/:id', loadComponent: () => import('./features/projects/project-detail.component').then(m => m.ProjectDetailComponent) },
      { path: 'tasks', canActivate: [staffGuard], loadComponent: () => import('./features/tasks/tasks.component').then(m => m.TasksComponent) },
      { path: 'issues', canActivate: [staffGuard], loadComponent: () => import('./features/issues/issues.component').then(m => m.IssuesComponent) },
      { path: 'viewer', loadComponent: () => import('./features/viewer/viewer.component').then(m => m.ViewerComponent) },
      { path: 'assistant', canActivate: [staffGuard], loadComponent: () => import('./features/assistant/assistant.component').then(m => m.AssistantComponent) },
      { path: 'automation', canActivate: [managerGuard], loadComponent: () => import('./features/automation/automation.component').then(m => m.AutomationComponent) },
      { path: 'reports', loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent) },
      { path: 'settings', loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent) }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
