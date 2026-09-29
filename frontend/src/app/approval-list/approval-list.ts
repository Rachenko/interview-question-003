import { Component, OnInit, inject, signal } from '@angular/core';
import { ApprovalDocument, ApprovalService } from '../approval.service';
import { DecisionModal, DecisionAction } from '../decision-modal/decision-modal';

const STATUS_LABEL: Record<string, string> = {
  Pending: 'รออนุมัติ',
  Approved: 'อนุมัติ',
  Rejected: 'ไม่อนุมัติ',
};

@Component({
  selector: 'app-approval-list',
  imports: [DecisionModal],
  templateUrl: './approval-list.html',
  styleUrl: './approval-list.css',
})
export class ApprovalList implements OnInit {
  private readonly service = inject(ApprovalService);

  readonly documents = signal<ApprovalDocument[]>([]);
  readonly selected = signal<Set<number>>(new Set());
  readonly pendingAction = signal<DecisionAction | null>(null);
  readonly errorMessage = signal<string>('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.service.getAll().subscribe({
      next: (docs) => {
        this.documents.set(docs);
        this.selected.set(new Set());
      },
      error: () => this.errorMessage.set('ไม่สามารถโหลดข้อมูลได้'),
    });
  }

  statusLabel(doc: ApprovalDocument): string {
    return STATUS_LABEL[doc.status] ?? doc.status;
  }

  isSelectable(doc: ApprovalDocument): boolean {
    return doc.status === 'Pending';
  }

  isSelected(doc: ApprovalDocument): boolean {
    return this.selected().has(doc.id);
  }

  toggle(doc: ApprovalDocument): void {
    if (!this.isSelectable(doc)) return;
    const next = new Set(this.selected());
    if (next.has(doc.id)) {
      next.delete(doc.id);
    } else {
      next.add(doc.id);
    }
    this.selected.set(next);
  }

  get hasSelection(): boolean {
    return this.selected().size > 0;
  }

  openModal(action: DecisionAction): void {
    if (!this.hasSelection) return;
    this.pendingAction.set(action);
  }

  closeModal(): void {
    this.pendingAction.set(null);
  }

  confirmDecision(reason: string): void {
    const action = this.pendingAction();
    if (!action) return;

    const ids = [...this.selected()];
    const request$ = action === 'approve'
      ? this.service.approve(ids, reason)
      : this.service.reject(ids, reason);

    request$.subscribe({
      next: () => {
        this.closeModal();
        this.load();
      },
      error: (err) => {
        this.closeModal();
        this.errorMessage.set(err?.error?.error ?? 'เกิดข้อผิดพลาด');
        this.load();
      },
    });
  }
}
