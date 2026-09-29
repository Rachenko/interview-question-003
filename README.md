# Interview Question 003 — IT 03 Approval System

ระบบอนุมัติเอกสารตามโจทย์ IT 03-1 / 03-2 / 03-3 (example.com)

- **Backend:** C# .NET 10 (ASP.NET Core Web API + EF Core)
- **Frontend:** Angular 22 (standalone components, signals)
- **Database:** PostgreSQL 16 (Docker / docker-compose)

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

## Prerequisites (สิ่งที่ต้องติดตั้งก่อน)

| เครื่องมือ | เวอร์ชันขั้นต่ำ |
|---|---|
| .NET SDK | 10.x |
| Node.js | 20+ (แนะนำ 24) |
| Docker + Docker Compose | ใดก็ได้ |
| Angular CLI | `npm i -g @angular/cli` (22.x) |

## ขั้นตอนการรันโปรเจกต์

### 0. Clone

```bash
git clone <repo-url> interview-question-003
cd interview-question-003
```

### 1. รัน Database (PostgreSQL ผ่าน Docker)

```bash
docker compose up -d
```

Postgres จะรันที่ `localhost:5432` (user/pass: `postgres/postgres`, db: `example_approval`)

> ถ้า port 5432 ถูกใช้อยู่แล้ว แก้ `ports` ใน `docker-compose.yml` แล้วปรับ `ConnectionStrings:Default` ใน `backend/src/Example.Api/appsettings.json` ให้ตรงกัน

### 2. รัน Backend

```bash
cd backend/src/Example.Api
dotnet restore
dotnet run
```

API จะรันที่ `http://localhost:5204`

- ตอน startup ระบบจะ `EnsureCreated` สร้างตาราง + seed ข้อมูล mockup 10 รายการ (รายการที่ 1–10) อัตโนมัติ
- สถานะเริ่มต้นตาม mockup: รายการ 2,5 = อนุมัติ / 3,6 = ไม่อนุมัติ / ที่เหลือ = รออนุมัติ
- OpenAPI spec อยู่ที่ `http://localhost:5204/openapi/v1.json`

ทดสอบ API เร็วๆ:

```bash
curl http://localhost:5204/api/approval-documents
```

### 3. รัน Frontend

เปิด terminal ใหม่:

```bash
cd frontend
npm install
ng serve
```

เปิดเบราว์เซอร์ที่ `http://localhost:4200`

> Frontend ยิง API ที่ `http://localhost:5204` — ถ้า backend รันพอร์ตอื่น แก้ `baseUrl` ใน `frontend/src/app/approval.service.ts`

## รัน Unit Tests

```bash
# Backend — 10 tests (xUnit + EF Core InMemory + FluentAssertions)
cd backend
dotnet test

# Frontend — 8 tests (vitest + HttpClientTesting)
cd frontend
ng test --no-watch
```

## API Reference

| Method | Endpoint | Body | คำอธิบาย |
|---|---|---|---|
| GET | `/api/approval-documents` | — | รายการเอกสารทั้งหมด |
| POST | `/api/approval-documents/approve` | `{ "documentIds": [1,4], "reason": "..." }` | อนุมัติ (batch) |
| POST | `/api/approval-documents/reject` | `{ "documentIds": [7], "reason": "..." }` | ไม่อนุมัติ (batch) |

**Business rules**

- เอกสารมี 3 สถานะ: `Pending` (รออนุมัติ), `Approved` (อนุมัติ), `Rejected` (ไม่อนุมัติ)
- ตัดสินได้เฉพาะเอกสารที่ `Pending` — ที่ตัดสินแล้วเลือกซ้ำไม่ได้ → `409 Conflict`
- `reason` จำเป็นต้องไม่ว่าง → `400 Bad Request`
- id ที่ไม่มีในระบบ → `404 Not Found`

## การทำงานของ UI

- **IT 03-1** — ตารางรายการเอกสาร: เลือก checkbox ได้เฉพาะรายการ `รออนุมัติ` (รายการที่ตัดสินแล้ว checkbox จะ disabled)
- **IT 03-2** — กด `อนุมัติ` → modal ยืนยันการอนุมัติ → กรอกเหตุผล → กด `อนุมัติ` เพื่ออัปเดตสถานะ / `ยกเลิก` เพื่อปิด modal
- **IT 03-3** — กด `ไม่อนุมัติ` → modal ยืนยันการไม่อนุมัติ → เช่นเดียวกัน
