import { HttpErrorResponse } from '@angular/common/http';

export function errorMessage(error: unknown, fallback = 'Something went wrong. Try again.'): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as { detail?: string; title?: string } | string | null;
    if (typeof body === 'string' && body.trim()) {
      return body;
    }
    if (body && typeof body === 'object') {
      return body.detail || body.title || fallback;
    }
    if (error.status === 0) {
      return 'The API is not running. Start it on http://localhost:5080.';
    }
  }
  return fallback;
}

export function money(value: number, currency = 'USD'): string {
  try {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency || 'USD',
      maximumFractionDigits: 0
    }).format(value || 0);
  } catch {
    return `$${Math.round(value || 0).toLocaleString()}`;
  }
}

export function prettyDate(value?: string | null): string {
  if (!value) {
    return '—';
  }
  const dateOnly = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  const date = dateOnly
    ? new Date(Number(dateOnly[1]), Number(dateOnly[2]) - 1, Number(dateOnly[3]))
    : new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

export function relativeTime(value?: string | null): string {
  if (!value) {
    return '';
  }
  const date = new Date(value);
  const diff = Date.now() - date.getTime();
  const minutes = Math.round(diff / 60000);
  if (Number.isNaN(minutes)) {
    return '';
  }
  if (minutes < 1) {
    return 'just now';
  }
  if (minutes < 60) {
    return `${minutes}m ago`;
  }
  const hours = Math.round(minutes / 60);
  if (hours < 24) {
    return `${hours}h ago`;
  }
  const days = Math.round(hours / 24);
  if (days < 14) {
    return `${days}d ago`;
  }
  return prettyDate(value);
}

export function initials(name?: string | null): string {
  if (!name) {
    return 'DF';
  }
  const parts = name.trim().split(/\s+/).slice(0, 2);
  return parts.map(part => part[0]?.toUpperCase() ?? '').join('') || 'DF';
}

export function tone(status?: string | null): string {
  const key = (status ?? '').toLowerCase();
  if (['active', 'done', 'resolved', 'closed', 'succeeded', 'passed', 'pass', 'approved', 'synced', 'ready', 'completed'].includes(key)) {
    return 'good';
  }
  if (['needsattention', 'critical', 'high', 'failed', 'fail', 'rejected', 'overdue'].includes(key)) {
    return 'bad';
  }
  if (['inreview', 'inprogress', 'medium', 'assigned', 'running', 'translating', 'pending', 'proposed', 'warning'].includes(key)) {
    return 'warn';
  }
  if (['low', 'info', 'draft', 'todo', 'queued', 'prospect'].includes(key)) {
    return 'info';
  }
  return 'neutral';
}

export function labelize(value?: string | null): string {
  if (!value) {
    return '—';
  }
  return value
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/Needs Attention/i, 'Needs attention');
}

export function looksLikeRevitCommand(text: string): boolean {
  const value = text.toLowerCase();
  return (value.includes('fire') && value.includes('door'))
    || value.includes('unplaced')
    || value.includes('sheet')
    || (value.includes('room') && (value.includes('name') || value.includes('number')));
}
