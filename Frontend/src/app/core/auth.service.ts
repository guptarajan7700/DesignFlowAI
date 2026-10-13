import { Injectable, computed, signal } from '@angular/core';
import { LoginResponse, UserProfile } from './models';

const TOKEN_KEY = 'df_token';
const USER_KEY = 'df_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenState = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly userState = signal<UserProfile | null>(this.readUser());

  readonly token = this.tokenState.asReadonly();
  readonly user = this.userState.asReadonly();
  readonly isLoggedIn = computed(() => !!this.tokenState());
  readonly roles = computed(() => this.userState()?.roles ?? []);
  readonly isManager = computed(() => this.roles().some(role => role === 'Admin' || role === 'ProjectManager'));
  readonly isStaff = computed(() => this.roles().some(role => role === 'Admin' || role === 'ProjectManager' || role === 'Designer'));
  readonly isDesigner = computed(() => this.roles().includes('Designer') && !this.isManager());
  readonly isClient = computed(() => this.isLoggedIn() && !this.isStaff());
  readonly displayName = computed(() => {
    const person = this.userState();
    return person ? `${person.firstName} ${person.lastName}`.trim() : 'Guest';
  });

  setSession(response: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, response.accessToken);
    localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    this.tokenState.set(response.accessToken);
    this.userState.set(response.user);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.tokenState.set(null);
    this.userState.set(null);
  }

  hasRole(...roles: string[]): boolean {
    return roles.some(role => this.roles().includes(role));
  }

  leads(project: { leadDesignerName?: string | null }): boolean {
    return (project.leadDesignerName ?? '').toLowerCase() === this.displayName().toLowerCase();
  }

  isMine(name?: string | null): boolean {
    return (name ?? '').toLowerCase() === this.displayName().toLowerCase();
  }

  private readUser(): UserProfile | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as UserProfile;
    } catch {
      return null;
    }
  }
}
