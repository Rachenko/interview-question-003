import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

export type DecisionAction = 'approve' | 'reject';

@Component({
  selector: 'app-decision-modal',
  imports: [FormsModule],
  templateUrl: './decision-modal.html',
  styleUrl: './decision-modal.css',
})
export class DecisionModal {
  @Input({ required: true }) action!: DecisionAction;
  @Output() confirmed = new EventEmitter<string>();
  @Output() cancelled = new EventEmitter<void>();

  reason = '';

  get title(): string {
    return this.action === 'approve' ? 'ยืนยันการอนุมัติ' : 'ยืนยันการไม่อนุมัติ';
  }

  get confirmLabel(): string {
    return this.action === 'approve' ? 'อนุมัติ' : 'ไม่อนุมัติ';
  }

  confirm(): void {
    const reason = this.reason.trim();
    if (!reason) {
      return;
    }
    this.confirmed.emit(reason);
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
