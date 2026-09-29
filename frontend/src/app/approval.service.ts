import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type ApprovalStatus = 'Pending' | 'Approved' | 'Rejected';

export interface ApprovalDocument {
  id: number;
  title: string;
  status: ApprovalStatus;
  reason: string | null;
  createdAt: string;
  decidedAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5204/api/approval-documents';

  getAll(): Observable<ApprovalDocument[]> {
    return this.http.get<ApprovalDocument[]>(this.baseUrl);
  }

  approve(documentIds: number[], reason: string): Observable<{ updated: number }> {
    return this.http.post<{ updated: number }>(`${this.baseUrl}/approve`, { documentIds, reason });
  }

  reject(documentIds: number[], reason: string): Observable<{ updated: number }> {
    return this.http.post<{ updated: number }>(`${this.baseUrl}/reject`, { documentIds, reason });
  }
}
