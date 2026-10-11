RezervFitness — Studio Booking API
1. Project Overview

RezervFitness is a simplified studio booking platform built with ASP.NET Core Web API and .NET 8.

The application allows customers to purchase class packages, view timetable schedules, book fitness classes, cancel bookings, and join waitlists.

Each package belongs to a specific customer and business. Packages can only be used to book schedules belonging to the same business. Each confirmed booking consumes one credit.

This project demonstrates backend architecture, REST API design, data modelling, business logic, database transactions, concurrency handling, Redis caching, and API documentation.

2. Technology Stack
Backend: .NET 8, ASP.NET Core Web API
Database: MySQL 8.4
ORM: Entity Framework Core
Caching: Redis 7
API Documentation: Swagger / OpenAPI
Containerization: Docker Compose
Development Environment: Visual Studio
Version Control: Git and GitHub
3. Architecture

The application follows a layered architecture to separate HTTP handling, business logic, database access, and data models.

RezervFitness.API
│
├── Controllers
│   ├── PackagesController.cs
│   ├── TimetableController.cs
│   ├── BookingsController.cs
│   └── WaitlistMaintenanceController.cs
│
├── Services
│   ├── PackageService.cs
│   ├── TimetableService.cs
│   ├── BookingService.cs
│   ├── WaitlistService.cs
│   └── TimetableCacheService.cs
│
├── DAO
│   ├── BookingDao.cs
│   ├── PackageDao.cs
│   └── TimetableDao.cs
│
├── DAO
│   └── BookingDao.cs
│
├── DTOs
│   ├── CreateBookingDto.cs
│   ├── CancelBookingDto.cs
│   ├── BookingResponseDto.cs
│   └── TimetableResponseDto.cs
│
├── Models
│   ├── User.cs
│   ├── Business.cs
│   ├── Package.cs
│   ├── TimetableSchedule.cs
│   ├── Booking.cs
│   └── WaitlistEntry.cs
│
├── Data
│   └── AppDbContext.cs
│
├── Migrations
├── Program.cs
├── appsettings.json
└── README.md



Layer Responsibilities

Controllers

Receive HTTP requests.
Validate request formats.
Call application services.
Return appropriate HTTP responses.

Services

Implement package, booking, cancellation, and waitlist business rules.
Coordinate database operations and transactions.
Apply credit deduction and refund logic.
Manage timetable cache invalidation.

DAO

Encapsulates selected database queries.
Provides database operations such as loading records with row locks.

DTOs

Define API request and response objects.
Separate the API contract from the database entity models.

Models

Represent users, businesses, packages, schedules, bookings, and waitlist entries.

AppDbContext

Configures Entity Framework Core.
Defines entity relationships, foreign keys, and indexes.
4. Database Design

The application contains six primary entities.

User

Represents a customer who purchases packages and books classes.

Business

Represents a fitness studio or business.

Package

Represents purchased class credits.

Fields:

Id
UserId
BusinessId
TotalCredits
RemainingCredits
ExpiryDate
PurchasedAt
TimetableSchedule

Represents a scheduled fitness class.

Fields:

Id
BusinessId
ClassName
InstructorName
StartTime
EndTime
AvailableSlots

AvailableSlots represents the total class capacity.

Booking

Represents a customer's booking for a timetable schedule.

Fields:

Id
UserId
TimetableScheduleId
PackageId
Status
BookedAt
CancelledAt
CreditRefunded

Booking statuses:

Confirmed
Cancelled
WaitlistEntry

Represents a customer's position on a class waitlist.

Fields:

Id
UserId
TimetableScheduleId
PackageId
Status
JoinedAt

Waitlist statuses:

Waiting
Promoted
Expired
Cancelled
Entity Relationships
One business can have many packages.
One business can have many timetable schedules.
One user can own many packages.
One user can have many bookings.
One timetable schedule can have many bookings.
One package can be referenced by multiple bookings over time.
One user can have many waitlist entries.
One timetable schedule can have many waitlist entries.
One package can be referenced by waitlist entries.

Foreign keys and indexes are configured in AppDbContext.

5. Prerequisites

Install the following before running the application:

