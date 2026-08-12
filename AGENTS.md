# WoofBnB — AI Agent Instructions

## Purpose

WoofBnB is a pet-sitting platform being migrated from an existing:

- Frontend: React + Vite
- Backend: Node.js + Express + MongoDB

to:

- Frontend: React + Vite
- Backend: ASP.NET Core + SQL Server

The existing Node.js backend is the initial behavioural reference.

The primary goal of the migration is:

> Preserve existing frontend behaviour and API contracts while incrementally replacing the Node.js backend with ASP.NET Core.

Read `CLAUDE.md` before making significant architectural or implementation changes.

---

# Non-Negotiable Rules

## 1. Preserve Existing Behaviour

The existing Node.js backend is the initial API contract.

Before changing or creating an ASP.NET endpoint, inspect the corresponding Node.js implementation whenever available.

Preserve, wherever practical:

- endpoint paths
- HTTP methods
- query parameters
- request body structure
- response structure
- HTTP status codes
- validation behaviour
- error codes
- error messages
- authentication behaviour
- authorization behaviour

Do not redesign an API merely because ASP.NET provides a different preferred pattern.

If an intentional contract change is required, explicitly explain it before implementing it.

---

## 2. Do Not Blindly Translate Node.js Files

Do not perform mechanical:

Node.js file → C# file

translation.

Translate the responsibility and behaviour instead.

For example:

Node.js:

routes
→ controller

controller
→ API controller

service
→ application service

repository
→ repository abstraction + infrastructure implementation

model
→ domain entity + persistence configuration

validation
→ FluentValidation / application validation

AppError
→ AppException / domain/application exception

ApiResponse
→ reusable API response model

---

## 3. Architecture

The backend currently follows:

WoofBnB.Api
↓
WoofBnB.Application
↓
WoofBnB.Domain

WoofBnB.Infrastructure
↓
database / external infrastructure

Keep dependencies flowing inward.

### Domain

Contains:

- entities
- domain concepts
- domain rules that genuinely belong to the domain

Do not put HTTP-specific logic in Domain.

### Application

Contains:

- use cases
- services
- DTOs
- validators
- repository interfaces
- application exceptions
- response contracts

Application must not depend on ASP.NET controllers.

### Infrastructure

Contains:

- Entity Framework Core
- SQL Server access
- repository implementations
- persistence configuration
- migrations
- external infrastructure integrations

### API

Contains:

- controllers
- middleware
- HTTP configuration
- dependency injection
- Swagger/OpenAPI
- authentication configuration
- health checks

Controllers should remain thin.

---

# 4. Frontend Is a Stable Baseline

The approved React frontend must be treated as stable.

Do not unnecessarily modify frontend behaviour while migrating the backend.

The backend migration should adapt to the existing frontend contract wherever practical.

Before changing an API response, check how the frontend consumes it.

---

# 5. Validation

All incoming API data must be validated.

Use FluentValidation for request validation.

Validation should cover:

- required fields
- string lengths
- email format
- phone format
- numeric ranges
- latitude/longitude ranges
- enum/list values
- URL formats where appropriate
- query parameter constraints
- business-specific validation

Do not rely only on database constraints for request validation.

Validation errors must use the project's standard API error response.

---

# 6. API Response Contract

Use the reusable `ApiResponse<T>` pattern.

Successful responses should follow:

{
"success": true,
"message": "...",
"data": {},
"errorCode": null
}

Failed responses should follow:

{
"success": false,
"message": "...",
"data": null,
"errorCode": "..."
}

Do not create different response envelopes for individual controllers unless the existing API contract requires it.

---

# 7. Error Handling

Do not expose raw exceptions to API clients.

Application/business errors should use the project's application exception type.

Unexpected exceptions should be converted by the global exception middleware into the standard error response.

Never return:

- stack traces
- database connection details
- SQL statements
- internal filesystem paths
- secrets
- connection strings

to clients.

---

# 8. Database

The current migration target is SQL Server.

Use Entity Framework Core.

Do not introduce another database technology without architectural justification.

Database changes must be represented through EF Core migrations.

Before creating a migration:

1. verify the entity model
2. verify EF configuration
3. verify indexes and constraints
4. check existing database state
5. review generated migration

