# Food Order REST API Documentation

## Overview

This document provides comprehensive documentation for the Food Order REST API. The API allows you to manage products, categories, cart, orders, and user authentication.

## Base URL

```
https://localhost:5001/api
```

## Authentication

The API uses JWT (JSON Web Token) authentication. To access protected endpoints, include the JWT token in the Authorization header:

```
Authorization: Bearer <your_jwt_token>
```

### Getting a Token

1. Register a new account or login with existing credentials
2. Use the returned token for subsequent requests

## Swagger UI

Access the interactive API documentation at:
```
https://localhost:5001/swagger
```

---

## Endpoints

### Users API

#### Register a New User
```http
POST /api/users/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourPassword123",
  "confirmPassword": "YourPassword123",
  "fullName": "John Doe",
  "phoneNumber": "0123456789"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "id": "user-id",
    "email": "user@example.com",
    "fullName": "John Doe",
    "roles": ["Customer"]
  }
}
```

#### Login
```http
POST /api/users/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourPassword123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiration": "2024-01-01T12:00:00Z",
    "user": {
      "id": "user-id",
      "email": "user@example.com",
      "fullName": "John Doe"
    }
  }
}
```

#### Get User Profile
```http
GET /api/users/profile
Authorization: Bearer <token>
```

#### Update Profile
```http
PUT /api/users/profile
Authorization: Bearer <token>
Content-Type: application/json

{
  "fullName": "John Smith",
  "phoneNumber": "0987654321",
  "address": "123 Main St"
}
```

#### Change Password
```http
POST /api/users/change-password
Authorization: Bearer <token>
Content-Type: application/json

{
  "currentPassword": "OldPassword123",
  "newPassword": "NewPassword123",
  "confirmPassword": "NewPassword123"
}
```

---

### Products API

#### Get All Products
```http
GET /api/products
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "productID": 1,
      "name": "Phở Bò",
      "description": "Traditional Vietnamese beef noodle soup",
      "basePrice": 50000,
      "discountPrice": 45000,
      "imageUrl": "/images/pho-bo.jpg",
      "categoryID": 1,
      "categoryName": "Món Việt",
      "isAvailable": true
    }
  ]
}
```

#### Get Product by ID
```http
GET /api/products/{id}
```

#### Get Products by Category
```http
GET /api/products/category/{categoryId}
```

#### Create Product (Admin Only)
```http
POST /api/products
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "name": "New Product",
  "description": "Product description",
  "basePrice": 50000,
  "discountPrice": 0,
  "discountPercent": 0,
  "imageUrl": "/images/product.jpg",
  "categoryID": 1,
  "isAvailable": true
}
```

#### Update Product (Admin Only)
```http
PUT /api/products/{id}
Authorization: Bearer <admin_token>
Content-Type: application/json
```

#### Delete Product (Admin Only)
```http
DELETE /api/products/{id}
Authorization: Bearer <admin_token>
```

---

### Categories API

#### Get All Categories
```http
GET /api/categories
```

#### Get Category by ID
```http
GET /api/categories/{id}
```

#### Create Category (Admin Only)
```http
POST /api/categories
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "name": "New Category",
  "description": "Category description"
}
```

#### Update Category (Admin Only)
```http
PUT /api/categories/{id}
Authorization: Bearer <admin_token>
Content-Type: application/json
```

#### Delete Category (Admin Only)
```http
DELETE /api/categories/{id}
Authorization: Bearer <admin_token>
```

---

### Cart API

For anonymous users, include `X-Session-Id` header to maintain cart across requests.

#### Get Current Cart
```http
GET /api/cart
X-Session-Id: your-session-id (optional for anonymous users)
```

**Response:**
```json
{
  "success": true,
  "data": {
    "cartID": 1,
    "items": [
      {
        "cartItemID": 1,
        "productID": 1,
        "productName": "Phở Bò",
        "quantity": 2,
        "unitPrice": 50000,
        "totalPrice": 100000
      }
    ],
    "totalPrice": 100000,
    "totalItems": 2
  }
}
```

#### Add Item to Cart
```http
POST /api/cart/add
Content-Type: application/json

{
  "productID": 1,
  "sizeID": 1,
  "toppingIds": [1, 2],
  "quantity": 1,
  "note": "Extra spicy"
}
```

