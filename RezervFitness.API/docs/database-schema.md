erDiagram
    User ||--o{ Package : owns
    Business ||--o{ Package : offers
    Business ||--o{ TimetableSchedule : schedules
    User ||--o{ Booking : makes
    TimetableSchedule ||--o{ Booking : receives
    Package ||--o{ Booking : uses
    User ||--o{ WaitlistEntry : joins
    TimetableSchedule ||--o{ WaitlistEntry : has
    Package ||--o{ WaitlistEntry : references

    User {
        int Id PK
        string Email
    }

    Business {
        int Id PK
        string Name
    }

    Package {
        int Id PK
        int UserId FK
        int BusinessId FK
        int TotalCredits
        int RemainingCredits
        datetime ExpiryDate
        datetime PurchasedAt
    }

    TimetableSchedule {
        int Id PK
        int BusinessId FK
        string ClassName
        string InstructorName
        datetime StartTime
        datetime EndTime
        int AvailableSlots
    }

    Booking {
        int Id PK
        int UserId FK
        int TimetableScheduleId FK
        int PackageId FK
        int Status
        datetime BookedAt
        datetime CancelledAt
        bool CreditRefunded
    }

    WaitlistEntry {
        int Id PK
        int UserId FK
        int TimetableScheduleId FK
        int PackageId FK
        int Status
        datetime JoinedAt
    }