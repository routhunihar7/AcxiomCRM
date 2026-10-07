# AcxiomCRM

## Role-Based Customer Relationship Management System

AcxiomCRM is a web-based Customer Relationship Management (CRM) application developed using **ASP.NET Core MVC**, **Entity Framework Core**, **ASP.NET Core Identity**, and **MySQL**.

The application supports customer and sales lifecycle management with role-based access control, validation, audit logging, dashboard analytics, REST APIs, and reporting.

---

## Project Overview

AcxiomCRM is designed for three primary roles:

- **Admin** – manages users, roles, audit logs, and overall CRM administration.
- **Manager** – monitors customers, leads, opportunities, reports, and sales pipeline.
- **Sales Executive** – manages assigned CRM records and sales activities.

The project follows a layered application structure with separate controllers, models, data access, services, and views.

---

## Main Modules

### 1. Authentication & Authorization
- User registration and login
- Logout
- ASP.NET Core Identity
- Password hashing
- Password policy
- Account lockout
- Role-based authorization
- Anti-forgery protection

### 2. Dashboard
The dashboard provides CRM KPIs and visual analytics:

- Total Customers
- Total Leads
- Open Leads
- Total Opportunities
- Open Opportunities
- Won Opportunities
- Lost Opportunities
- Total Pipeline Value
- Lead Status Chart
- Opportunity Pipeline Chart
- Monthly Sales Chart

### 3. Customer Management
- Create customer
- Edit customer
- View customer details
- Delete customer
- Search customers
- Email uniqueness validation
- Phone uniqueness validation
- Role-based record access

### 4. Lead Management
- Create and manage leads
- Lead status tracking
- Expected value validation
- Search and filtering
- Sales Executive assignment and access control

### 5. Follow-Up Management
- Schedule follow-ups
- Manage follow-up status
- Customer/Lead association
- Assigned user support
- Follow-up date validation

### 6. Opportunity Management
- Create and manage opportunities
- Sales pipeline stages
- Amount and probability tracking
- Expected close date
- Open/Won/Lost status
- Business validation rules

### 7. Activity Management
Supports CRM activities such as:

- Calls
- Meetings
- Emails
- Tasks

### 8. User & Role Management
Admin users can:

- Create users
- Assign roles
- View users
- Manage Admin, Manager, and Sales Executive roles

### 9. Audit Log
Important CRM operations are recorded with:

- User ID
- Action
- Entity
- Record ID
- Old Value
- New Value
- Created Date
- IP Address

### 10. REST API
Secured REST APIs are available for:

- Customers
- Leads
- Opportunities

Swagger/OpenAPI documentation is also configured for API testing.

### 11. Reports
Admin and Manager users can access CRM reports and sales metrics including:

- Customer statistics
- Lead statistics
- Opportunity statistics
- Pipeline value
- Lead status distribution
- Opportunity status distribution

---

## Technology Stack

| Technology | Purpose |
|---|---|
| C# | Application programming language |
| ASP.NET Core MVC | Web application framework |
| .NET 8 | Runtime / SDK |
| Entity Framework Core | ORM / database access |
| ASP.NET Core Identity | Authentication and authorization |
| MySQL 8 | Relational database |
| Pomelo.EntityFrameworkCore.MySql | MySQL EF Core provider |
| Bootstrap | Responsive UI |
| Razor Views | Server-side UI |
| Chart.js | Dashboard charts |
| Swagger / OpenAPI | REST API documentation |
| Git & GitHub | Version control |

---

## Project Structure

```text
AcxiomCRM/
│
├── Controllers/
│   ├── Api/
│   ├── AdminController.cs
│   ├── AuditLogsController.cs
│   ├── CustomersController.cs
│   ├── DashboardController.cs
│   ├── FollowUpsController.cs
│   ├── LeadsController.cs
│   ├── OpportunitiesController.cs
│   ├── ReportsController.cs
│   └── ActivitiesController.cs
│
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
│
├── Migrations/
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── Customer.cs
│   ├── Lead.cs
│   ├── FollowUp.cs
│   ├── Opportunity.cs
│   ├── Activity.cs
│   └── AuditLog.cs
│
├── Services/
│   └── AuditService.cs
│
├── Views/
│   ├── Activities/
│   ├── Admin/
│   ├── AuditLogs/
│   ├── Customers/
│   ├── Dashboard/
│   ├── FollowUps/
│   ├── Leads/
│   ├── Opportunities/
│   ├── Reports/
│   └── Shared/
│
├── wwwroot/
│
├── Program.cs
├── appsettings.json
└── AcxiomCRM.csproj
```

