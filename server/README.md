# WoofBnB Backend

A scalable Node.js + Express backend for **WoofBnB**, a pet sitter locator platform.

The project follows a **Layered Architecture** using the **Repository Pattern** to keep business logic, data access, and HTTP concerns separated.

---

# Tech Stack

- Node.js
- Express.js
- MongoDB
- Mongoose
- JWT Authentication
- Zod Validation
- bcrypt
- Multer
- Helmet
- CORS

---

# Architecture

```
Client
   │
   ▼
Routes
   │
   ▼
Validation Middleware
   │
   ▼
Controller
   │
   ▼
Service
   │
   ▼
Repository
   │
   ▼
Mongoose Model
   │
   ▼
MongoDB
```

---

# Project Structure

```
src
│
├── config/
├── constants/
├── middlewares/
├── modules/
│   ├── auth/
│   └── petsitter/
├── scripts/
├── utils/
├── app.js
└── server.js
```

---

# Features

## Authentication

- Admin Login
- JWT Authentication
- Protected Routes
- Current Logged-in User

## Pet Sitters

- Register Pet Sitter
- Get All Pet Sitters
- Nearby Search (GeoJSON)
- Image Upload (Planned)

---

# Getting Started

## Install dependencies

```bash
npm install
```

## Environment Variables

Create a `.env` file.

```env
PORT=5000

MONGO_URI=your_mongodb_connection

JWT_SECRET=your_secret

JWT_EXPIRES_IN=7d
```

---

## Seed Admin

```bash
npm run seed:admin
```

---

## Run Development Server

```bash
npm run dev
```

---

# Design Principles

- Layered Architecture
- Repository Pattern
- Separation of Concerns
- Centralized Error Handling
- Standardized API Responses
- Input Validation using Zod
- DTO Mapping

---

# API Modules

## Auth

- POST /api/auth/login
- GET /api/auth/me

## Pet Sitters

- POST /api/petsitters
- GET /api/petsitters
- GET /api/petsitters/nearby

---

# Future Enhancements

- Image Uploads
- Pagination
- Search
- Filters
- Booking Module
- Reviews
- Payments

---

# Author

Avijit Pateriya
