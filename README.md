================================================================================

                    RACEDAY EVENT MANAGEMENT PLATFORM

               PROG6212 Portfolio of Evidence (PoE) Final

================================================================================



Repository URL : https://github.com/ST10468948/PROG6212_POE.git

Student Number : ST10468948

CI Build Badge : https://github.com/ST10468948/PROG6212_POE/actions/workflows/ci.yml

Video Demo URL : 



--------------------------------------------------------------------------------

1. EXECUTIVE SUMMARY & SYSTEM OVERVIEW

--------------------------------------------------------------------------------

RaceDay is an enterprise-grade, cloud-ready event administration platform built

for South African road running, cycling, and trail competitions (e.g., Comrades

Marathon, Soweto Marathon, Cape Town Cycle Tour, and community fitness runs).



The platform transitions fragmented, spreadsheet-based event management into a

secure, multi-tier .NET 8 architecture featuring:

  - An ASP.NET Core 8 Web API with EF Core Code-First database persistence.

  - An ASP.NET Core 8 MVC dynamic front-end communicating strictly over HTTP.

  - Azure Blob Storage integration for event banners and participant avatars.

  - Automated xUnit integration tests validating authentication and role tiers.

  - Multi-stage Linux Docker containerization with host gateway integration.



User Roles:

  - Organisers  : Create/manage events, configure category tiers, upload race

                  banners, view participant registrations, and publish results.

  - Participants: Register/login, browse active races, enrol into categories,

                  track personal statuses, upload avatars, and view results.



--------------------------------------------------------------------------------

2. PROJECT ARCHITECTURE & COMPONENT BREAKDOWN

