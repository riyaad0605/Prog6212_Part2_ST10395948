# RaceDay API

A RESTful Web API for managing race day events across South Africa. Built with ASP.NET Core 8, Entity Framework Core (Code-First), and session-based authentication.

## System Overview

RaceDay lets Organisers create and manage running, walking, and cycling events, while Participants can browse events, enrol in race categories, and view their results.

## Roles

| Role | Permissions |
|------|-------------|
| **Organiser** | Create, update, delete events. Manage categories and event-category links. Capture and update race results. View all enrolments. |
| **Participant** | Browse published events. Enrol in event categories. View own enrolments and results. Withdraw from events. |

## Tech Stack

- **Framework:** ASP.NET Core 8 Web API
- **ORM:** Entity Framework Core 8 (Code-First)
- **Database:** SQL Server (LocalDB for development)
- **Auth:** Server-side sessions with BCrypt password hashing
- **Docs:** Swagger / OpenAPI via Swashbuckle
- **Testing:** xUnit with EF Core InMemory provider
- **CI/CD:** GitHub Actions (build + test on push)

## Project Structure

```
RaceDay_Part2/
├── RaceDay.sln
├── RaceDay.API/
│   ├── Controllers/       # API endpoint controllers
│   ├── Data/              # DbContext with Code-First config and seed data
│   ├── DTOs/              # Data transfer objects for requests/responses
│   ├── Models/            # EF Core entity models
│   ├── Program.cs         # App startup and middleware config
│   └── appsettings.json   # Connection string and app settings
├── RaceDay.Tests/
│   ├── Helpers/           # Test utilities (in-memory DB, mock session)
│   ├── AuthControllerTests.cs
│   ├── EventsControllerTests.cs
│   ├── EnrolmentsControllerTests.cs
│   └── ResultsControllerTests.cs
├── docs/
│   ├── API_Endpoint_Plan.md
│   ├── RaceDay_ERD.png
│   └── RaceDay_Schema.sql
└── .github/workflows/
    └── ci.yml             # GitHub Actions CI/CD pipeline
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Setup
1. Clone the repository
2. Open `RaceDay.sln` in Visual Studio 2022
3. Update the connection string in `appsettings.json` if needed
4. Run the project (F5) — the database is created automatically on first run
5. Swagger UI opens at `https://localhost:{port}/swagger`

### Running Tests
```bash
dotnet test
```

### Test Accounts (Seed Data)
| Email | Password | Role |
|-------|----------|------|
| organiser1@raceday.co.za | Pass123! | Organiser |
| organiser2@raceday.co.za | Pass123! | Organiser |
| runner1@raceday.co.za | Pass123! | Participant |
| runner2@raceday.co.za | Pass123! | Participant |

## API Endpoints

| Method | Route | Description | Access |
|--------|-------|-------------|--------|
| POST | /api/auth/register | Register a new account | Public |
| POST | /api/auth/login | Log in and start session | Public |
| POST | /api/auth/logout | End session | Authenticated |
| GET | /api/users/profile | Get own profile | Authenticated |
| PUT | /api/users/profile | Update own name | Authenticated |
| GET | /api/events | List events | Authenticated |
| GET | /api/events/{id} | Get event details | Authenticated |
| POST | /api/events | Create event | Organiser |
| PUT | /api/events/{id} | Update event | Organiser (owner) |
| DELETE | /api/events/{id} | Delete event | Organiser (owner) |
| GET | /api/categories | List categories | Authenticated |
| GET | /api/categories/{id} | Get category | Authenticated |
| POST | /api/categories | Create category | Organiser |
| PUT | /api/categories/{id} | Update category | Organiser |
| DELETE | /api/categories/{id} | Delete category | Organiser |
| GET | /api/eventcategories/event/{id} | Get event's categories | Authenticated |
| POST | /api/eventcategories | Link category to event | Organiser (owner) |
| DELETE | /api/eventcategories/{id} | Remove category link | Organiser (owner) |
| POST | /api/enrolments | Enrol in category | Participant |
| GET | /api/enrolments/my | My enrolments | Participant |
| GET | /api/enrolments/event/{id} | Event enrolments | Authenticated |
| DELETE | /api/enrolments/{id} | Withdraw | Participant (own) |
| POST | /api/results | Capture result | Organiser (owner) |
| GET | /api/results/event/{id} | Event results | Authenticated |
| PUT | /api/results/{id} | Update result | Organiser (owner) |



GitHub Actions runs on every push and pull request to `main`:
1. Restores NuGet packages
2. Builds the solution in Release mode
3. Runs all xUnit tests



## Video Demonstration  <img width="1587" height="747" alt="Capture" src="https://github.com/user-attachments/assets/7041600f-c979-476b-a2d7-dfc7de37700a" />






GitHub Copilot was used to assist with boilerplate code generation and unit test scaffolding during development. All logic, architecture decisions, and final code were reviewed and validated manually.
