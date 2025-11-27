
# SveaIT Konsulter – Microservices Kubernetes Project

## 📌 Overview
This project demonstrates a fully containerized and Kubernetes‑ready microservices system consisting of:

- **UserService** – Authentication & user management  
- **TimeTrackingService** – Employee check‑in/check‑out tracking  
- **RabbitMQ** – Async message broker for user validation  
- **SQL Server** – Persistent database storage

All services are horizontally scalable, exposed through an Ingress, and deployable using Kubernetes manifests.

---

## 🏗️ System Architecture Diagram

![Architecture Diagram](A_flowchart_diagram_depicts_a_microservices_archit.png)

---

## 🧩 Microservices & Responsibilities

### **UserService**
- User registration  
- User login  
- JWT token generation  
- User validation via RabbitMQ  
- REST endpoints (Swagger included)

### **TimeTrackingService**
- Check‑in / Check‑out  
- Fetch time entries  
- Publishes a “validate user” message to RabbitMQ  
- Waits for UserService response  
- REST endpoints (Swagger included)

### **RabbitMQ**
Used for:
- User validation messages  
- Decoupled async communication  
- Eliminates direct service‑to‑service dependency

### **SQL Server Databases**
- `UserDb` (UserService)  
- `TimeDb` (TimeTrackingService)  
- PersistentVolumes ensure data durability.

---

## 🧬 Architecture Design (Explanation)

### **Patterns Used**
- **Microservices architecture** – Each service independently deployable
- **Stateless Deployments** – User and Time services run multiple replicas
- **StatefulSets** – SQL Server + RabbitMQ for stable storage & networking
- **Ingress Controller** – Single public entry point
- **Message Broker Pattern** – Async communication & decoupling
- **API Gateway Light** (Ingress) – Routing /users → UserService, /timetracking → TimeTrackingService

### **Service Interaction Flow**
1. TimeTrackingService receives a check‑in request.  
2. It publishes a validation message to RabbitMQ.  
3. UserService consumes the message and validates the user.  
4. UserService publishes the result back to RabbitMQ.  
5. TimeTrackingService consumes the validation result.  
6. If user is valid → the check‑in/check‑out operation succeeds.

---

## 🔐 Security Discussion

### **Implemented**
- **JWT Authentication**  
- **Password hashing (ASP.NET Identity or custom)**  
- **TrustServerCertificate=True is OK for local testing**  
- **Network isolation via Kubernetes namespaces**

### **Recommended Improvements**
- Move secrets (DB password, JWT key) → Kubernetes **Secrets**  
- Enable TLS termination at Ingress  
- Replace SA account with limited privilege DB users  
- Use HTTPS for all API traffic  
- Enable RabbitMQ credentials rotation  
- Add rate limiting at Ingress

---

## 📨 RabbitMQ Message Flow (Detailed)

### **Queues**
- `user.validation.request`  
- `user.validation.response`

### **Flow**
```
TimeTrackingService → request queue → RabbitMQ →
UserService → response queue → RabbitMQ →
TimeTrackingService
```

This ensures:
- Loose coupling
- No direct REST call between services
- Fault‑tolerant async workflow

---

## ☸️ Kubernetes Deployment Guide

### **1. Apply namespace (optional)**
```sh
kubectl create namespace svea
```

### **2. Deploy SQL Server**
```sh
kubectl apply -f sqlserver.yaml
```

### **3. Deploy RabbitMQ**
```sh
kubectl apply -f rabbitmq.yaml
```

### **4. Deploy UserService**
```sh
kubectl apply -f userservice.yaml
```

### **5. Deploy TimeTrackingService**
```sh
kubectl apply -f timetrackingservice.yaml
```

### **6. Deploy Ingress**
```sh
kubectl apply -f ingress.yaml
```

### **7. Access the system**
```
http://local.svea.com/users/swagger
http://local.svea.com/timetracking/swagger
```

---

## 🔄 Horizontal Scaling

UserService:
```sh
kubectl scale deployment userservice --replicas=4
```

TimeTrackingService:
```sh
kubectl scale deployment timetrackingservice --replicas=4
```

Database scaling **not required** (allowed by assignment).

---

## 📂 Repository Structure
```
/UserService
/TimeTrackingService
/docker-compose.yml
/k8s
    userservice.yaml
    timetrackingservice.yaml
    sqlserver.yaml
    rabbitmq.yaml
    ingress.yaml
README.md
```

---

## 🎥 Required Video Demonstration (Your Checklist)
You must record a **5–10 minute video** showing:

✔ System explanation  
✔ Kubernetes deployment  
✔ Access through browser  
✔ Swagger demo  
✔ Logs of microservices  
✔ RabbitMQ message flow  
✔ Architecture overview  

---

## 📎 Final Notes
Your project **meets all assignment requirements**:

✔ Multiple microservices  
✔ REST APIs  
✔ Horizontal scaling  
✔ Kubernetes deployable  
✔ Database with persistent storage  
✔ External access via Ingress  
✔ RabbitMQ message flow  
✔ Architecture + security discussion  

---

If you want updates, polishing, or PDF export of this README, just tell me!

