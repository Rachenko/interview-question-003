# Interview Question — IT 03 Approval System

ระบบอนุมัติเอกสารตามโจทย์ IT 03-1 / 03-2 / 03-3

- **Backend:** C# .NET 10 (ASP.NET Core Web API + EF Core)
- **Frontend:** Angular 22 (standalone components, signals)
- **Database:** PostgreSQL 16 (Docker)

## โครงสร้างโปรเจกต์

```
backend/
  src/Example.Domain          — Entities (ApprovalDocument), Enums (ApprovalStatus)
  src/Example.Application     — Business logic (ApprovalService), DTOs, Result types
  src/Example.Infrastructure  — EF Core DbContext, Npgsql, Seed data
  src/Example.Api             — Controllers, Program.cs, CORS
  tests/Example.UnitTests     — xUnit + EF InMemory + FluentAssertions (10 tests)
frontend/                     — Angular 22 app (vitest unit tests, 8 tests)
docker-compose.yml            — PostgreSQL
```

## API

| Method | Endpoint | Body | คำอธิบาย |
|---|---|---|---|
| GET | `/api/approval-documents` | — | รายการเอกสารทั้งหมด |
| POST | `/api/approval-documents/approve` | `{ documentIds: [int], reason: string }` | อนุมัติ (batch) |
| POST | `/api/approval-documents/reject` | `{ documentIds: [int], reason: string }` | ไม่อนุมัติ (batch) |

Business rules:
- เอกสารมี 3 สถานะ: `Pending` (รออนุมัติ), `Approved` (อนุมัติ), `Rejected` (ไม่อนุมัติ)
- อนุมัติ/ไม่อนุมัติได้เฉพาะเอกสารที่ `Pending` เท่านั้น — ที่ตัดสินแล้วเลือกซ้ำไม่ได้ (409 Conflict)
- `reason` จำเป็นต้องไม่ว่าง (400 Bad Request)

## การรัน

### 1. Database (PostgreSQL ผ่าน Docker)

```bash
docker compose up -d
```

### 2. Backend

```bash
cd backend/src/Example.Api
dotnet run          # http://localhost:5204
```

EF Core จะ `EnsureCreated` และ seed ข้อมูล mockup 10 รายการ (รายการที่ 1–10) อัตโนมัติตอน startup
สถานะตาม mockup: 2,5 = อนุมัติ / 3,6 = ไม่อนุมัติ / ที่เหลือ = รออนุมัติ

### 3. Frontend

```bash
cd frontend
npm install
ng serve            # http://localhost:4200
```

## Unit tests

```bash
# Backend — 10 tests (xUnit, EF Core InMemory)
cd backend && dotnet test

# Frontend — 8 tests (vitest + HttpClientTesting)
cd frontend && ng test --no-watch
```

## การทำงานของ UI

- หน้า IT 03-1: ตารางรายการเอกสาร — เลือกได้เฉพาะรายการ `รออนุมัติ` (checkbox ของรายการที่ตัดสินแล้วจะ disabled)
- กดปุ่ม `อนุมัติ` → modal ยืนยันการอนุมัติ (IT 03-2) → กรอกเหตุผล → กด `อนุมัติ` เพื่ออัปเดตสถานะ, `ยกเลิก` เพื่อปิด
- กดปุ่ม `ไม่อนุมัติ` → modal ยืนยันการไม่อนุมัติ (IT 03-3) → เช่นเดียวกัน
