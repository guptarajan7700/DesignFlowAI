import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Client, ProjectListItem } from '../../core/models';
import { errorMessage, initials, tone } from '../../core/format';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-clients',
  imports: [FormsModule, RouterLink],
  templateUrl: './clients.component.html'
})
export class ClientsComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly clients = signal<Client[]>([]);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly open = signal(false);
  query = '';
  companyName = '';
  contactName = '';
  email = '';
  phone = '';
  city = '';
  country = 'India';
  status = 'Active';

  get filtered(): Client[] {
    const q = this.query.trim().toLowerCase();
    return this.clients().filter(client => !q || `${client.companyName} ${client.contactName} ${client.city ?? ''}`.toLowerCase().includes(q));
  }

  constructor() {
    this.api.clients().subscribe({
      next: items => {
        this.clients.set(items);
        this.loading.set(false);
      },
      error: error => {
        this.loading.set(false);
        this.toast.error(errorMessage(error));
      }
    });
    this.api.projects().subscribe({ next: items => this.projects.set(items), error: () => this.projects.set([]) });
  }

  projectCount(name: string): number {
    return this.projects().filter(project => project.clientName === name).length;
  }

  initials = initials;
  tone = tone;

  save(): void {
    this.api.createClient({
      companyName: this.companyName,
      contactName: this.contactName,
      email: this.email,
      phone: this.phone || null,
      city: this.city || null,
      country: this.country || null,
      status: this.status
    }).subscribe({
      next: client => {
        this.clients.update(items => [client, ...items]);
        this.open.set(false);
        this.companyName = this.contactName = this.email = this.phone = this.city = '';
        this.toast.success(`${client.companyName} is now a client.`);
      },
      error: error => this.toast.error(errorMessage(error))
    });
  }
}
