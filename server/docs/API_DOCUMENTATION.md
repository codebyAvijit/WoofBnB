# WoofBnB API Documentation

Base URL

```
http://localhost:5000/api
```

---

# Authentication

---

## Login

### POST

```
/auth/login
```

### Request

```json
{
  "email": "admin@example.com",
  "password": "password123"
}
```

### Response

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Login successful",
  "data": {
    "accessToken": "...",
    "user": {}
  },
  "timestamp": "..."
}
```

---

## Get Current User

### GET

```
/auth/me
```

### Headers

```
Authorization: Bearer <token>
```

---

# Pet Sitters

---

## Register Pet Sitter

### POST

```
/petsitters
```

### Request

```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "9876543210",
  "bio": "Professional pet sitter",
  "address": "Connaught Place",
  "location": {
    "type": "Point",
    "coordinates": [77.209, 28.6139]
  },
  "workingHours": {
    "start": "09:00",
    "end": "18:00"
  },
  "amenities": ["Dog Walking", "Medication"]
}
```

---

### Success Response

```json
{
  "success": true,
  "statusCode": 201,
  "message": "Pet sitter registered successfully",
  "data": {}
}
```

---

## Get All Pet Sitters

### GET

```
/petsitters
```

### Success Response

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Pet sitters fetched successfully",
  "data": []
}
```

---

## Nearby Pet Sitters

### GET

```
/petsitters/nearby
```

### Query Parameters

| Parameter | Type   | Required |
| --------- | ------ | -------- |
| lat       | Number | Yes      |
| lng       | Number | Yes      |
| radius    | Number | No       |

Default radius:

```
5000 meters
```

---

### Example

```
GET /petsitters/nearby?lat=28.6139&lng=77.209&radius=5000
```

---

### Success Response

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Nearby pet sitters fetched successfully",
  "data": [
    {
      "id": "...",
      "name": "John Doe",
      "email": "john@example.com",
      "phone": "9876543210",
      "bio": "Professional pet sitter",
      "address": "Connaught Place",
      "location": {
        "type": "Point",
        "coordinates": [77.209, 28.6139]
      },
      "workingHours": {
        "start": "09:00",
        "end": "18:00"
      },
      "amenities": ["Dog Walking", "Medication", "Indoor Stay"],
      "profileImage": null,
      "createdAt": "...",
      "updatedAt": "..."
    }
  ]
}
```

---

# Error Response

```json
{
  "success": false,
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Invalid email"
    }
  ],
  "timestamp": "..."
}
```

---

# Authentication Flow

```
Login
   │
   ▼
Receive JWT
   │
   ▼
Authorization Header
   │
   ▼
Protected APIs
```

---

# Geospatial Search

MongoDB uses a **2dsphere index** on the `location` field.

Nearby searches are performed using the `$near` operator with GeoJSON coordinates in the format:

```
[longitude, latitude]
```

The `radius` query parameter is specified in **meters**.

---

# Upcoming APIs

- GET /petsitters/:id
- Image Upload
- Pagination
- Search
- Filters