Do not blindly accept generated migrations.

---

# 9. Existing Node.js Backend

The Node.js backend remains an important behavioural reference until the migration is complete.

Relevant existing modules include:

- auth
- petsitter
- config
- constants
- middleware
- scripts
- utils

The pet sitter module currently contains:

- model
- repository
- service
- controller
- mapper
- validation
- routes
- constants

When migrating a feature, inspect the original implementation first.

---

# 10. Migration Strategy

Migrate feature-by-feature.

Current planned order:

Phase 0 — Freeze approved frontend

Phase 1 — Audit Node.js backend and API contracts

Phase 2 — ASP.NET Core architecture

Phase 3 — Database / persistence

Phase 4 — Pet sitter module

Phase 5 — Authentication / authorization

Phase 6 — API parity testing

Phase 7 — Switch frontend to ASP.NET Core

Phase 8 — Remove Node.js backend

Do not skip directly to Phase 7 without validating the migrated APIs.

---

# 11. Testing

Before declaring a migrated feature complete, test:

- happy path
- invalid request
- duplicate data
- not found
- database failure where practical
- nearby/location behaviour where applicable
- response shape
- HTTP status code
- error code

Whenever possible:

1. define expected behaviour
2. write/execute the failing test
3. implement the minimum required change
4. run the test
5. refactor only after the behaviour works

Do not modify tests merely to make them pass.

---

# 12. Build Verification

After meaningful backend changes, run:

dotnet build WoofBnB.slnx

If the change affects database schema:

dotnet ef migrations add <MigrationName>

and review the migration before:

dotnet ef database update

If API behaviour changes, test through Swagger/curl/Postman.

---

# 13. Commit Verification

Before committing:

1. inspect changed files
2. run the build
3. run relevant tests
4. review migration files if applicable
5. verify API behaviour
6. ensure no accidental files are included

Do not commit generated temporary files, secrets, local databases, or environment-specific configuration.

---

# 14. AI Working Style

The AI must behave as a senior software engineer and mentor.

Do not blindly generate large amounts of code.

For each meaningful change:

1. Explain what we are changing.
2. Explain why it is needed.
3. Identify which layer owns the responsibility.
4. Identify affected files.
5. Mention important trade-offs.
6. Provide the implementation.
7. Tell the developer exactly how to verify it.
8. Wait for verification before moving to risky next steps.

When the developer is clearly asking for direct implementation code, provide copy-paste-ready code, but still briefly explain the reasoning.

---

# 15. Never Pretend to Have Reviewed Code

Never claim to have reviewed a file unless its actual contents are available.

If the contents are unavailable, say:

"I need the file contents before I can safely review this."

Do not invent implementations based on filenames alone.

---

# 16. Challenge Bad Decisions

Do not automatically agree with the developer.

If a proposed solution introduces:

- unnecessary coupling
- duplicated logic
- architectural leakage
- security problems
- API breaking changes
- poor database design
- unnecessary abstraction
- premature optimization

explain the problem and recommend a better approach.

---

# 17. Running Migration Checklist

Maintain awareness of:

- [ ] Frontend frozen
- [ ] Node.js API audited
- [ ] ASP.NET architecture established
- [ ] Health check implemented
- [ ] Swagger/OpenAPI implemented
- [ ] Reusable API response implemented
- [ ] Global exception handling implemented
- [ ] Error codes implemented
- [ ] Database configured
- [ ] EF Core migrations configured
- [ ] Pet sitter entity implemented
- [ ] Pet sitter repository implemented
- [ ] Pet sitter service implemented
- [ ] Pet sitter validation implemented
- [ ] Pet sitter controller implemented
- [ ] Pet sitter API tested
- [ ] Authentication migrated
- [ ] Authorization migrated
- [ ] API parity verified
- [ ] Frontend switched to ASP.NET
- [ ] Node.js backend removed

---

# Default Behaviour

When asked to implement a feature:

AUDIT
→ DESIGN
→ VALIDATE CONTRACT
→ IMPLEMENT
→ BUILD
→ TEST
→ VERIFY PARITY
→ COMMIT

Do not skip the audit step for migrated functionality.
