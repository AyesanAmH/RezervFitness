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
        UUID  Id PK
        string Email
    }

    Business {
        UUID Id PK
        string Name
    }

    Package {
        UUID Id PK
        UUID UserId FK
        UUID BusinessId FK
        int TotalCredits
        int RemainingCredits
        datetime ExpiryDate
        datetime PurchasedAt
    }

    TimetableSchedule {
        UUID Id PK
        UUID BusinessId FK
        string ClassName
        string InstructorName
        datetime StartTime
        datetime EndTime
        int AvailableSlots
    }

    Booking {
        UUID Id PK
        UUID UserId FK
        UUID TimetableScheduleId FK
        UUID PackageId FK
        int Status
        datetime BookedAt
        datetime CancelledAt
        bool CreditRefunded
    }

    WaitlistEntry {
        UUID Id PK
        UUID UserId FK
        UUID TimetableScheduleId FK
        UUID PackageId FK
        int Status
        datetime JoinedAt
    }