---

## Validation & Business Rules

The application implements both client-side and server-side validation.

Important rules include:

- Customer name is required.
- Customer email must be valid and unique.
- Customer phone must be valid and unique.
- Lead name is required.
- Lead status must be valid.
- Lead expected value must be greater than zero.
- Opportunity amount must be greater than zero for active opportunities.
- Opportunity probability must be between 0 and 100.
- Expected close date cannot be in the past for an active opportunity.
- New/planned follow-up dates cannot be earlier than today.
- Unauthorized users cannot modify records outside their permitted scope.

---

## Security

Security features include:

- ASP.NET Core Identity
- Password hashing
- Password policy
- Account lockout
- Role-based authorization
- Server-side validation
- Client-side validation
- Anti-forgery protection
- Entity Framework parameterized queries
- Protected MVC endpoints
- Secured REST API endpoints
- Audit logging
- Secure configuration through application secrets

**Database credentials and administrator passwords should not be committed to source control.**

---

## Database

The application uses **MySQL** with Entity Framework Core.

Database migrations are included in the `Migrations` folder.

Main application entities include:

- ApplicationUser
- Customer
- Lead
- FollowUp
- Opportunity
- Activity
- AuditLog

ASP.NET Core Identity manages authentication-related tables.

---

## Running the Project Locally

### Prerequisites

Install:

- .NET 8 SDK
- MySQL 8
- Git
- Visual Studio / Visual Studio Code / JetBrains Rider

### 1. Clone the repository

```bash
git clone https://github.com/routhunihar7/AcxiomCRM.git
cd AcxiomCRM
```

### 2. Configure the database

Create a MySQL database named:

```text
AcxiomCRMDb
```

Configure the connection string using **User Secrets** or another secure configuration mechanism.

Do not place production passwords directly in `appsettings.json`.

### 3. Apply migrations

```bash
dotnet ef database update
```

### 4. Run the application

```bash
dotnet run
```

Open the HTTPS URL shown in the terminal.

---

## API Endpoints

### Customers

```text
GET    /api/customers
GET    /api/customers/{id}
POST   /api/customers
PUT    /api/customers/{id}
DELETE /api/customers/{id}
```

### Leads

```text
GET    /api/leads
GET    /api/leads/{id}
POST   /api/leads
PUT    /api/leads/{id}
DELETE /api/leads/{id}
```

### Opportunities

```text
GET    /api/opportunities
GET    /api/opportunities/{id}
POST   /api/opportunities
PUT    /api/opportunities/{id}
DELETE /api/opportunities/{id}
```

Swagger is configured for API documentation and testing during development.

---

## Role-Based Access

| Feature | Admin | Manager | Sales Executive |
|---|:---:|:---:|:---:|
| Dashboard | ✓ | ✓ | ✓ |
| Customers | ✓ | ✓ | Assigned |
| Leads | ✓ | ✓ | Assigned |
| Follow-Ups | ✓ | ✓ | Assigned |
| Opportunities | ✓ | ✓ | Assigned |
| Activities | ✓ | ✓ | ✓ |
| Reports | ✓ | ✓ | - |
| Audit Logs | ✓ | ✓ | - |
| User Management | ✓ | - | - |

---

## Project Architecture

The application follows a structured architecture:

```text
Presentation Layer
        ↓
Controllers / Razor Views
        ↓
Application / Business Logic
        ↓
Services
        ↓
Entity Framework Core
        ↓
MySQL Database
```

Cross-cutting concerns include:

- Authentication
- Authorization
- Validation
- Security
- Auditing
- Logging

---

## Testing & Acceptance

The application was developed to satisfy the CRM project requirements including:

- Authentication and authorization
- Role-based access
- Client-side validation
- Server-side validation
- Business rule validation
- Customer/Lead/Opportunity management
- Follow-up management
- Dashboard KPIs and charts
- Audit logging
- REST API authorization
- Reporting
- Structured MVC architecture

---

## Repository

GitHub Repository:

https://github.com/routhunihar7/AcxiomCRM

---

## Author

**Nihar Routhu**

B.Tech – Computer Science Engineering  
GITAM University
