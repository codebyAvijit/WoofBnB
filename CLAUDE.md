# WoofBnB — Claude Code Project Instructions

## 1. Project Overview

WoofBnB is a full-stack pet-sitting platform.

Current frontend:

- React
- Vite
- React Router
- Axios
- React Query
- React Context
- Google Maps
- @react-google-maps/api

Existing backend:

- Node.js
- Express
- MongoDB
- JWT authentication
- password hashing
- validation
- modular architecture

Migration target:

- ASP.NET Core
- C#
- Entity Framework Core
- SQL Server
- Swagger/OpenAPI
- FluentValidation
- global exception handling
- health checks

The approved React frontend is the stable consumer of the backend API.

The Node.js backend is the behavioural reference during migration.

---

# 2. Primary Objective

The goal is not to redesign WoofBnB.

The goal is to migrate the backend technology while preserving observable behaviour.

The migration should therefore follow:

Existing Node.js behaviour
↓
API contract
↓
ASP.NET Core implementation
↓
API parity validation
↓
Frontend switch

The frontend should require minimal or no behavioural changes.

---

# 3. Authority and Contract Rules

The existing Node.js backend defines the initial behaviour of the migrated API.

When implementing an existing feature, inspect:

routes
controller
service
repository
model
mapper
validation
constants
middleware

before changing the ASP.NET equivalent.

If two implementations disagree, determine which behaviour the current frontend actually depends on.

Do not assume that the ASP.NET implementation is automatically correct because it follows a framework convention.

---

# 4. Backend Architecture

The solution uses a layered architecture.

## WoofBnB.Domain

Responsible for core business concepts.

Typical contents:

src/WoofBnB.Domain/
Entities/
ValueObjects/
Constants/
Exceptions/

Domain should remain independent of:

- ASP.NET Core
- Entity Framework implementation details
- HTTP
- controllers
- middleware

---

## WoofBnB.Application

Responsible for application use cases.

Typical structure:

src/WoofBnB.Application/
Common/
Exceptions/
Responses/
ErrorCodes/
PetSitters/
DTOs/
Interfaces/
Mappers/
Validators/
IPetSitterService.cs
IPetSitterRepository.cs
PetSitterService.cs

Application contains:

- DTOs
- validators
- services
- repository abstractions
- application exceptions
- response contracts

Application should not know about HTTP controllers.

---

## WoofBnB.Infrastructure

Responsible for technical implementations.

Typical structure:

src/WoofBnB.Infrastructure/
Persistence/
Repositories/
Configurations/
Migrations/

Infrastructure contains:

- DbContext
- EF Core configurations
- repository implementations
- database migrations
- external services

---

## WoofBnB.Api

Responsible for HTTP concerns.

Typical structure:

src/WoofBnB.Api/
Controllers/
Middleware/
Program.cs

API contains:

- controllers
- middleware
- Swagger
- health checks
- dependency injection
- authentication configuration
- HTTP-specific concerns

Controllers should remain thin.

---

# 5. Dependency Direction

Preferred dependency direction:

WoofBnB.Api
↓
WoofBnB.Application
↓
WoofBnB.Domain

WoofBnB.Infrastructure
↓
WoofBnB.Application
↓
WoofBnB.Domain

The Domain must not depend on Infrastructure.

The Application must not depend on controllers.

Controllers should not contain business logic.

Repositories should not contain business rules that belong in Application or Domain.

---

# 6. API Design

Existing endpoint contracts should be preserved.

For example:

GET /api/petsitters

POST /api/petsitters

GET /api/petsitters/{id}

GET /api/petsitters/nearby?lat=...&lng=...&radius=...

Do not change routes merely to make them look more RESTful.

The existing frontend is the consumer and therefore compatibility matters.

---

# 7. Standard Response

Use:

ApiResponse<T>

for API responses.

Success:

{
"success": true,
"message": "Pet sitters fetched successfully",
"data": [],
"errorCode": null
}

Failure:

{
"success": false,
"message": "Pet sitter not found",
"data": null,
"errorCode": "NOT_FOUND"
}

Response envelope should remain consistent across modules.

---

# 8. Error Codes

Use centralized error codes.

Examples:

VALIDATION_ERROR
NOT_FOUND
CONFLICT
UNAUTHORIZED
FORBIDDEN
INTERNAL_SERVER_ERROR

Do not scatter arbitrary error-code strings throughout controllers.

If a new error code is required:

1. define it centrally
2. document why it exists
3. use it consistently
4. preserve compatibility with the Node.js API where applicable

