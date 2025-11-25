# SveaITKonsulter System

## Overview

This project is a **microservices-based time tracking and user management system** built with .NET 9, SQL Server, RabbitMQ, and Kubernetes. It consists of:

- **UserService** – Handles user registration, authentication, and company management.  
- **TimeTrackingService** – Allows users to check in/out and track work sessions.  
- **RabbitMQ** – Handles asynchronous communication and user validation between services.  
- **SQL Server** – Stores user and work session data.

The system is designed for **scalability and security

---

## Architecture

```mermaid
graph TB
    %% External Clients
    Browser[Web Browser] --> Ingress
    Swagger[Swagger UI] --> Ingress
    
    %% Kubernetes Cluster
    subgraph K8S["Kubernetes Cluster"]
        Ingress[Ingress] --> UserPod
        Ingress --> TimePod
        
        %% UserService Section
        subgraph UserPod["UserService Pod"]
            UserSvc[UserService<br/>REST API]
            style UserPod stroke:#2e7d32,stroke-width:3px
            UserPodLabel[Deployment]:::deploymentLabel
        end
        
        subgraph UserDbPod["UserDB Pod"]
            UserDB[(SQL Server<br/>UserDb)]
            style UserDbPod stroke:#e65100,stroke-width:3px
            UserDbLabel[StatefulSet]:::statefulsetLabel
        end
        
        %% TimeTrackingService Section
        subgraph TimePod["TimeTrackingService Pod"]
            TimeSvc[TimeTrackingService<br/>REST API]
            style TimePod stroke:#2e7d32,stroke-width:3px
            TimePodLabel[Deployment]:::deploymentLabel
        end
        
        subgraph TimeDbPod["TimeDB Pod"]
            TimeDB[(SQL Server<br/>TimeDb)]
            style TimeDbPod stroke:#e65100,stroke-width:3px
            TimeDbLabel[StatefulSet]:::statefulsetLabel
        end
        
        %% Message Broker
        subgraph RabbitPod["RabbitMQ Pod"]
            RabbitMQ[RabbitMQ<br/>Message Broker]
            style RabbitPod stroke:#880e4f,stroke-width:3px
            RabbitLabel[StatefulSet]:::statefulsetLabel
        end
        
        %% Database Connections
        UserSvc --> UserDB
        TimeSvc --> TimeDB
        
        %% Async Validation Flow
        TimeSvc -->|"1. User validation request"| RabbitMQ
        RabbitMQ -->|"2. Forward validation request"| UserSvc
        UserSvc -->|"3. Boolean response"| RabbitMQ
        RabbitMQ -->|"4. Deliver validation result"| TimeSvc
    end

    %% API Endpoints Legend
    subgraph APIs["REST API Endpoints"]
        UserAPI["UserService:<br/>• Register<br/>• Login<br/>• Authenticate"]
        TimeAPI["TimeTrackingService:<br/>• Check-in<br/>• Check-out"]
    end

    %% Kubernetes Resources Legend
    subgraph K8SResources["Kubernetes Resource Types"]
        DeployLabel[Deployment: Stateless Services]:::deploymentLabel
        StatefulLabel[StatefulSet: Stateful Services]:::statefulsetLabel
    end

    %% Styling
    classDef k8s fill:#f0f8ff,stroke:#4682b4,stroke-width:3px
    classDef servicePod fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    classDef dbPod fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef messagePod fill:#fce4ec,stroke:#880e4f,stroke-width:2px
    classDef service fill:#90ee90,stroke:#32cd32,stroke-width:1px
    classDef db fill:#ffd700,stroke:#ffa500,stroke-width:1px
    classDef message fill:#ffb6c1,stroke:#ff69b4,stroke-width:1px
    classDef external fill:#e3f2fd,stroke:#1565c0,stroke-width:1px
    classDef api fill:#e1f5fe,stroke:#0288d1,stroke-width:1px,dashed
    classDef deploymentLabel fill:#2e7d32,stroke:#1b5e20,color:white
    classDef statefulsetLabel fill:#e65100,stroke:#bf360c,color:white
    
    class K8S k8s
    class UserPod,TimePod servicePod
    class UserDbPod,TimeDbPod dbPod
    class RabbitPod messagePod
    class UserSvc,TimeSvc service
    class UserDB,TimeDB db
    class RabbitMQ message
    class Browser,Swagger,Ingress external
    class APIs api
    class DeployLabel,StatefulLabel,UserPodLabel,TimePodLabel,UserDbLabel,TimeDbLabel,RabbitLabel deploymentLabel,statefulsetLabel

---

Design Discussion

Microservices: Each service handles a distinct responsibility: UserService (authentication) vs TimeTrackingService (work sessions).
Database per service: Reduces coupling and allows independent scaling.
Messaging: RabbitMQ decouples services; TimeTrackingService validates users asynchronously.
Resilience: Background services reconnect to RabbitMQ automatically.
Scalability: Both services can scale horizontally using Kubernetes replicas.
Security Discussion

JWT Authentication: All APIs require JWT Bearer tokens.
Password Storage: HMACSHA512 password hashing with per-user salts.
Message Validation: TimeTrackingService validates users via RabbitMQ before writing sessions.
Secrets Management: In Kubernetes, database passwords and JWT keys should be stored in Secrets (currently in env vars for development).
HTTPS: Recommended to expose ingress via HTTPS in production.
RabbitMQ Message Flow

TimeTrackingService needs to validate a user before check-in/out.
It publishes a message to user.validation.exchange with routingKey=validate.
UserServiceRabbitMq listens on user.validation.requests, checks the user in the database, and replies via the ReplyTo queue.
TimeTrackingService receives the response and proceeds if valid.
Flow diagram:

text
TimeTrackingService --> [validate user message] --> RabbitMQ --> UserService
UserService --> [validation response] --> RabbitMQ --> TimeTrackingService
Kubernetes Deployment Guide

Prerequisites

Kubernetes cluster (minikube, Docker Desktop, or cloud provider)
kubectl configured
NGINX ingress controller
Deploy

bash
kubectl apply -f k8s/sqlserver.yaml
kubectl apply -f k8s/rabbitmq.yaml
kubectl apply -f k8s/userservice.yaml
kubectl apply -f k8s/timetrackingservice.yaml
kubectl apply -f k8s/ingress.yaml
Notes

SQL Server uses a PersistentVolumeClaim for data persistence.
RabbitMQ exposed via ports 5672 (AMQP) and 15672 (management UI).
Services are reachable through ingress:

http://local.svea.com/users → UserService
http://local.svea.com/timetracking → TimeTrackingService
How the System Works End-to-End

A company registers via /users/companies on UserService.
Users register via /users/register. JWT token returned for authentication.
Users access /timetracking/checkin or /checkout.
TimeTrackingService sends a RabbitMQ validation request to UserService.
If user is valid, a work session is created/updated in TimeDb.
Users can retrieve history via /timetracking/history.
Key points:

All endpoints require authentication.
Background services handle RabbitMQ message sending and receiving.
Both services can scale independently in Kubernetes.
Development

Run locally with Docker Compose

bash
docker-compose up --build
UserService → http://localhost:5001/users
TimeTrackingService → http://localhost:5002/timetracking
RabbitMQ → http://localhost:15672 (guest/guest)
SQL Server → localhost:1433 (SA/BenjiSql123!)
Tech Stack

.NET 9
Entity Framework Core
SQL Server 2022
RabbitMQ 3
Kubernetes & NGINX ingress
Docker & Docker Compose
