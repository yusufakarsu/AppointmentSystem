# **Appointment System API**

## **📌 Overview**
The **Appointment System API** is designed to handle **customer appointment scheduling** efficiently. Customers can book available time slots with sales managers based on:
- **Availability**
- **Language preferences**
- **Products offered**
- **Customer rating compatibility**

### **🚀 Tech Stack**
- **ASP.NET Core** (Web API Framework)
- **Entity Framework Core** (ORM for database operations)
- **PostgreSQL** (Database)
- **Docker & Docker Compose** (Containerization & Deployment)
- **NUnit & Moq** (Unit Testing)

---

## **🔧 Project Structure**
This project follows a **layered architecture** ensuring scalability, maintainability, and separation of concerns.

```
📂 AppointmentSystem.Api (API Layer)
   ├── Controllers/ (Handles HTTP requests)
   ├── Program.cs
📂 AppointmentSystem.Business (Business Logic Layer)
   ├── Services/ (Handles application logic)
   ├── Extensions/ (Helper methods)
📂 AppointmentSystem.Data (Data Access Layer)
   ├── DbContext/ (Database context)
   ├── Repositories/ (Data fetching logic)
   ├── Interfaces/ (Repository contracts)
📂 AppointmentSystem.Models (DTOs - Data Transfer Objects)
📂 AppointmentSystem.Tests (Unit Tests - NUnit & Moq)
```

---

## **🔥 Features & Implementation Details**

### **1️⃣ Database Schema & Entity Relationships**
- **SalesManager** and **Slot** entities are structured with a **one-to-many relationship**.
- The **`Slots`** navigation property is included inside **SalesManager** for better **query performance**.

```csharp
public class SalesManager
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    // List of available slots
    public virtual List<Slot> Slots { get; set; } = new();
}
```

```csharp
public class Slot
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(SalesManager))]
    public int SalesManagerId { get; set; }
    public virtual SalesManager SalesManager { get; set; }
}
```

✅ **Result:** The system **retrieves managers and their slots in a single query** instead of multiple database calls.

---

### **2️⃣ Repository Layer Implementation**
The repository pattern is used to **abstract database operations**.

```csharp
public async Task<List<SalesManager>> GetSalesManagersWithSlotsAsync()
{
    return await _context.SalesManagers
        .Include(sm => sm.Slots)
        .AsNoTracking()
        .ToListAsync();
}
```
✅ **Result:** **Efficient data fetching**, reducing unnecessary database queries.

---

### **3️⃣ Business Logic Implementation**
The **service layer** processes and filters data before returning results.

```csharp
public async Task<List<AvailableTimeSlotDto>> GetAvailableSlotsAsync(AppointmentRequestDto request)
{
    var salesManagers = await _repository.GetSalesManagersWithSlotsAsync();
    var matchingManagers = salesManagers.Where(m => 
        m.Languages.Contains(request.Language) &&
        m.Products.Contains(request.Products) &&
        m.CustomerRatings.Contains(request.Rating)).ToList();

    var availableSlots = matchingManagers.SelectMany(m => m.Slots).Where(s => !s.Booked).ToList();

    return availableSlots.Select(s => new AvailableTimeSlotDto 
    {
        StartDate = s.StartDate,
        AvailableCount = 1 // Each slot is available
    }).ToList();
}
```
✅ **Result:** **Filters managers based on language, product, and rating, then retrieves available slots**.

---

### **4️⃣ Overlap Checking via Extension Methods**
The **`SlotExtensions`** helper class improves code **reusability**.

```csharp
public static class SlotExtensions
{
    public static bool OverlapWith(this Slot slot, Slot other)
    {
        return slot.SalesManagerId == other.SalesManagerId &&
               slot.StartDate < other.EndDate &&
               slot.EndDate > other.StartDate;
    }
}
```

✅ **Result:** The system **detects and removes overlapping slots** dynamically.

---

### **5️⃣ Exception Handling**
The **CalendarController** now gracefully handles **unexpected errors**.

```csharp
try
{
    var availableSlots = await _appointmentService.GetAvailableSlotsAsync(request);
    return Ok(availableSlots);
}
catch (Exception ex)
{
    _logger.LogError(ex, "An unexpected error occurred.");
    return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
}
```
✅ **Result:** The system **returns a structured error message instead of crashing**.

---

## **🐳 Docker-Based Deployment**
I **containerized** the project using **Docker Compose** for **easy deployment**.

### **1️⃣ PostgreSQL Database Setup**
The **PostgreSQL 16** database container initializes automatically.

```yaml
services:
  database:
    image: postgres:16
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: mypassword123!
      POSTGRES_DB: coding-challenge
    ports:
      - "5432:5432"
```

---

### **2️⃣ Preventing Duplicate Initialization**
A **custom command** ensures the database does **not reset** on every restart.

```yaml
command: >
  bash -c "if [ ! -f /var/lib/postgresql/data/PG_VERSION ]; then
  echo 'Initializing database...';
  docker-entrypoint.sh postgres;
  else
  echo 'Database already initialized, skipping init';
  exec postgres;
  fi"
```

✅ **Ensures database persistence** and **prevents accidental resets**.

---

### **3️⃣ Automatic SQL Initialization**
A **SQL script initializes the database** only on first startup.

```yaml
volumes:
  - ./database/init.sql:/docker-entrypoint-initdb.d/init.sql
```

✅ **Ensures required tables exist before API starts.**

---

### **4️⃣ API Waits for Database Readiness**
A **health check ensures** the API does **not start before the database is ready**.

```yaml
depends_on:
  database:
    condition: service_healthy

healthcheck:
  test: ["CMD", "pg_isready", "-U", "postgres"]
  interval: 10s
  retries: 5
```
✅ **Prevents API crashes due to missing database connections**.

---

## **🚀 Running the System**
### **Start the System**
```sh
docker-compose up --build
```
📌 **This will:**
- Start PostgreSQL
- Initialize tables
- Launch the API on **http://localhost:3000**

---

### **Stopping the System**
```sh
docker-compose down
```
✅ **Stops all services and preserves data.**

---

## **📌 Final Summary**
✅ **SalesManagers and Slots loaded in a single query**  
✅ **Overlap checking refactored into `SlotExtensions`**  
✅ **Improved error handling with proper status codes**  
✅ **Robust unit tests for API behavior validation**  
✅ **Fully Dockerized Deployment with PostgreSQL**  

🔥 **This project is optimized for performance, maintainability, and scalability.** 🚀  
