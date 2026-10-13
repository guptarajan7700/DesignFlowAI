import { Component, HostListener, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { ThemeService } from '../core/theme.service';
import { ToastService } from '../core/toast.service';
import { ProjectListItem, StudioNotification } from '../core/models';
import { initials, labelize, relativeTime } from '../core/format';
import { IconComponent } from '../shared/icon.component';
import { AssistantComponent } from '../features/assistant/assistant.component';

interface NavItem {
  path: string;
  label: string;
  icon: string;
  audience?: 'staff' | 'manager';
}

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, IconComponent, AssistantComponent],
  templateUrl: './shell.component.html'
})
export class ShellComponent {
  readonly auth = inject(AuthService);
  readonly theme = inject(ThemeService);
  readonly toasts = inject(ToastService);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);

  readonly collapsed = signal(localStorage.getItem('df_sidebar') === '1');
  readonly mobileOpen = signal(false);
  readonly paletteOpen = signal(false);
  readonly bellOpen = signal(false);
  readonly menuOpen = signal(false);
  readonly aiOpen = signal(false);
  readonly query = signal('');
  readonly crumbs = signal('Dashboard');
  readonly notes = signal<StudioNotification[]>([]);
  readonly projects = signal<ProjectListItem[]>([]);

  readonly nav: NavItem[] = [
    { path: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
    { path: '/clients', label: 'Clients', icon: 'clients', audience: 'manager' },
    { path: '/projects', label: 'Projects', icon: 'projects' },
    { path: '/tasks', label: 'Task board', icon: 'kanban', audience: 'staff' },
    { path: '/issues', label: 'Issues', icon: 'issues', audience: 'staff' },
    { path: '/viewer', label: 'Model studio', icon: 'cube' },
    { path: '/assistant', label: 'AI assistant', icon: 'spark', audience: 'staff' },
    { path: '/automation', label: 'Automation', icon: 'automation', audience: 'manager' },
    { path: '/reports', label: 'Reports', icon: 'reports' },
    { path: '/settings', label: 'Settings', icon: 'settings' }
  ];

  readonly visibleNav = computed(() => this.nav.filter(item => {
    if (item.audience === 'manager') {
      return this.auth.isManager();
    }
    if (item.audience === 'staff') {
      return this.auth.isStaff();
    }
    return true;
  }));
  readonly unread = computed(() => this.notes().filter(item => !item.isRead).length);
  readonly paletteItems = computed(() => {
    const q = this.query().trim().toLowerCase();
    const pages = this.visibleNav().map(item => ({ label: item.label, hint: 'Page', path: item.path }));
    const projects = this.projects().map(item => ({
      label: item.name,
      hint: item.code,
      path: `/projects/${item.projectId}`
    }));
    return [...pages, ...projects].filter(item => !q || `${item.label} ${item.hint}`.toLowerCase().includes(q)).slice(0, 8);
  });
  readonly initials = computed(() => initials(this.auth.displayName()));
  readonly onAssistant = signal(false);

  constructor() {
    this.syncCrumbs(this.router.url);
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(event => {
      const url = (event as NavigationEnd).urlAfterRedirects;
      this.syncCrumbs(url);
      this.mobileOpen.set(false);
      this.paletteOpen.set(false);
    });
    this.api.notifications().subscribe({
      next: items => this.notes.set(items),
      error: () => this.notes.set([])
    });
    this.api.projects().subscribe({
      next: items => this.projects.set(this.auth.isDesigner() ? items.filter(item => this.auth.leads(item)) : items),
      error: () => this.projects.set([])
    });
  }

  toggleSidebar(): void {
    if (window.innerWidth <= 860) {
      this.mobileOpen.update(value => !value);
      return;
    }
    this.collapsed.update(value => {
      localStorage.setItem('df_sidebar', value ? '0' : '1');
      return !value;
    });
  }

  openPalette(): void {
    this.paletteOpen.set(true);
    this.bellOpen.set(false);
    this.menuOpen.set(false);
  }

  go(path: string): void {
    this.paletteOpen.set(false);
    void this.router.navigateByUrl(path);
  }

  mark(note: StudioNotification): void {
    if (!note.isRead) {
      this.api.markNotificationRead(note.notificationId).subscribe({
        next: () => this.notes.update(items => items.map(item => item.notificationId === note.notificationId ? { ...item, isRead: true } : item))
      });
    }
    if (note.projectId) {
      this.bellOpen.set(false);
      void this.router.navigate(['/projects', note.projectId]);
    }
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }

  relative = relativeTime;
  labelize = labelize;

  @HostListener('document:keydown', ['$event'])
  onKey(event: KeyboardEvent): void {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.paletteOpen.update(value => !value);
    }
    if (event.key === 'Escape') {
      this.paletteOpen.set(false);
      this.aiOpen.set(false);
      this.bellOpen.set(false);
      this.menuOpen.set(false);
    }
  }

  private syncCrumbs(url: string): void {
    const clean = url.split('?')[0];
    this.onAssistant.set(clean.startsWith('/assistant'));
    const match = this.nav.find(item => clean === item.path || (item.path !== '/dashboard' && clean.startsWith(item.path)));
    this.crumbs.set(match?.label ?? 'Studio');
  }
}
