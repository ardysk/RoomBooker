# RoomBooker - Enterprise Room Reservation System

**RoomBooker** is a robust, enterprise-level web application designed to streamline the management of corporate or academic resources. The system eliminates the critical issue of "double bookings," ensures transparency in resource availability, and automates scheduling workflows through seamless integration with Google Calendar.

---

## 1. Project Overview & Objectives

The primary goal of this project was to engineer a complete .NET-based web application that demonstrates advanced proficiency in software architecture, database design, and full-stack development.

### User Roles
The application supports two distinct role-based access levels:
* **Administrators:** Have full control over the system. They manage resources (Rooms), users, oversee the reservation approval workflow (Approve/Reject), and analyze occupancy reports.
* **Users:** Can browse the interactive schedule, request room reservations, manage their own bookings, and synchronize them with their private Google Calendar.

---

## 2. Technology Stack & Architecture

The solution implements **Clean Architecture** principles to ensure separation of concerns, scalability, and testability.

### Backend
* **Framework:** .NET 8 (ASP.NET Core Web API)
* **ORM:** Entity Framework Core 8 (Code First approach)
* **Database:** Microsoft SQL Server
* **Authentication:** JWT (JSON Web Tokens) + OAuth 2.0 (Google Integration)
* **Validation:** FluentValidation & DataAnnotations + Custom Business Logic
* **Testing:** xUnit + FluentAssertions

### Frontend
* **Framework:** Blazor Server (.NET 8)
* **UI Library:** Bootstrap 5 (Responsive Design)
* **State Management:** Custom `AuthenticationStateProvider` with persistence via `ProtectedLocalStorage`

### Project Structure
1.  **`RoomBooker.Core`**: The domain heart. Contains Entities, Interfaces (Repository/Service Pattern), and DTOs. No external dependencies.
2.  **`RoomBooker.Infrastructure`**: Technical implementation. Includes EF Core DbContext, concrete Services (`ReservationService`, `GoogleAuthService`), Migrations, and Raw SQL handlers.
3.  **`RoomBooker.Api`**: Presentation layer exposing data. REST API Controllers secured with `[Authorize]` attributes.
4.  **`RoomBooker.Frontend`**: The UI layer. Razor Components, Event Handling, and HTTP Client communication.

---

## 3. Key Features & Compliance

This project meets and exceeds all academic requirements:

### ? Backend & Business Logic
* **N-Tier Architecture:** Strict separation of API, Logic, and Data Access layers.
* **Comprehensive CRUD:** Full management for Rooms, Users, and Reservations.
* **Advanced Conflict Detection:** A mathematical algorithm checks for time overlaps in real-time, preventing double bookings.
* **Global Error Handling:** Centralized exception handling returning appropriate HTTP status codes (400, 401, 404, 500).

### ? Advanced Database Implementation (SQL Server)
The system leverages native SQL capabilities beyond standard ORM:
* **Trigger (`trg_Reservations_Audit`):** Automatically logs every `INSERT` or `UPDATE` on the `Reservations` table into the immutable `AuditLogs` table, tracking who changed what and when.
* **Stored Procedure (`sp_GetMonthlyRoomStats`):** Performs server-side data aggregation to generate performance reports (e.g., room occupancy per month).
* **User-Defined Function (`fn_GetReservationDurationHours`):** A scalar function used within reports to calculate meeting durations accurately.

### ? Modern Frontend (Blazor)
* **Responsive UI:** Fully functional on desktop and mobile devices.
* **Dynamic Role-Based UI:** Menus and buttons adapt dynamically based on the user's role (e.g., "Manage Rooms" is invisible to standard users).
* **Google Calendar Sync (Killer Feature):** Users can link their Google Account via OAuth2. Approved reservations are pushed to their primary calendar asynchronously in the background (using `Task.Run` to maintain UI responsiveness).

---

## 4. Database Schema (ERD)

The application relies on a relational SQL Server database with the following structure:

