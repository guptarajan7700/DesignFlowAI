import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { ThemeService } from '../../core/theme.service';
import { errorMessage } from '../../core/format';
import { IconComponent } from '../../shared/icon.component';

@Component({
  selector: 'app-login',
  imports: [FormsModule, IconComponent],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly theme = inject(ThemeService);

  email = 'maya.chen@designflow.ai';
  password = 'DesignFlow123!';
  readonly busy = signal(false);
  readonly error = signal('');
  readonly personas = [
    { name: 'Maya Chen', role: 'Project manager', email: 'maya.chen@designflow.ai' },
    { name: 'Arjun Mehta', role: 'Designer', email: 'arjun.mehta@designflow.ai' },
    { name: 'Ava Kapoor', role: 'Studio admin', email: 'admin@designflow.ai' },
    { name: 'Rahul Sharma', role: 'Client', email: 'rahul.sharma@northwind.example' }
  ];

  pick(email: string): void {
    this.email = email;
    this.password = 'DesignFlow123!';
    this.error.set('');
  }

  submit(): void {
    this.busy.set(true);
    this.error.set('');
    this.api.login(this.email.trim(), this.password).subscribe({
      next: response => {
        this.auth.setSession(response);
        this.busy.set(false);
        void this.router.navigate(['/dashboard']);
      },
      error: err => {
        this.busy.set(false);
        this.error.set(errorMessage(err, 'Those credentials were not accepted.'));
      }
    });
  }
}
