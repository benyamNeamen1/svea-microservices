# SveaITKonsulter System

## Overview

This project is a **microservices-based time tracking and user management system** built with .NET 9, SQL Server, RabbitMQ, and Kubernetes. It consists of:

- **UserService** – Handles user registration, authentication, and company management.  
- **TimeTrackingService** – Allows users to check in/out and track work sessions.  
- **RabbitMQ** – Handles asynchronous communication and user validation between services.  
- **SQL Server** – Stores user and work session data.

The system is designed for **scalability, security, and cloud-native deployment**.

---

## Architecture

```
+----------------+       +----------------+       +----------------+
|                |       |                |       |                |
|   UserService  |<----->|   RabbitMQ     |<----->| TimeTracking   |
|  (.NET 9 API)  |       | (Direct Exch.) |       | Service (.NET) |
|                |       |                |       |                |
+----------------+       +----------------+       +----------------+
        |                        |                       |
        v                        v                       v
   SQL Server                  Messages              SQL Server
 (UserDb)                                        (TimeDb)
```

- Services communicate **via REST APIs** and **RabbitMQ messages**.  
- Each service uses **its own database** for separation of concerns.  
- Both services are deployed in Kubernetes and exposed via an **NGINX ingress**.

---

## Design Discussion

- **Microservices**: Each service handles a distinct responsibility: `UserService` (authentication) vs `TimeTrackingService` (work sessions).  
- **Database per service**: Reduces coupling and allows independent scaling.  
- **Messaging**: RabbitMQ decouples services; `TimeTrackingService` validates users asynchronously.  
- **Resilience**: Background services reconnect to RabbitMQ automatically.  
- **Scalability**: Both services can scale horizontally using Kubernetes replicas.

---

## Security Discussion

- **JWT Authentication**: All APIs require JWT Bearer tokens.  
- **Password Storage**: HMACSHA512 password hashing with per-user salts.  
- **Message Validation**: `TimeTrackingService` validates users via RabbitMQ before writing sessions.  
- **Secrets Management**: In Kubernetes, database passwords and JWT keys should be stored in **Secrets** (currently in env vars for development).  
- **HTTPS**: Recommended to expose ingress via HTTPS in production.

---

## RabbitMQ Message Flow

1. `TimeTrackingService` needs to validate a user before check-in/out.  
2. It publishes a message to **`user.validation.exchange`** with `routingKey=validate`.  
3. `UserServiceRabbitMq` listens on **`user.validation.requests`**, checks the user in the database, and replies via the **ReplyTo** queue.  
4. `TimeTrackingService` receives the response and proceeds if valid.

**Flow diagram:**

```
TimeTrackingService --> [validate user message] --> RabbitMQ --> UserService
UserService --> [validation response] --> RabbitMQ --> TimeTrackingService
```

---

## Kubernetes Deployment Guide

### Prerequisites
- Kubernetes cluster (minikube, Docker Desktop, or cloud provider)  
- `kubectl` configured  
- NGINX ingress controller  

### Deploy

```bash
kubectl apply -f k8s/sqlserver.yaml
kubectl apply -f k8s/rabbitmq.yaml
kubectl apply -f k8s/userservice.yaml
kubectl apply -f k8s/timetrackingservice.yaml
kubectl apply -f k8s/ingress.yaml
```

### Notes
- SQL Server uses a **PersistentVolumeClaim** for data persistence.  
- RabbitMQ exposed via ports **5672** (AMQP) and **15672** (management UI).  
- Services are reachable through ingress:
  - `http://local.svea.com/users` → UserService  
  - `http://local.svea.com/timetracking` → TimeTrackingService

---

## How the System Works End-to-End

1. A company registers via `/users/companies` on `UserService`.  
2. Users register via `/users/register`. JWT token returned for authentication.  
3. Users access `/timetracking/checkin` or `/checkout`.  
4. `TimeTrackingService` sends a **RabbitMQ validation request** to `UserService`.  
5. If user is valid, a **work session** is created/updated in `TimeDb`.  
6. Users can retrieve history via `/timetracking/history`.  

**Key points:**
- All endpoints require authentication.  
- Background services handle RabbitMQ message sending and receiving.  
- Both services can scale independently in Kubernetes.  

---

## Development

### Run locally with Docker Compose

```bash
docker-compose up --build
```

- UserService → `http://localhost:5001/users`  
- TimeTrackingService → `http://localhost:5002/timetracking`  
- RabbitMQ → `http://localhost:15672` (guest/guest)  
- SQL Server → `localhost:1433` (SA/BenjiSql123!)  

---

## Tech Stack

- **.NET 9**  
- **Entity Framework Core**  
- **SQL Server 2022**  
- **RabbitMQ 3**  
- **Kubernetes & NGINX ingress**  
- **Docker & Docker Compose**

