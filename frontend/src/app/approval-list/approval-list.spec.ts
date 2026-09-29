import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApprovalList } from './approval-list';
import { ApprovalDocument } from '../approval.service';

const API = 'http://localhost:5204/api/approval-documents';

const MOCK_DOCS: ApprovalDocument[] = [
  { id: 1, title: 'รายการที่ 1', status: 'Pending', reason: 'xxxxx', createdAt: '', decidedAt: null },
  { id: 2, title: 'รายการที่ 2', status: 'Approved', reason: 'xxxxx', createdAt: '', decidedAt: '' },
  { id: 3, title: 'รายการที่ 3', status: 'Rejected', reason: 'xxxxx', createdAt: '', decidedAt: '' },
  { id: 4, title: 'รายการที่ 4', status: 'Pending', reason: 'xxxxx', createdAt: '', decidedAt: null },
];

describe('ApprovalList', () => {
  let http: HttpTestingController;

  async function setup() {
    await TestBed.configureTestingModule({
      imports: [ApprovalList],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(ApprovalList);
    fixture.detectChanges();
    http.expectOne(API).flush(MOCK_DOCS);
    fixture.detectChanges();
    return fixture;
  }

  afterEach(() => http.verify());

  it('renders rows with Thai status labels', async () => {
    const fixture = await setup();
    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(4);
    expect(rows[0].textContent).toContain('รออนุมัติ');
    expect(rows[1].textContent).toContain('อนุมัติ');
    expect(rows[2].textContent).toContain('ไม่อนุมัติ');
  });

  it('disables checkbox for already-decided documents', async () => {
    const fixture = await setup();
    const el: HTMLElement = fixture.nativeElement;
    const boxes = el.querySelectorAll<HTMLInputElement>('tbody input[type=checkbox]');
    expect(boxes[0].disabled).toBe(false);
    expect(boxes[1].disabled).toBe(true);
    expect(boxes[2].disabled).toBe(true);
    expect(boxes[3].disabled).toBe(false);
  });

  it('keeps action buttons disabled until a pending row is selected', async () => {
    const fixture = await setup();
    const approveBtn: HTMLButtonElement = fixture.nativeElement.querySelector('.btn-approve');
    expect(approveBtn.disabled).toBe(true);

    const component = fixture.componentInstance;
    component.toggle(MOCK_DOCS[0]);
    fixture.detectChanges();
    expect(approveBtn.disabled).toBe(false);
  });

  it('does not allow selecting a decided document', async () => {
    const fixture = await setup();
    const component = fixture.componentInstance;
    component.toggle(MOCK_DOCS[1]);
    expect(component.selected().size).toBe(0);
  });

  it('approve flow: opens modal, posts approve with reason, reloads', async () => {
    const fixture = await setup();
    const component = fixture.componentInstance;
    component.toggle(MOCK_DOCS[0]);
    component.toggle(MOCK_DOCS[3]);
    component.openModal('approve');
    fixture.detectChanges();

    const modal = fixture.nativeElement.querySelector('.modal');
    expect(modal).toBeTruthy();
    expect(modal.querySelector('.modal-header').textContent).toContain('ยืนยันการอนุมัติ');

    component.confirmDecision('เหตุผลทดสอบ');
    const req = http.expectOne(`${API}/approve`);
    expect(req.request.body).toEqual({ documentIds: [1, 4], reason: 'เหตุผลทดสอบ' });
    req.flush({ updated: 2 });

    http.expectOne(API).flush(MOCK_DOCS);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.modal')).toBeNull();
  });

  it('reject flow: posts reject with reason', async () => {
    const fixture = await setup();
    const component = fixture.componentInstance;
    component.toggle(MOCK_DOCS[0]);
    component.openModal('reject');
    component.confirmDecision('no');

    const req = http.expectOne(`${API}/reject`);
    expect(req.request.body).toEqual({ documentIds: [1], reason: 'no' });
    req.flush({ updated: 1 });
    http.expectOne(API).flush(MOCK_DOCS);
  });

  it('cancel closes the modal without calling the API', async () => {
    const fixture = await setup();
    const component = fixture.componentInstance;
    component.toggle(MOCK_DOCS[0]);
    component.openModal('approve');
    component.closeModal();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.modal')).toBeNull();
  });
});
