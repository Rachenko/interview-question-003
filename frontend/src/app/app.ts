import { Component } from '@angular/core';
import { ApprovalList } from './approval-list/approval-list';

@Component({
  imports: [ApprovalList],
  selector: 'app-root',
  template: '<app-approval-list />',
})
export class App {}
