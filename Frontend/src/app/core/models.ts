export interface UserProfile {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string | null;
  isActive: boolean;
  roles: string[];
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: UserProfile;
}

export interface Client {
  clientId: string;
  companyName: string;
  contactName: string;
  email: string;
  phone?: string | null;
  city?: string | null;
  country?: string | null;
  status: string;
  portalUserId?: string | null;
}

export interface ProjectListItem {
  projectId: string;
  code: string;
  name: string;
  status: string;
  clientName: string;
  dueDate?: string | null;
  budget: number;
  spent: number;
  currency: string;
  projectManagerName?: string | null;
  leadDesignerName?: string | null;
  openIssueCount: number;
  overdueTaskCount: number;
  proposedTaskCount: number;
}

export interface Requirement {
  requirementId: string;
  title: string;
  description?: string | null;
  category: string;
  priority: string;
  source: string;
  status: string;
  sortOrder: number;
}

export interface Milestone {
  milestoneId: string;
  name: string;
  description?: string | null;
  dueDate?: string | null;
  status: string;
  sortOrder: number;
}

export interface TaskItem {
  taskId: string;
  title: string;
  description?: string | null;
  status: string;
  priority: string;
  source: string;
  assigneeId?: string | null;
  assigneeName?: string | null;
  dueDate?: string | null;
  labels?: string | null;
  approvedAt?: string | null;
}

export interface RevitModel {
  revitModelId: string;
  name: string;
  fileName: string;
  versionLabel?: string | null;
  autodeskUrn?: string | null;
  lastSyncedAt?: string | null;
  status: string;
}

export interface ProjectDetail extends ProjectListItem {
  clientId: string;
  description?: string | null;
  startDate?: string | null;
  siteAddress?: string | null;
  leadDesignerId?: string | null;
  requirements: Requirement[];
  milestones: Milestone[];
  tasks: TaskItem[];
  models: RevitModel[];
}

export interface Issue {
  issueId: string;
  projectId: string;
  projectCode: string;
  title: string;
  description?: string | null;
  severity: string;
  status: string;
  elementId?: string | null;
  suggestedFix?: string | null;
  assignedToId?: string | null;
  assignedToName?: string | null;
  validationRunId?: string | null;
  revitModelId?: string | null;
  createdAt: string;
}

export interface TeamMember {
  userId: string;
  fullName: string;
  email: string;
  roles: string[];
}

export interface StudioNotification {
  notificationId: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  projectId?: string | null;
  createdAt: string;
}

export interface AutomationJob {
  automationJobId: string;
  projectId?: string | null;
  revitModelId?: string | null;
  jobType: string;
  status: string;
  progressPercent: number;
  errorMessage?: string | null;
  resultPayload?: string | null;
  startedAt?: string | null;
  completedAt?: string | null;
  createdAt: string;
}

export interface SuggestedTask {
  title: string;
  priority: string;
  label: string;
}

export interface RequirementAnalysis {
  rooms: string[];
  style: string;
  budget?: number | null;
  constraints: string[];
  materials: string[];
  suggestedTasks: SuggestedTask[];
  reviewChecklist: string[];
}

export interface AnalyzeBriefResponse {
  analysis: RequirementAnalysis;
  savedToProject: boolean;
  savedRequirements: Requirement[];
}

export interface PlanResponse {
  projectId: string;
  approvalStatus: string;
  proposedTasks: TaskItem[];
}

export interface CommandPreview {
  previewId: string;
  detectedCommand?: string | null;
  provider: string;
  allowed: boolean;
  impact: string;
  requiresConfirmation: boolean;
  parametersJson?: string | null;
}

export interface StoredCheck {
  checkCode: string;
  checkName: string;
  status: string;
  elementId?: string | null;
  message: string;
  suggestedFix?: string | null;
}

export interface ExecuteCommandResponse {
  previewId: string;
  command: string;
  status: string;
  message: string;
  storedResults: StoredCheck[];
}

export interface AskResponse {
  answer: string;
  provider: string;
  source: string;
}

export interface Approval {
  approvalId: string;
  entityType: string;
  status: string;
  comments?: string | null;
  requestedAt: string;
  reviewedAt?: string | null;
}

export interface DrawingRevision {
  drawingRevisionId: string;
  sheetNumber: string;
  sheetName: string;
  revisionCode: string;
  description?: string | null;
  issuedAt?: string | null;
}

export interface ProjectReport {
  projectId: string;
  code: string;
  name: string;
  status: string;
  openIssueCount: number;
  overdueTaskCount: number;
  passRatePercent: number;
  aiSummaryLabel: string;
  aiSummary: string;
  validationResults: StoredCheck[];
  approvals: Approval[];
  revisions: DrawingRevision[];
}

export interface IntegrationStatus {
  aiProvider: string;
  autodeskConfigured: boolean;
  authentication: string;
  modelDerivative: string;
  dataManagement: string;
  designAutomation: string;
}

export interface ViewerSession {
  accessToken?: string | null;
  expiresIn: number;
  urn?: string | null;
  modelName: string;
  status: string;
  message?: string | null;
}

export interface BoardTask extends TaskItem {
  projectId: string;
  projectCode: string;
  projectName: string;
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  text: string;
  source?: string;
  preview?: CommandPreview;
  results?: StoredCheck[];
}