#### Update Cart Item
```http
PUT /api/cart/update
Content-Type: application/json

{
  "cartItemID": 1,
  "quantity": 3
}
```

#### Remove Item from Cart
```http
DELETE /api/cart/remove/{itemId}
```

#### Clear Cart
```http
DELETE /api/cart/clear
```

#### Get Cart Item Count
```http
GET /api/cart/count
```

---

### Orders API

#### Get All Orders (Admin/Staff Only)
```http
GET /api/orders
Authorization: Bearer <admin_or_staff_token>
```

#### Get Order by ID
```http
GET /api/orders/{id}
Authorization: Bearer <token>
```

#### Create Order (from Cart)
```http
POST /api/orders
Content-Type: application/json

{
  "customerName": "John Doe",
  "customerPhone": "0123456789",
  "customerEmail": "john@example.com",
  "shippingAddress": "123 Main St, City",
  "note": "Please call before delivery",
  "paymentMethod": "Cash",
  "shippingFee": 30000,
  "discountCode": "DISCOUNT10"
}
```

#### Update Order Status (Admin/Staff Only)
```http
PUT /api/orders/{id}/status
Authorization: Bearer <admin_or_staff_token>
Content-Type: application/json

{
  "status": "Confirmed"
}
```

**Valid Statuses:**
- `Pending` - Order placed, waiting for confirmation
- `Confirmed` - Order confirmed
- `Preparing` - Food being prepared
- `Ready` - Ready for pickup/delivery
- `Delivering` - Out for delivery
- `Completed` - Order completed
- `Cancelled` - Order cancelled

#### Get Orders by User ID
```http
GET /api/orders/user/{userId}
Authorization: Bearer <token>
```

---

### POS API (Staff Only)

#### Get Products for POS
```http
GET /api/pos/products
Authorization: Bearer <staff_token>
```

#### Create POS Order
```http
POST /api/pos/orders
Authorization: Bearer <staff_token>
Content-Type: application/json

{
  "customerName": "Walk-in Customer",
  "customerPhone": "0123456789",
  "note": "Table 5",
  "paymentMethod": "Cash",
  "discountCode": null,
  "items": [
    {
      "productID": 1,
      "sizeName": "Lớn",
      "toppingName": "Trân châu",
      "quantity": 2,
      "unitPrice": 50000,
      "note": ""
    }
  ]
}
```

#### Get Pending Orders
```http
GET /api/pos/orders/pending
Authorization: Bearer <staff_token>
```

---

## Response Format

All API responses follow this format:

### Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Error 1", "Error 2"]
}
```

---

## HTTP Status Codes

| Code | Description |
|------|-------------|
| 200  | OK - Request successful |
| 201  | Created - Resource created successfully |
| 400  | Bad Request - Invalid input data |
| 401  | Unauthorized - Authentication required |
| 403  | Forbidden - Insufficient permissions |
| 404  | Not Found - Resource not found |
| 500  | Internal Server Error |

---

## User Roles

| Role | Description |
|------|-------------|
| AdminIT | Super administrator with full access |
| FoodAdmin | Can manage products and categories |
| UserAdmin | Can manage users |
| Staff | Can use POS and manage orders |
| Customer | Regular user, can place orders |

---

## CORS

The API supports CORS for the following origins (configurable in appsettings.json):
- http://localhost:3000
- http://localhost:5173
- http://localhost:4200

---

## Testing

You can test the API using:

1. **Swagger UI**: https://localhost:5001/swagger
2. **Postman**: Import the collection and set the base URL
3. **cURL**: Use command-line requests

### Example cURL Commands

**Login:**
```bash
curl -X POST "https://localhost:5001/api/users/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"customer@gmail.com","password":"Customer@123"}'
```

**Get Products:**
```bash
curl -X GET "https://localhost:5001/api/products"
```

**Add to Cart:**
```bash
curl -X POST "https://localhost:5001/api/cart/add" \
  -H "Content-Type: application/json" \
  -H "X-Session-Id: my-session-123" \
  -d '{"productID":1,"quantity":2}'
```

---

## Development

### Running the API

```bash
dotnet run
```

### Configuration

Edit `appsettings.json` to configure:
- Database connection string
- JWT settings (Secret, Issuer, Audience, ExpirationMinutes)
- CORS allowed origins
