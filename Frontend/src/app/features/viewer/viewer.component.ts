import { Component, ElementRef, OnDestroy, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { errorMessage, tone } from '../../core/format';
import { Issue, ProjectDetail, ProjectListItem, ViewerSession } from '../../core/models';

interface AutodeskViewer {
  Viewing: {
    Initializer: (options: unknown, callback: () => void) => void;
    Document: {
      load: (urn: string, onSuccess: (doc: AutodeskDocument) => void, onError: (code: number, message: string) => void) => void;
    };
    GuiViewer3D: new (container: HTMLElement) => AutodeskGuiViewer;
  };
}

interface AutodeskDocument {
  getRoot: () => { getDefaultGeometry: () => unknown };
}

interface AutodeskGuiViewer {
  start: () => number;
  finish: () => void;
  loadDocumentNode: (doc: AutodeskDocument, node: unknown) => Promise<unknown>;
  model?: { getBoundingBox: () => { min: Vec3; max: Vec3 } };
  navigation?: {
    toPerspective: () => void;
    setView: (position: unknown, target: unknown) => void;
  };
}

interface Vec3 {
  x: number;
  y: number;
  z: number;
}

@Component({
  selector: 'app-viewer',
  imports: [FormsModule],
  templateUrl: './viewer.component.html'
})
export class ViewerComponent implements OnDestroy {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  readonly host = viewChild<ElementRef<HTMLDivElement>>('viewerHost');

  readonly projects = signal<ProjectListItem[]>([]);
  readonly detail = signal<ProjectDetail | null>(null);
  readonly issues = signal<Issue[]>([]);
  readonly session = signal<ViewerSession | null>(null);
  readonly statusText = signal('Opening the Autodesk Viewer…');
  projectId = this.route.snapshot.queryParamMap.get('project') ?? '';
  private gui: AutodeskGuiViewer | null = null;
  private retries = 0;
  private disposed = false;

  constructor() {
    this.api.projects().subscribe({
      next: items => {
        const visible = this.auth.isDesigner() ? items.filter(item => this.auth.leads(item)) : items;
        this.projects.set(visible);
        if (!this.projectId && visible[0]) {
          this.projectId = visible[0].projectId;
        }
        this.load();
      }
    });
  }

  ngOnDestroy(): void {
    this.disposed = true;
    this.gui?.finish();
  }

  load(): void {
    if (!this.projectId) {
      return;
    }
    this.retries = 0;
    this.gui?.finish();
    this.gui = null;
    this.session.set(null);
    this.statusText.set('Opening the Autodesk Viewer…');
    forkJoin({
      detail: this.api.project(this.projectId),
      issues: this.api.issues({ projectId: this.projectId })
    }).subscribe({
      next: result => {
        this.detail.set(result.detail);
        this.issues.set(result.issues);
        this.openViewer();
      }
    });
  }

  tone = tone;

  private openViewer(): void {
    this.api.viewer(this.projectId).subscribe({
      next: session => {
        if (this.disposed) {
          return;
        }
        this.session.set(session);
        if (session.status === 'Ready' && session.accessToken && session.urn) {
          this.statusText.set(session.message || session.modelName);
          setTimeout(() => void this.mount(session), 0);
          return;
        }
        if (session.status === 'Preparing' && this.retries < 20) {
          this.retries++;
          this.statusText.set(session.message || 'Autodesk is translating the model…');
          setTimeout(() => this.openViewer(), 4000);
          return;
        }
        this.statusText.set(session.message || 'The 3D model is not available.');
      },
      error: error => this.statusText.set(errorMessage(error))
    });
  }

  private async mount(session: ViewerSession): Promise<void> {
    const container = await this.waitForHost();
    if (!container || !session.accessToken || !session.urn || this.disposed) {
      return;
    }
    try {
      await this.loadScript();
    } catch (error) {
      this.statusText.set(error instanceof Error ? error.message : 'The Autodesk Viewer script did not load.');
      return;
    }
    const autodesk = (window as unknown as { Autodesk?: AutodeskViewer }).Autodesk;
    if (!autodesk) {
      this.statusText.set('The Autodesk Viewer script did not load.');
      return;
    }
    const token = session.accessToken;
    const expires = session.expiresIn || 3600;
    autodesk.Viewing.Initializer({
      env: 'AutodeskProduction',
      api: 'streamingV2',
      getAccessToken: (callback: (accessToken: string, expiresIn: number) => void) => callback(token, expires)
    }, () => {
      if (this.disposed) {
        return;
      }
      this.gui?.finish();
      const viewer = new autodesk.Viewing.GuiViewer3D(container);
      const started = viewer.start();
      if (started > 0) {
        this.statusText.set('The viewer could not start in this browser.');
        return;
      }
      this.gui = viewer;
      autodesk.Viewing.Document.load(
        'urn:' + session.urn,
        document => {
          const node = document.getRoot().getDefaultGeometry();
          void viewer.loadDocumentNode(document, node).then(() => this.frameSampleRoom(viewer, session));
        },
        (_code, message) => this.statusText.set(message || 'Autodesk could not open this model.')
      );
    });
  }

  private frameSampleRoom(viewer: AutodeskGuiViewer, session: ViewerSession): void {
    if (session.modelName !== 'Sample studio room' || !viewer.navigation || !viewer.model) {
      return;
    }
    const three = (window as unknown as { THREE?: { Vector3: new (x: number, y: number, z: number) => unknown } }).THREE;
    const box = viewer.model.getBoundingBox();
    if (!three || !box) {
      return;
    }
    const cx = (box.min.x + box.max.x) / 2;
    const cy = (box.min.y + box.max.y) / 2;
    const cz = (box.min.z + box.max.z) / 2;
    viewer.navigation.toPerspective();
    viewer.navigation.setView(
      new three.Vector3(cx - 1.2, cy - 0.15, cz - 0.4),
      new three.Vector3(cx + 0.4, cy - 0.55, cz + 2.2)
    );
  }

  private waitForHost(): Promise<HTMLDivElement | null> {
    return new Promise(resolve => {
      const attempt = (left: number) => {
        const element = this.host()?.nativeElement ?? null;
        if (element || left <= 0 || this.disposed) {
          resolve(element);
          return;
        }
        requestAnimationFrame(() => attempt(left - 1));
      };
      attempt(40);
    });
  }

  private loadScript(): Promise<void> {
    if ((window as unknown as { Autodesk?: AutodeskViewer }).Autodesk) {
      return Promise.resolve();
    }
    if (!document.querySelector('link[data-aps-viewer]')) {
      const css = document.createElement('link');
      css.rel = 'stylesheet';
      css.href = 'https://developer.api.autodesk.com/modelderivative/v2/viewers/7.*/style.min.css';
      css.setAttribute('data-aps-viewer', 'true');
      document.head.appendChild(css);
    }
    return new Promise((resolve, reject) => {
      const existing = document.querySelector('script[data-aps-viewer]');
      if (existing) {
        existing.addEventListener('load', () => resolve());
        return;
      }
      const script = document.createElement('script');
      script.src = 'https://developer.api.autodesk.com/modelderivative/v2/viewers/7.*/viewer3D.min.js';
      script.setAttribute('data-aps-viewer', 'true');
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('Autodesk Viewer could not be downloaded.'));
      document.body.appendChild(script);
    });
  }
}
