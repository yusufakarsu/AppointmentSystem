# **Appointment System API**

## **Overview**
This project is an **appointment scheduling system** that enables customers to book time slots with sales managers based on **availability, language, products, and customer ratings**. 

The system is **built from scratch** using **ASP.NET Core, Entity Framework Core**, and follows a **modular architecture** with repositories, services, and controllers.

---

## **1. Project Architecture**

### **1.1. Technology Stack**
- **Backend:** ASP.NET Core 8.0
- **Database:** PostgreSQL (via Docker)
- **ORM:** Entity Framework Core
- **Logging:** Built-in `ILogger<T>`
- **Testing:** NUnit & Moq
- **Containerization:** Docker & Docker Compose

### **1.2. Project Structure**
```
📂 AppointmentSystem.Api (Main API)
   ├── Controllers
   ├── Startup.cs
📂 AppointmentSystem.Business (Services & Logic)
   ├── Services
   ├── Extensions
📂 AppointmentSystem.Data (Database & Entities)
   ├── DbContext
   ├── Repositories
   ├── Interfaces
📂 AppointmentSystem.Models (DTOs)
📂 AppointmentSystem.Tests (Unit Tests - NUnit & Moq)
```

---

## **2. Key Features**

✅ **Sales Managers & Slots Relationship**: Defined with proper foreign key constraints.  
✅ **Efficient Slot Filtering**: Checks for overlapping booked slots to prevent double-booking.  
✅ **Service Layer Abstraction**: Clean separation of concerns using services and repositories.  
✅ **Validation & Error Handling**: Ensures proper request validation and structured error messages.  
✅ **Unit Tests with NUnit & Moq**: Verifies API behavior with service mock testing.  
✅ **Docker & Docker Compose Support**: Includes PostgreSQL database initialization.  

---

## **3. API Endpoints**

### **3.1. Get Available Appointment Slots**
#### **Request**
```http
POST /calendar/query
Content-Type: application/json
```
```json
{
  "date": "2024-05-03",
  "language": "English",
  "products": ["SolarPanels"],
  "rating": "Gold"
}
```
#### **Response (200 OK)**
```json
[
  {
    "start_date": "2024-05-03T10:00:00Z",
    "available_count": 3
  },
  {
    "start_date": "2024-05-03T11:00:00Z",
    "available_count": 2
  }
]
```
#### **Response (400 Bad Request)**
```json
{
  "message": "Date is required."
}
```

---

## **4. Database Configuration & Docker Setup**

### **4.1. Docker Compose Setup**
The system is configured to run PostgreSQL as a container.

#### **docker-compose.yml**
```yaml
version: "3.8"

services:
  database:
    container_name: enpal-coding-challenge-db
    image: postgres:16
    restart: always
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: mypassword123!
      POSTGRES_DB: coding-challenge
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./database/init.sql:/docker-entrypoint-initdb.d/init.sql

  appointmentsystem.api:
    container_name: appointment-api
    build:
      context: .
      dockerfile: AppointmentSystem.Api/Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Host=database;Port=5432;Database=coding-challenge;Username=postgres;Password=mypassword123!
      - ASPNETCORE_URLS=http://+:3000
      - ASPNETCORE_ENVIRONMENT=Development
    ports:
      - "3000:3000"
    depends_on:
      database:
        condition: service_healthy

volumes:
  postgres_data:
```

### **4.2. Running the Project**
```sh
# Start the application with Docker Compose
docker-compose up --build
```

---

## **5. Running Unit Tests**

To run the NUnit tests:
```sh
# In Visual Studio Test Explorer
Run Tests

# Or use .NET CLI
cd AppointmentSystem.Tests

# Run tests
dotnet test
```

---

## **📌 Summary**
🚀 **This project was built from scratch** to provide an optimized appointment booking system with proper validation, clean architecture, and robust testing.  

**Key Benefits:**  
✅ **Well-structured codebase** using services & repositories  
✅ **Optimized database queries** for performance  
✅ **Unit tests** to ensure API reliability  
✅ **Dockerized environment** for easy deployment  

---

🔗 **GitHub Repository:** *Add GitHub link here*  
📌 **Author:** Yusuf  