--------------------------------------------------------------------------------

  - RaceDay.Contracts : Shared C# class library containing POCO Data Transfer

                        Objects (DTOs) ensuring decoupled network serialization.

  - RaceDay.Api       : RESTful Web API handling business logic, BCrypt hashing,

                        session tokens, EF Core migrations, and media uploads.

  - RaceDay.Api.Tests : xUnit test suite using WebApplicationFactory and an

                        in-memory database to test auth barriers and endpoints.

  - RaceDay.Mvc       : Responsive MVC client with sporting visual theme palette

                        (#10B981, #FFFFFF, #0F172A, #F59E0B) and role-aware navigation.

  - Azure Blob Storage: Cloud persistence for race banners and user avatars.



--------------------------------------------------------------------------------

3. DATABASE SCHEMA & ENTITIES

--------------------------------------------------------------------------------

Managed through EF Core Code-First migrations on SQL Server:

  1. Roles      : System authorization tiers ('Organiser', 'Participant').

  2. Users      : Profile credentials, unique email constraint, BCrypt password.

  3. Events     : Event details, date, venue, distance, banner URL, OrganiserID.

  4. Categories : Distance, age, or competitive tiers linked to an event.

  5. Enrolments : Participant race registrations with duplicate-entry constraints.

  6. Results    : Finishing times, official positions, and total field sizes.



--------------------------------------------------------------------------------

4. REST API ENDPOINTS SUMMARY

--------------------------------------------------------------------------------

Auth:

  POST /api/auth/register          [Public]        Create account

  POST /api/auth/login             [Public]        Authenticate & set session

  POST /api/auth/logout            [Authenticated] Terminate active session



Profile & Cloud Uploads:

  GET  /api/profile/me             [Authenticated] Retrieve current profile

  PUT  /api/profile/me             [Authenticated] Update name and avatar

  POST /api/uploads/banner         [Organiser]     Upload race banner to Azure

  POST /api/uploads/avatar         [Authenticated] Upload profile photo to Azure



Events & Categories:

  GET  /api/events                 [Public]        List all active races

  GET  /api/events/{id}            [Public]        Retrieve event details

  POST /api/events                 [Organiser]     Create new event

  PUT  /api/events/{id}            [Organiser]     Update owned event

  DEL  /api/events/{id}            [Organiser]     Remove owned event

  GET  /api/events/{id}/categories [Public]        Get categories for event

  POST /api/events/{id}/categories [Organiser]     Add category to event



Enrolments & Results:

  POST /api/enrolments             [Participant]  Enrol in event category

  GET  /api/enrolments/mine        [Participant]  View personal enrolments

  GET  /api/enrolments/event/{id}  [Organiser]    View registered participants

  POST /api/results                [Organiser]    Record finish time & position

  GET  /api/results/mine           [Participant]  View personal race results



--------------------------------------------------------------------------------

5. EXECUTION & DEPLOYMENT INSTRUCTIONS

--------------------------------------------------------------------------------



A. Local Execution (Visual Studio 2022):

   1. Open RaceDay.sln in Visual Studio 2022.

   2. Configure Database Connection in RaceDay.Api/appsettings.json:

        "ConnectionStrings": {

          "DefaultConnection": "Server=localhost;Database=RaceDayDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"

        }

   3. In Package Manager Console (default project: RaceDay.Api):

        Update-Database

      (Or run /docs/RaceDay_Database.sql in SSMS).

   4. Configure Azure Blob Storage in RaceDay.Api/appsettings.json:

        "AzureBlobStorage": {

          "ConnectionString": "<your-connection-string>",

          "ContainerName": "raceday-media"

        }

   5. Configure Multiple Startup Projects (RaceDay.Api and RaceDay.Mvc).

   6. Press F5 (API: https://localhost:5000/swagger | MVC: http://localhost:5001).



B. Docker Container Execution (Part 3 Milestone):

   1. Build Docker image from solution root:

        docker build -f .\RaceDay\Dockerfile -t raceday-mvc:part3 .

   2. Start the API on Windows host bound to all network interfaces:

        dotnet run --project RaceDay.Api --urls http://0.0.0.0:5000

   3. Run the MVC container connecting via host gateway:

        docker run -d --name raceday-mvc -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5000/ raceday-mvc:part3

   4. Open http://localhost:8080 in your browser.



--------------------------------------------------------------------------------

6. AUTOMATED CI/CD & TESTING

--------------------------------------------------------------------------------

  - Local Test Execution:

      dotnet test RaceDay.sln

  - GitHub Actions (.github/workflows/ci.yml):

      Automatically triggers on every push and pull request to 'main' or 'part3'.

      Runs on ubuntu-latest, sets up .NET 8, restores packages, compiles all

      projects in Release mode, and runs automated xUnit integration tests.



--------------------------------------------------------------------------------

7. REPOSITORY DELIVERABLES INDEX

--------------------------------------------------------------------------------

  /docs/RaceDay_ERD.pdf               : Six-table relational diagram

  /docs/RaceDay_API_Endpoint_Plan.md  : REST API specification

  /docs/RaceDay_Database.sql          : T-SQL schema and South African seed data

  /RaceDay.Contracts/                 : Shared DTO POCO definitions

  /RaceDay.Api/                       : Web API, EF Core DbContext, Controllers

  /RaceDay.Api.Tests/                 : xUnit automated integration test suite

  /RaceDay/                           : ASP.NET Core MVC front-end application

  /RaceDay/Dockerfile                 : Multi-stage Linux container definition

  /.dockerignore                      : Container build exclusion rules

  /.github/workflows/ci.yml           : Continuous integration workflow



--------------------------------------------------------------------------------

8. REFERENCES & Tools

--------------------------------------------------------------------------------

  - Docker, 2024. Dockerize an ASP.NET Core application.

    https://docs.docker.com/language/dotnet/

  - GitHub, 2024. GitHub Actions: Building and testing .NET.

    https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net

  - Microsoft, 2024. Azure Blob Storage client library for .NET.

    https://learn.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet

  - Microsoft, 2024. Create Data Transfer Objects (DTOs).

    https://learn.microsoft.com/en-us/aspnet/web-api/overview/data/using-web-api-with-entity-framework/part-5

  - Microsoft, 2024. Integration tests in ASP.NET Core.

    https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

  - Microsoft, 2024. Overview of Entity Framework Core.

    https://learn.microsoft.com/en-us/ef/core/

  - Microsoft, 2024. Session and state management in ASP.NET Core.

    https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state

  - OpenAI & GitHub, 2024. GitHub Copilot (Visual Studio 2022 Integration).

    Used as an AI peer programmer for architectural suggestions, troubleshooting

    Docker multi-stage build pathing, assisting with Razor view layouts,

    refining CSS color palette classes, debugging EF Core model key constraints,

    and generating xUnit integration test scaffolding.

================================================================================