.NET 10 SDK
Visual Studio 2026 or a compatible IDE
Docker Desktop
Git
6. Setup Instructions
Step 1: Clone the repository
git clone https://github.com/AyesanAmH/RezervFitness.git
cd RezervFitness



Open the solution file in Visual Studio.

Step 2: Start MySQL and Redis

Run Docker Compose from the directory containing docker-compose.yml.

docker compose up -d


Check the container status:

docker compose ps


The local development configuration uses:

MySQL host port: 3307
Redis host port: 6379

Check Redis connectivity:

docker exec -it rezervfitness-redis-1 redis-cli ping


Expected response:

PONG


The container name may differ if your Docker Compose configuration uses another name.

Step 3: Configure connection strings

Configure your local MySQL and Redis connections in appsettings.json or through development secrets.

Example:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3307;Database=rezervfitness;User=root;Password=YOUR_PASSWORD;",
    "Redis": "localhost:6379"
  }
}


Replace YOUR_PASSWORD with your local MySQL password.

Ensure the connection string names match those used in Program.cs.


Step 4: Restore dependencies

From the API project directory, run:

dotnet restore

Step 5: Apply database migrations

If Entity Framework Core migrations already exist, run:

dotnet ef database update


If the EF Core command-line tool is not installed:

dotnet tool install --global dotnet-ef


Run migrations from the directory containing the API project, or specify the project path when necessary.

Step 6: Run the application

In Visual Studio, set RezervFitness.API as the startup project and press F5.

Alternatively, run:

dotnet run


Use the HTTP or HTTPS URL displayed in the terminal or Visual Studio output.

Step 7: Open Swagger

Open the Swagger URL displayed by the application, typically:

https://localhost:<your-port>/swagger


Swagger allows you to test the available API endpoints.

7. API Endpoints
Method	Endpoint	Description
GET	/api/packages	List available packages
POST	/api/packages/purchase	Purchase a package using mock payment
GET	/api/timetable	List timetable schedules
GET	/api/timetable?businessId=1	Filter schedules by business
GET	/api/timetable?date=2026-10-12	Filter schedules by date
POST	/api/bookings	Book a timetable schedule
POST	/api/bookings/cancel	Cancel an existing booking
POST	/api/waitlist	Join a waitlist
POST	/api/waitlist/maintenance/expire-ended	Expire waitlist entries for ended classes

The maintenance endpoint should be restricted to authorized maintenance operations in a production deployment.

Verify the exact route attributes in the source code before submission.

Example: Book a Class

Request:

{
  "userId": 1,
  "timetableScheduleId": 1,
  "packageId": 1
}


Use valid IDs from the seeded database.

A successful booking should deduct one credit and create a confirmed booking.

Example: Cancel a Booking

Request:

{
  "bookingId": 1,
  "userId": 1
}


The API applies the refund rule according to the time remaining before the class starts.

8. Business Rules
Packages
A package belongs to one customer and one business.
Packages can only be used for schedules belonging to the same business.
Expired packages cannot be used.
A customer must have at least one remaining credit.
Each successful booking deducts one credit.
Timetable Schedules
Each schedule belongs to a business.
Each schedule includes a class name, instructor, start time, end time, and capacity.
Attendance count is calculated from confirmed bookings.
Remaining capacity is calculated as total capacity minus confirmed attendance.
Booking

The API validates:

Customer ownership of the package.
Business compatibility between package and schedule.
Package expiry and remaining credits.
Duplicate active bookings.
Overlapping confirmed bookings.
Schedule start time and end time.
Remaining class capacity.

Credit deduction and booking creation are performed within a database transaction.

Cancellation
Customers cannot cancel a class that has already started.
Cancellation more than four hours before class start refunds one credit.
Cancellation within four hours does not refund a credit.
A booking cannot be cancelled more than once.
Cancellation can trigger waitlist promotion.
Waitlist
Waitlist entries follow FIFO ordering.
When a confirmed booking is cancelled, an eligible waiting customer can be promoted.
Promotion deducts one package credit.
Ineligible entries should be skipped without deducting credits.
Waiting entries are marked expired after the class ends.
Customers who remain on the waitlist do not lose credits.
9. Concurrency Strategy

