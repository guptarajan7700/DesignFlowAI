import { Component, inject, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { ThemeService } from '../../core/theme.service';
import { IntegrationStatus } from '../../core/models';
import { errorMessage, labelize } from '../../core/format';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html'
})
export class SettingsComponent {
  readonly auth = inject(AuthService);
  readonly theme = inject(ThemeService);
  private readonly api = inject(ApiService);
  readonly integrations = signal<IntegrationStatus | null>(null);
  readonly note = signal('');
  labelize = labelize;

  constructor() {
    if (this.auth.isManager()) {
      this.api.integrations().subscribe({
        next: status => this.integrations.set(status),
        error: error => this.note.set(errorMessage(error))
      });
    }
  }
}