---

# 9. Exceptions

Application-specific failures should use:

AppException

Examples:

AppException.Conflict(...)
AppException.NotFound(...)
AppException.Validation(...)

Do not use:

throw new InvalidOperationException(...)

for expected business/application errors when those errors need a controlled API response.

Expected application errors should be handled by:

ExceptionHandlingMiddleware

Unexpected exceptions should return a generic 500 response.

Never expose internal exception details in production responses.

---

# 10. Validation

Use FluentValidation.

Validation belongs in Application.

Example location:

WoofBnB.Application/
PetSitters/
Validators/
CreatePetSitterRequestValidator.cs
NearbyPetSitterRequestValidator.cs

Validation should reproduce the Node.js validation rules.

For the pet sitter create request, validate at minimum:

Name:

- required
- trim-compatible
- minimum length 2
- maximum length 50

Email:

- required
- valid email

Phone:

- required
- valid phone format

Bio:

- required
- minimum 20
- maximum 1000

Address:

- required
- minimum 5

Latitude:

- minimum -90
- maximum 90

Longitude:

- minimum -180
- maximum 180

Working hours:

- required

Amenities:

- valid allowed values

ProfileImage:

- optional
- valid URL when supplied, if that matches the existing Node.js contract

Nearby query:

lat:

- -90 to 90

lng:

- -180 to 180

radius:

- greater than 0
- default 5000 when omitted, if this matches the original API

---

# 11. Pet Sitter Amenities

The existing Node.js constants are:

Dog Walking
Medication
24x7 Care
Training
Vet Nearby
Indoor Stay
Outdoor Play
CCTV
Pickup Drop
Large Yard
Small Pets
Cats
Dogs
Birds

These values are part of the API contract.

Do not rename them casually.

---

# 12. Database Strategy

The current migration target uses SQL Server.

Entity Framework Core is the persistence mechanism.

Use migrations for schema changes.

Workflow:

1. modify entity/configuration
2. build
3. create migration
4. inspect migration
5. apply migration
6. verify database
7. test API

Never blindly trust generated migration SQL.

Pay particular attention to:

- destructive changes
- nullable → non-nullable changes
- default values
- indexes
- unique constraints
- data loss warnings

---

# 13. Entity vs DTO

Do not expose EF entities directly from controllers.

Use DTOs.

Flow:

HTTP request
↓
Request DTO
↓
Validator
↓
Application service
↓
Domain entity
↓
Repository
↓
Domain entity
↓
Mapper
↓
Response DTO
↓
ApiResponse<T>

---

# 14. Repository Rules

Repositories are responsible for persistence operations.

They should not decide application business behaviour.

For example:

Correct:

repository.GetByEmailAsync(email)

Application decides:

if existing → throw conflict

Incorrect:

repository decides that an email is invalid business logic.

---

# 15. Service Rules

Application services coordinate use cases.

Example:

RegisterPetSitter:

1. normalize email
2. check duplicate
3. construct entity
4. persist entity
5. map entity to DTO
6. return DTO

The controller should not perform these operations.

---

# 16. Controller Rules

Controllers should:

- receive HTTP input
- invoke application services
- return HTTP responses

Controllers should not:

- directly access DbContext
- contain business rules
- perform duplicate checks
- manually implement validation logic
- construct complex domain entities

---

# 17. Swagger

Swagger/OpenAPI is a development and testing tool.

It should remain enabled for development.

Swagger is useful for:

- endpoint discovery
- request testing
- response verification
- API contract inspection

Swagger authorization will be configured when authentication is migrated.

---

# 18. Health Check

The API must expose:

GET /health

Expected healthy response:

Healthy

Health checks should remain lightweight.

Additional dependency checks can be added later if required.

---

# 19. Authentication

Authentication is migrated after the pet sitter foundation.

Existing Node.js behaviour includes:

- JWT
- password hashing
- authentication middleware

The ASP.NET implementation should preserve:

- token semantics where practical
- protected endpoints
- unauthorized behaviour
- authorization behaviour
- role/permission behaviour

Do not introduce a completely different authentication contract without justification.

---

# 20. Migration Workflow

Every feature migration follows:

## Step 1 — Audit

Inspect the Node.js implementation.

Determine:

- routes
- request shape
- response shape
- validation
- errors
- database behaviour
- edge cases
- frontend dependencies

## Step 2 — Contract

Write down the observable API behaviour.

## Step 3 — Design

Map Node.js responsibilities to ASP.NET layers.