MySQL is the authoritative source for booking capacity and package credits.

The booking implementation uses database transactions and row-locking queries, including SELECT ... FOR UPDATE, to serialize competing requests for a schedule.

The intended lock order is:

Lock the timetable schedule.
Lock the related package or booking as required.
Validate capacity, credits, and booking conflicts.
Apply changes to credits, bookings, or waitlist entries.
Commit the transaction.

This approach reduces the risk of overbooking and concurrent credit deductions.

All operations that modify schedule attendance should follow a consistent lock order. Waitlist promotion must participate in the same transaction and use compatible locking behavior.

Tradeoffs

Pessimistic locking

Helps prevent competing requests from consuming the same capacity.
Serializes requests for the same schedule.
May reduce throughput for highly popular classes.

Database transactions

Keep related changes atomic when all operations participate in the same transaction.
Require careful handling of deadlocks and transaction retries.

Redis caching

Improves repeated timetable reads.
Is not used as the authority for booking capacity or credits.
Can return stale results until expiration or successful invalidation.

Limitations

Production deployment requires further integration and concurrency testing.
Cache failures, deadlocks, retries, and simultaneous waitlist operations require additional hardening.
10. Redis Caching

Timetable responses are cached using Redis.

Cache keys distinguish filter combinations, including business and date. Cached responses use a 60-second time-to-live.

A versioned key strategy can invalidate existing timetable responses without enumerating Redis keys.

Booking-related operations should invalidate the timetable cache after successful database commits.

Redis is used to improve read performance. All critical booking validation remains in MySQL.

If Redis becomes unavailable, a production implementation should ideally fall back to MySQL for timetable reads and should not report an already committed booking as failed because cache invalidation failed.

11. Seed Data and Testing

The assessment requires sample data covering:

At least 10 users.
At least 2 businesses.
At least 10 timetable schedules.
Schedules with available and full capacity.
Valid and expired packages.
Packages with insufficient remaining credits.
Confirmed and cancelled bookings.
Waitlist entries with different statuses.
Recommended Test Scenarios
Purchase a package successfully.
Book a class and verify credit deduction.
Attempt to use an expired package.
Attempt to book with insufficient credits.
Attempt to use a package from another business.
Attempt a duplicate booking.
Attempt overlapping bookings.
Attempt to book a full schedule.
Cancel more than four hours before class start.
Cancel within four hours of class start.
Verify FIFO waitlist promotion.
Verify that promotion deducts one credit.
Verify that waitlist entries expire after class end.
Send concurrent booking requests and verify capacity is not exceeded.

Verify both API responses and database state when testing business rules.

12. Assumptions
Package purchases use mock payment processing; no real payment gateway is integrated.
Each confirmed booking consumes one credit.
Refund eligibility is based on the four-hour cancellation cutoff.
Schedule timestamps use a consistent time-zone convention.
Package ownership and business matching are validated before booking.
Total schedule capacity is stored in AvailableSlots.
Attendance is derived from confirmed bookings.
Waitlist promotion follows FIFO among eligible customers.
Customers remaining on a waitlist after class end are expired without a credit deduction.
Authentication and authorization are outside the simplified assessment scope unless implemented separately.
13. Future Improvements

Potential improvements for production include:

Authentication and authorization.
Automated integration and concurrency tests.
Background workers for waitlist expiration.
Durable cache invalidation using an outbox pattern.
Retry policies for transient database failures and deadlocks.
Structured logging, metrics, tracing, and health checks.
Time-zone normalization and consistent UTC storage.
CI/CD pipelines and automated migration deployment.
Secure secrets management.
Load testing and database query optimization.
14. Author and Repository

Project: RezervFitness Studio Booking API

Repository: https://github.com/AyesanAmH/RezervFitness.git

Framework: .NET 10

Database: MySQL 8.4

Cache: Redis 7

## Database Schema

The database schema and entity relationship diagram are documented in
[Database Schema](docs/database-schema.md).

The schema uses foreign keys to connect customers, businesses, packages,
timetable schedules, bookings, and waitlist entries.