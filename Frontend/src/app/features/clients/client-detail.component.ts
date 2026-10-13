import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Client, ProjectListItem } from '../../core/models';
import { money, prettyDate, tone, labelize } from '../../core/format';

@Component({
  selector: 'app-client-detail',
  imports: [RouterLink],
  templateUrl: './client-detail.component.html'
})
export class ClientDetailComponent {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  readonly client = signal<Client | null>(null);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly missing = signal(false);
  readonly tab = signal<'overview' | 'projects'>('overview');

  readonly related = computed(() => this.projects().filter(project => project.clientName === this.client()?.companyName));

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    this.api.clients().subscribe({
      next: items => {
        const match = items.find(item => item.clientId === id) ?? null;
        this.client.set(match);
        this.missing.set(!match);
      },
      error: () => this.missing.set(true)
    });
    this.api.projects().subscribe({ next: items => this.projects.set(items), error: () => this.projects.set([]) });
  }

  money = money;
  prettyDate = prettyDate;
  tone = tone;
  labelize = labelize;
}
