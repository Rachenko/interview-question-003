# Interview Question 003 — IT 03 Approval System

ระบบอนุมัติเอกสารตามโจทย์ IT 03-1 / 03-2 / 03-3 (example.com)

- **Backend:** C# .NET 10 (ASP.NET Core Web API + EF Core)
- **Frontend:** Angular 22 (standalone components, signals)
- **Database:** PostgreSQL 16 (Docker / docker-compose)

## โครงสร้างโปรเจกต์

```
backend/
  src/Example.Domain          — Entities (ApprovalDocument, ApprovalDecision, ApprovalDecisionItem), Enums
  src/Example.Application     — Business logic (ApprovalService), DTOs, Result types
  src/Example.Infrastructure  — EF Core DbContext, Npgsql, PostgreSQL init SQL
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

### 1. รัน Database แบบ Local (แนะนำ)

วิธีนี้รันทั้ง Database, Backend และ Frontend บนเครื่อง local โดยไม่ใช้ Docker

ติดตั้งและเปิด PostgreSQL บนเครื่อง จากนั้นตั้งค่าให้ตรงกับ `appsettings.json`:

```text
Host=localhost
Port=5432
Database=example_approval
Username=postgres
Password=postgres
```

สร้าง database และรัน schema + mock data ด้วย `psql`:

```powershell
psql -h localhost -U postgres -c "CREATE DATABASE example_approval;"
psql -h localhost -U postgres -d example_approval -f "backend\src\Example.Infrastructure\Persistence\seed.sql"
```

ถ้าไม่มีคำสั่ง `psql` สามารถเปิดไฟล์ `seed.sql` แล้วรันผ่าน pgAdmin Query Tool ได้ โดยต้องสร้าง database ชื่อ `example_approval` ก่อน

> วิธี Local ต้องรัน `seed.sql` ด้วยตนเองหนึ่งครั้ง

### 1.1 ทางเลือก: รัน Database ผ่าน Docker

วิธีนี้ใช้ Docker เฉพาะ PostgreSQL ส่วน Backend และ Frontend ยังรันบนเครื่อง local

```bash
docker compose up -d
```

Postgres จะรันที่ `localhost:5432` (user/pass: `postgres/postgres`, db: `example_approval`)

> ถ้า port 5432 ถูกใช้อยู่แล้ว แก้ `ports` ใน `docker-compose.yml` แล้วปรับ `ConnectionStrings:Default` ใน `backend/src/Example.Api/appsettings.json` ให้ตรงกัน

Docker จะรัน `backend/src/Example.Infrastructure/Persistence/seed.sql` เป็น PostgreSQL init script ตอนสร้าง database ครั้งแรก โดย SQL script จะสร้างตาราง, index และข้อมูล mockup 10 รายการโดยตรง

ถ้าเคยรันมาก่อนและต้องการให้ init script ทำงานใหม่ ให้ลบ database volume แล้วเริ่มใหม่:

```bash
docker compose down -v
docker compose up -d
```

สถานะเริ่มต้นตาม mockup: รายการ 2,5 = อนุมัติ / 3,6 = ไม่อนุมัติ / ที่เหลือ = รออนุมัติ

`seed.sql` แยกข้อมูลการตัดสินใจไว้ใน `ApprovalDecisions` และ `ApprovalDecisionItems` โดยใช้ `DecidedBy = 'admin'` เป็น dummy user; รายการที่รออนุมัติจะยังไม่มี decision

### 2. รัน Backend

```bash
cd backend/src/Example.Api
dotnet restore
dotnet run
```

API จะรันที่ `http://localhost:5204`

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

> `ng serve` proxy `/api` → `http://localhost:5204` อัตโนมัติ (`proxy.conf.json`) — ถ้า backend รันพอร์ตอื่น แก้ `target` ในไฟล์นั้น

> **Windows + PowerShell:** ถ้า `npm` error `npm.ps1 cannot be loaded because running scripts is disabled` ให้ใช้ `npm.cmd` แทน เช่น `npm.cmd install`, `npm.cmd run dev` หรือตั้งค่า `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser` แล้วเปิด PowerShell ใหม่

### 4. (ทางเลือก) รันแบบ single-port

Backend จะเสิร์ฟ Angular build จาก `wwwroot/` — build frontend แล้ว copy ไฟล์เข้า `backend/src/Example.Api/wwwroot/` จากนั้นเปิดแค่ `http://localhost:5204` ก็ใช้ได้ทั้ง UI + API

```bash
cd frontend && ng build
mkdir -p ../backend/src/Example.Api/wwwroot
cp -r dist/example-approval-ui/browser/* ../backend/src/Example.Api/wwwroot/
```

## โครงสร้างฐานข้อมูล

ระบบใช้ 3 ตารางหลัก:

- `ApprovalDocuments` — ข้อมูลเอกสารและสถานะปัจจุบัน
- `ApprovalDecisions` — เหตุผล ผู้ตัดสิน และเวลาของการตัดสินใจหนึ่ง batch
- `ApprovalDecisionItems` — ตารางเชื่อมระหว่าง decision กับเอกสารที่ถูกตัดสิน

หนึ่ง batch จะสร้าง `ApprovalDecisions` เพียง 1 แถว และสร้าง `ApprovalDecisionItems` ตามจำนวนเอกสารที่เลือก จึงไม่เก็บเหตุผลซ้ำในทุกเอกสาร

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