## Step 4 — Implement

Build the minimum ASP.NET implementation.

## Step 5 — Validate

Run build/tests.

## Step 6 — Compare

Compare ASP.NET responses against Node.js behaviour.

## Step 7 — Integrate

Only after parity is acceptable should the frontend switch to the ASP.NET API.

---

# 21. Testing Strategy

Minimum testing should cover:

### API success

- create
- get all
- get by id
- nearby search

### Validation

- invalid name
- invalid email
- invalid phone
- invalid bio
- invalid address
- invalid coordinates
- invalid amenities
- invalid nearby parameters

### Business errors

- duplicate email
- missing pet sitter

### Response contract

Verify:

success
message
data
errorCode

### HTTP status

Verify:

200
201
400
404
409
500

as appropriate.

---

# 22. Build Commands

Solution build:

dotnet build WoofBnB.slnx

Run API:

dotnet run --project src/WoofBnB.Api

EF migration:

dotnet ef migrations add <MigrationName> \
 --project src/WoofBnB.Infrastructure \
 --startup-project src/WoofBnB.Api

Database update:

dotnet ef database update \
 --project src/WoofBnB.Infrastructure \
 --startup-project src/WoofBnB.Api

---

# 23. AI Development Protocol

When asked:

"Implement X"

the AI should internally follow:

AUDIT
→ CONTRACT
→ ARCHITECTURE
→ IMPLEMENT
→ BUILD
→ TEST
→ VERIFY

The AI should explain these steps briefly when useful.

---

# 24. Before Editing Existing Code

Before modifying an existing class/method/file:

1. inspect the current implementation
2. identify callers/dependencies where available
3. understand its responsibility
4. identify whether API behaviour can change
5. make the smallest appropriate change

Do not overwrite existing code without understanding it.

---

# 25. Before Adding a New Abstraction

Ask:

- Does this responsibility already exist?
- Is the abstraction actually reusable?
- Is it needed by more than one feature?
- Does it improve architecture or merely add files?

Avoid unnecessary abstraction.

---

# 26. Avoid Premature Optimization

Do not optimize before measuring.

Correctness and API parity come first.

For example, nearby pet sitter search may initially use a basic implementation while the correct SQL Server spatial approach is designed and verified.

Do not silently claim a temporary implementation is production-ready.

---

# 27. Security

Never commit:

- passwords
- JWT secrets
- connection strings containing credentials
- API keys
- Google Maps keys with inappropriate exposure
- production secrets

Use configuration/environment variables.

Review authentication and authorization changes carefully.

---

# 28. Git / Commit Rules

Before commit:

dotnet build WoofBnB.slnx

Then inspect:

git status
git diff

Confirm:

- only intended files changed
- no secrets
- no temporary files
- migrations are correct
- API behaviour is verified

Commit messages should describe the actual change.

Example:

feat(petsitter): add request validation

or:

feat(api): add standardized exception handling

---

# 29. Current Migration Status

Phase 0 — Frontend freeze:
COMPLETED

Phase 1 — Backend/API audit:
IN PROGRESS / PARTIALLY COMPLETED

Phase 2 — ASP.NET architecture:
COMPLETED

Phase 3 — Persistence:
IN PROGRESS

Phase 4 — Pet sitter:
IN PROGRESS

Phase 5 — Authentication:
NOT STARTED

Phase 6 — API parity testing:
NOT STARTED

Phase 7 — Frontend switch:
NOT STARTED

Phase 8 — Node.js removal:
NOT STARTED

Known completed infrastructure:

- ASP.NET Core API
- Swagger/OpenAPI
- /health
- EF Core
- SQL Server
- PetSitter entity
- PetSitter repository
- PetSitter service
- PetSitter controller
- ApiResponse
- AppException
- global exception middleware
- FluentValidation

---

# 30. Important Current Technical Debt

The following should be tracked rather than hidden:

- EF Core collection/value-converter warning for Amenities
- SQL Server nearby/spatial query still requires proper implementation
- EF CLI/runtime version should remain aligned
- HealthChecks package warning should be cleaned up
- API parity tests still need to be added
- authentication migration is pending

Do not mark these as completed merely because the application builds.

---

# 31. Final Rule

WoofBnB is being migrated, not rewritten.

Prefer:

small change

- explicit reasoning
- preserved contract
- verification

over:

large rewrite

- framework-driven redesign
- unverified assumptions

The AI should behave like a senior engineer reviewing and implementing production software, not like a code generator.
