# WoofBnB Backend Architecture

## Overview

WoofBnB follows a **Layered Architecture** combined with the **Repository Pattern** to separate HTTP handling, business logic, and database access.

The objective is to keep the codebase maintainable, testable, and scalable as new modules are introduced.

---

# High-Level Architecture

```mermaid
flowchart TD

A[Client]

A --> B[Express Routes]

B --> C[Validation Middleware]

C --> D[Authentication Middleware]

D --> E[Controller]

E --> F[Service]

F --> G[Repository]

G --> H[Mongoose Model]

H --> I[(MongoDB)]
```

---

# Request Lifecycle

Every request follows the same execution flow.

```mermaid
sequenceDiagram

participant Client

participant Route

participant Middleware

participant Controller

participant Service

participant Repository

participant MongoDB

Client->>Route: HTTP Request

Route->>Middleware: Validate Request

Middleware->>Controller: Validated Request

Controller->>Service: Execute Business Logic

Service->>Repository: Database Operation

Repository->>MongoDB: Query

MongoDB-->>Repository: Result

Repository-->>Service: Entity

Service-->>Controller: DTO

Controller-->>Client: ApiResponse
```

---

# Layer Responsibilities

## Routes

Responsibilities

- Define endpoints
- Register middlewares
- Forward request to controllers

Routes contain **no business logic**.

---

## Middlewares

Responsibilities

- Authentication
- Authorization
- Validation
- Global Error Handling

Middlewares execute before controllers.

---

## Controllers

Responsibilities

- Receive HTTP request
- Call service layer
- Return standardized API response

Controllers should remain thin.

They should never contain business logic or database queries.

---

## Services

Responsibilities

- Business rules
- Orchestrate repositories
- Convert entities into DTOs
- Throw application errors

Services never communicate directly with Express.

---

## Repositories

Responsibilities

- Communicate with MongoDB
- Perform CRUD operations
- Encapsulate Mongoose queries

Repositories know nothing about HTTP.

---

## Models

Responsibilities

- Database schema
- Indexes
- Constraints

Only persistence-related logic belongs here.

---

# Folder Structure

```
src
│
├── config
├── constants
├── middlewares
├── modules
│
│   ├── auth
│   │
│   ├── petsitter
│
├── scripts
├── utils
│
├── app.js
└── server.js
```

---

# Module Structure

Each feature follows the same layout.

```
module
│
├── controller
├── service
├── repository
├── model
├── validation
├── mapper
└── routes
```

This makes the application easy to scale.

---

# Authentication Flow

```mermaid
flowchart TD

A[Login]

A --> B[Validate Credentials]

B --> C[Generate JWT]

C --> D[Return Access Token]

D --> E[Protected Routes]

E --> F[Verify JWT]

F --> G[Authenticated Request]
```

---

# Pet Sitter Registration Flow

```mermaid
flowchart TD

A[POST /petsitters]

A --> B[Validate Request]

B --> C[Controller]

C --> D[Service]

D --> E[Check Email Exists]

E --> F[Repository]

F --> G[(MongoDB)]

G --> H[Create Pet Sitter]

H --> I[DTO Mapper]

I --> J[ApiResponse]
```

---

# Nearby Search Flow

```mermaid
flowchart TD

A[GET /petsitters/nearby]

A --> B[Validate Query]

B --> C[Controller]

C --> D[Service]

D --> E[Repository]

E --> F[$near Query]

F --> G[2dsphere Index]

G --> H[(MongoDB)]

H --> I[Mapped DTO]

I --> J[Client]
```

---

# GeoSpatial Search

Pet sitters are stored using MongoDB GeoJSON.

Example

```json
{
  "type": "Point",
  "coordinates": [77.209, 28.6139]
}
```

Coordinates follow the GeoJSON standard.

```
[longitude, latitude]
```

A **2dsphere index** is created on the location field to enable efficient nearby searches.

Queries use MongoDB's `$near` operator.

---

# Error Handling

```mermaid
flowchart TD

A[Controller]

A --> B[asyncHandler]

B --> C[Throw AppError]

C --> D[Global Error Middleware]

D --> E[ApiError]

E --> F[Client]
```

---

# API Response Format

Every successful response follows a consistent structure.

```json
{
  "success": true,
  "statusCode": 200,
  "message": "...",
  "data": {},
  "timestamp": "..."
}
```

Error responses are also standardized.

---

# Design Principles

- Layered Architecture
- Repository Pattern
- Separation of Concerns
- Single Responsibility Principle
- Centralized Error Handling
- DTO Mapping
- Validation Before Business Logic
- Reusable Utilities
- Modular Feature-Based Structure

---

# Future Enhancements

- Image Uploads
- Pagination
- Search
- Filters
- Booking Module
- Reviews
- Payments
- Redis Caching
