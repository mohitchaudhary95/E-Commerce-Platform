import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast" [class]="'toast-' + toast.type">
          <span class="toast-icon">{{ icons[toast.type] }}</span>
          <span class="toast-message">{{ toast.message }}</span>
          <button class="toast-close" (click)="toastService.dismiss(toast.id)">×</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container { position: fixed; bottom: 24px; right: 24px; z-index: 9999; display: flex; flex-direction: column; gap: 8px; }
    .toast { display: flex; align-items: center; gap: 10px; padding: 12px 16px; border-radius: 8px; min-width: 280px; max-width: 400px; box-shadow: 0 4px 16px rgba(0,0,0,0.15); animation: slideIn 0.3s ease; font-size: 0.9rem; color: #fff; }
    .toast-success { background: #1e8449; }
    .toast-error { background: #922b21; }
    .toast-info { background: #1a5276; }
    .toast-warning { background: #b7770d; }
    .toast-icon { font-size: 1.1rem; }
    .toast-message { flex: 1; }
    .toast-close { background: none; border: none; color: rgba(255,255,255,0.8); cursor: pointer; font-size: 1.2rem; line-height: 1; padding: 0; }
    @keyframes slideIn { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
  `]
})
export class ToastComponent {
  readonly toastService = inject(ToastService);
  readonly icons: Record<string, string> = {
    success: '✅', error: '❌', info: 'ℹ️', warning: '⚠️'
  };
}
