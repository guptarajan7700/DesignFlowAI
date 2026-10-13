import { Component, input } from '@angular/core';

@Component({
  selector: 'app-icon',
  template: `
    <svg viewBox="0 0 24 24" aria-hidden="true">
      @switch (name()) {
        @case ('dashboard') { <path d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-5v-6H10v6H5a1 1 0 0 1-1-1z"/> }
        @case ('clients') { <path d="M16 20v-1.2A3.8 3.8 0 0 0 12.2 15H7.8A3.8 3.8 0 0 0 4 18.8V20"/><circle cx="10" cy="8" r="3"/><path d="M20 20v-1.1A3.2 3.2 0 0 0 17.4 16"/><path d="M16 5.2a3 3 0 0 1 0 5.6"/> }
        @case ('projects') { <path d="M3 7.5A2.5 2.5 0 0 1 5.5 5H9l2 2h7.5A2.5 2.5 0 0 1 21 9.5v8A2.5 2.5 0 0 1 18.5 20h-13A2.5 2.5 0 0 1 3 17.5z"/> }
        @case ('kanban') { <path d="M5 4h4v16H5zM15 4h4v10h-4z"/> }
        @case ('issues') { <path d="M12 4 3.5 19h17z"/><path d="M12 10v4"/><path d="M12 16.5h.01"/> }
        @case ('cube') { <path d="m12 3 8 4.5v9L12 21l-8-4.5v-9z"/><path d="M12 12 20 7.5"/><path d="M12 12v9"/><path d="M12 12 4 7.5"/> }
        @case ('spark') { <path d="M12 3.5 13.6 9 19 10.5 13.6 12 12 17.5 10.4 12 5 10.5 10.4 9z"/><path d="M18 15.5 18.7 17.3 20.5 18 18.7 18.7 18 20.5 17.3 18.7 15.5 18 17.3 17.3z"/> }
        @case ('automation') { <path d="M12 7V4"/><path d="M8 20h8"/><rect x="7" y="7" width="10" height="9" rx="2"/><path d="M10 11h4"/> }
        @case ('reports') { <path d="M5 19V9"/><path d="M10 19V5"/><path d="M15 19v-7"/><path d="M20 19V8"/> }
        @case ('settings') { <circle cx="12" cy="12" r="3"/><path d="M12 3.5v2.2M12 18.3v2.2M4.8 6.2l1.6 1.6M17.6 16.2l1.6 1.6M3.5 12h2.2M18.3 12h2.2M4.8 17.8l1.6-1.6M17.6 7.8l1.6-1.6"/> }
        @case ('search') { <circle cx="11" cy="11" r="6"/><path d="m20 20-3.5-3.5"/> }
        @case ('bell') { <path d="M6 16V11a6 6 0 1 1 12 0v5l1.5 2H4.5z"/><path d="M10 19a2 2 0 0 0 4 0"/> }
        @case ('sun') { <circle cx="12" cy="12" r="3.5"/><path d="M12 3v2M12 19v2M3 12h2M19 12h2M5.2 5.2l1.4 1.4M17.4 17.4l1.4 1.4M18.8 5.2l-1.4 1.4M6.6 17.4l-1.4 1.4"/> }
        @case ('moon') { <path d="M16 3.5A8.5 8.5 0 1 0 20.5 14 7 7 0 0 1 16 3.5z"/> }
        @case ('menu') { <path d="M4 7h16M4 12h16M4 17h16"/> }
        @case ('plus') { <path d="M12 5v14M5 12h14"/> }
        @case ('logout') { <path d="M10 7V5a2 2 0 0 1 2-2h7v18h-7a2 2 0 0 1-2-2v-2"/><path d="M4 12h11"/><path d="m12 8 4 4-4 4"/> }
        @case ('check') { <path d="m5 12 5 5L20 7"/> }
        @case ('x') { <path d="M6 6l12 12M18 6 6 18"/> }
        @case ('grid') { <path d="M4 4h7v7H4zM13 4h7v7h-7zM4 13h7v7H4zM13 13h7v7h-7z"/> }
        @case ('list') { <path d="M8 7h12M8 12h12M8 17h12"/><path d="M4 7h.01M4 12h.01M4 17h.01"/> }
        @case ('send') { <path d="m4 12 16-7-6 16-2-7z"/> }
        @case ('chevron') { <path d="m9 6 6 6-6 6"/> }
      }
    </svg>
  `,
  styles: [`
    :host { display: inline-flex; }
    svg {
      width: 1.15rem;
      height: 1.15rem;
      fill: none;
      stroke: currentColor;
      stroke-width: 1.75;
      stroke-linecap: round;
      stroke-linejoin: round;
    }
  `]
})
export class IconComponent {
  readonly name = input.required<string>();
}
