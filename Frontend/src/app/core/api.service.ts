import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import {
  AnalyzeBriefResponse,
  AskResponse,
  AutomationJob,
  Client,
  CommandPreview,
  ExecuteCommandResponse,
  IntegrationStatus,
  Issue,
  LoginResponse,
  PlanResponse,
  ProjectDetail,
  ProjectListItem,
  ProjectReport,
  Requirement,
  StudioNotification,
  TaskItem,
  TeamMember,
  ViewerSession
} from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  readonly baseUrl = 'http://localhost:5080';

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${this.baseUrl}/api/auth/login`, { email, password });
  }

  clients() {
    return this.http.get<Client[]>(`${this.baseUrl}/api/clients`);
  }

  createClient(body: Record<string, unknown>) {
    return this.http.post<Client>(`${this.baseUrl}/api/clients`, body);
  }

  projects() {
    return this.http.get<ProjectListItem[]>(`${this.baseUrl}/api/projects`);
  }

  project(id: string) {
    return this.http.get<ProjectDetail>(`${this.baseUrl}/api/projects/${id}`);
  }

  createProject(body: Record<string, unknown>) {
    return this.http.post<ProjectDetail>(`${this.baseUrl}/api/projects`, body);
  }

  addRequirement(projectId: string, body: Record<string, unknown>) {
    return this.http.post<Requirement>(`${this.baseUrl}/api/projects/${projectId}/requirements`, body);
  }

  plan(projectId: string, brief: string) {
    return this.http.post<PlanResponse>(`${this.baseUrl}/api/projects/${projectId}/plan`, { brief });
  }

  approvePlan(projectId: string, comments: string) {
    return this.http.post<PlanResponse>(`${this.baseUrl}/api/projects/${projectId}/task-plans/approve`, { comments });
  }

  rejectPlan(projectId: string, comments: string) {
    return this.http.post<PlanResponse>(`${this.baseUrl}/api/projects/${projectId}/task-plans/reject`, { comments });
  }

  report(projectId: string) {
    return this.http.get<ProjectReport>(`${this.baseUrl}/api/projects/${projectId}/reports`);
  }

  issues(filters?: { projectId?: string; severity?: string; status?: string }) {
    let params = new HttpParams();
    if (filters?.projectId) {
      params = params.set('projectId', filters.projectId);
    }
    if (filters?.severity) {
      params = params.set('severity', filters.severity);
    }
    if (filters?.status) {
      params = params.set('status', filters.status);
    }
    return this.http.get<Issue[]>(`${this.baseUrl}/api/issues`, { params });
  }

  assignIssue(issueId: string, assigneeId: string) {
    return this.http.post<Issue>(`${this.baseUrl}/api/issues/${issueId}/assign`, { assigneeId });
  }

  users() {
    return this.http.get<TeamMember[]>(`${this.baseUrl}/api/users`);
  }

  notifications() {
    return this.http.get<StudioNotification[]>(`${this.baseUrl}/api/notifications`);
  }

  markNotificationRead(id: string) {
    return this.http.post(`${this.baseUrl}/api/notifications/${id}/read`, {});
  }

  createTask(projectId: string, body: Record<string, unknown>) {
    return this.http.post<TaskItem>(`${this.baseUrl}/api/projects/${projectId}/tasks`, body);
  }

  updateTaskStatus(taskId: string, status: string) {
    return this.http.patch<TaskItem>(`${this.baseUrl}/api/tasks/${taskId}/status`, { status });
  }

  jobs() {
    return this.http.get<AutomationJob[]>(`${this.baseUrl}/api/automation/jobs`);
  }

  job(id: string) {
    return this.http.get<AutomationJob>(`${this.baseUrl}/api/automation/jobs/${id}`);
  }

  enqueueJob(body: Record<string, unknown>) {
    return this.http.post<AutomationJob>(`${this.baseUrl}/api/automation/jobs`, body);
  }

  analyze(brief: string, projectId?: string) {
    return this.http.post<AnalyzeBriefResponse>(`${this.baseUrl}/api/ai/requirements/analyze`, {
      brief,
      projectId: projectId || null
    });
  }

  previewCommand(prompt: string, projectId?: string) {
    return this.http.post<CommandPreview>(`${this.baseUrl}/api/ai/commands/preview`, {
      prompt,
      projectId: projectId || null
    });
  }

  executeCommand(previewId: string) {
    return this.http.post<ExecuteCommandResponse>(`${this.baseUrl}/api/ai/commands/execute`, {
      previewId,
      confirm: true
    });
  }

  ask(question: string) {
    return this.http.post<AskResponse>(`${this.baseUrl}/api/ai/ask`, { question });
  }

  integrations() {
    return this.http.get<IntegrationStatus>(`${this.baseUrl}/api/system/integrations`);
  }

  syncModel(body: Record<string, unknown>) {
    return this.http.post(`${this.baseUrl}/api/revit/models/sync`, body);
  }

  viewer(projectId?: string) {
    let params = new HttpParams();
    if (projectId) {
      params = params.set('projectId', projectId);
    }
    return this.http.get<ViewerSession>(`${this.baseUrl}/api/revit/viewer`, { params });
  }
}