```mermaid
erDiagram
    User ||--o{ Reservation : "creates"
    User ||--o{ AuditLog : "triggers"
    User ||--o{ Review : "writes"
    Room ||--o{ Reservation : "hosts"
    Room ||--o{ MaintenanceWindow : "has"
    Room ||--o{ Review : "receives"
    
    User {
        int UserId PK
        string Email "Unique Index"
        string HashedPassword
        string Role "Admin/User"
        string GoogleAccessToken
        string GoogleRefreshToken
    }

    Room {
        int RoomId PK
        string Name
        int Capacity
        bool IsActive
    }

    Reservation {
        int ReservationId PK
        int UserId FK
        int RoomId FK
        datetime StartTimeUtc
        datetime EndTimeUtc
        string Status "Pending/Approved/Rejected"
        string Purpose
    }

    AuditLog {
        int LogId PK
        string Action
        string Details
        datetime ActionTimestamp "Default GETDATE()"
    }

    MaintenanceWindow {
        int BlockId PK
        int RoomId FK
        datetime StartTime
        datetime EndTime
    }

    Review {
        int ReviewId PK
        int RoomId FK
        int UserId FK
        int Rating
        string Comment
    }
    ## 5. SQL Programmability Details

The project meets advanced database requirements by implementing raw SQL logic directly on the server side.

### 1. Audit Trigger
Ensures data integrity and accountability by automatically logging all changes made to the `Reservations` table.

```sql
CREATE OR ALTER TRIGGER trg_Reservations_Audit
ON Reservations
AFTER UPDATE, INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditLogs (EntityType, EntityId, Action, UserId, ActionTimestamp, Details)
    SELECT 
        'Reservation',
        i.ReservationId,
        CASE WHEN d.ReservationId IS NULL THEN 'Create' ELSE 'Update' END,
        i.UserId,
        GETUTCDATE(),
        CONCAT('Status: ', i.Status)
    FROM inserted i
    LEFT JOIN deleted d ON i.ReservationId = d.ReservationId
END

### 2. Helper Function
A scalar function used to calculate the duration of a reservation in hours. This logic is reused in reporting procedures.

```sql
CREATE OR ALTER FUNCTION dbo.fn_GetReservationDurationHours (@Start DateTime, @End DateTime)
RETURNS INT
AS
BEGIN
    RETURN DATEDIFF(HOUR, @Start, @End);
END;
### 3. Reporting Stored Procedure
Generates aggregated statistics for the admin dashboard, calculating total reservations and hours per room for a specific month.

```sql
CREATE OR ALTER PROCEDURE dbo.sp_GetMonthlyRoomStats
    @Month INT,
    @Year INT
AS
BEGIN
    SELECT 
        r.Name AS RoomName,
        COUNT(res.ReservationId) AS ReservationCount,
        SUM(dbo.fn_GetReservationDurationHours(res.StartTimeUtc, res.EndTimeUtc)) AS TotalHours
    FROM Rooms r
    LEFT JOIN Reservations res ON r.RoomId = res.RoomId
    WHERE MONTH(res.StartTimeUtc) = @Month AND YEAR(res.StartTimeUtc) = @Year
    GROUP BY r.Name;
END;

## 6. Setup & Installation Guide

Follow these steps to run the project locally.

### Prerequisites
* .NET 8 SDK
* SQL Server (LocalDB or Full Instance)
* Visual Studio 2022

### Step 1: Database Configuration
1.  Open `RoomBooker.Api/appsettings.json`.
2.  Verify that `ConnectionStrings:DefaultConnection` points to your local SQL instance.
3.  Open a terminal in the `RoomBooker.Infrastructure` directory and run the following command to create the database and seed initial data:
    ```powershell
    dotnet ef database update
    ```
    *Note: The system will automatically seed an Administrator account: `admin@roombooker.local` / `admin123`.*

### Step 2: Google API Configuration (Optional)
To fully enable the Google Calendar Sync feature:
1.  Create a new project in the **Google Cloud Console**.
2.  Enable the **Google Calendar API**.
3.  Generate **OAuth 2.0 Client ID** and **Client Secret**.
4.  Add them to the `Google` section in `RoomBooker.Api/appsettings.json`:
    ```json
    "Google": {
      "ClientId": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    }
    ```

### Step 3: Running the Application
In Visual Studio, configure the solution to start multiple projects:
1.  Right-click the Solution -> **Properties**.
2.  Select **Multiple Startup Projects**.
3.  Set `RoomBooker.Api` to **Start**.
4.  Set `RoomBooker.Frontend` to **Start**.

Alternatively, run via terminal:
```bash
# Terminal 1 (Backend)
cd RoomBooker.Api
dotnet run

# Terminal 2 (Frontend)
cd RoomBooker.Frontend
dotnet run

---

## 7. Author

**Name:** Arkadiusz Kowalczyk
**Student ID:** 168681
**Course:** DBwAI
**Date:** November 2025