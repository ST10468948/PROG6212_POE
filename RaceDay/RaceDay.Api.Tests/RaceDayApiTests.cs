using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class RaceDayApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        // Static in-memory database name ensures all tests in this fixture hit the same data store
        private static readonly string SharedDatabaseName = "RaceDayTestDb_" + Guid.NewGuid().ToString();

        public RaceDayApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // 1. Remove existing SQL Server DbContext registration
                    ServiceDescriptor descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // 2. Register shared in-memory database
                    services.AddDbContext<RaceDayDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(SharedDatabaseName);
                    });

                    // 3. Ensure database schema is created and default roles are seeded
                    ServiceProvider sp = services.BuildServiceProvider();
                    using (IServiceScope scope = sp.CreateScope())
                    {
                        RaceDayDbContext db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
                        db.Database.EnsureCreated();

                        if (!db.Roles.Any(r => r.RoleName == "Organiser"))
                        {
                            db.Roles.Add(new Role { RoleID = 1, RoleName = "Organiser" });
                        }

                        if (!db.Roles.Any(r => r.RoleName == "Participant"))
                        {
                            db.Roles.Add(new Role { RoleID = 2, RoleName = "Participant" });
                        }

                        db.SaveChanges();
                    }
                });
            });
        }

        // Test 1: User Registration Success
        [Fact]
        public async Task Register_ValidParticipant_Returns201Created()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            RegisterRequest request = new RegisterRequest
            {
                FullName = "Kagiso Rabada",
                Email = "kagiso_" + Guid.NewGuid().ToString() + "@raceday.co.za",
                Password = "Password123!",
                Role = "Participant"
            };

            // Act
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Test 2: Login Failure on Incorrect Credentials
        [Fact]
        public async Task Login_InvalidCredentials_Returns401Unauthorized()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            LoginRequest request = new LoginRequest
            {
                Email = "nonexistent.user@raceday.co.za",
                Password = "WrongPassword999!"
            };

            // Act
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Test 3: Unauthenticated Access to Protected Route
        [Fact]
        public async Task CreateEvent_UnauthenticatedUser_Returns401Unauthorized()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            CreateEventRequest request = new CreateEventRequest
            {
                EventName = "Comrades Marathon",
                Description = "Iconic ultra marathon",
                EventDate = DateTime.UtcNow.AddDays(60),
                Location = "Durban",
                DistanceKm = 89.0m,
                EventType = "Run",
                BannerUrl = string.Empty
            };

            // Act
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Test 4: Role-Based Rejection - Participant cannot create Events
        [Fact]
        public async Task CreateEvent_WhenUserIsParticipant_Returns403Forbidden()
        {
            // Arrange
            WebApplicationFactoryClientOptions options = new WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            };
            HttpClient client = _factory.CreateClient(options);

            string participantEmail = "runner_" + Guid.NewGuid().ToString() + "@raceday.co.za";

            // 1. Register as Participant
            RegisterRequest reg = new RegisterRequest
            {
                FullName = "Sipho Runner",
                Email = participantEmail,
                Password = "Password123!",
                Role = "Participant"
            };
            HttpResponseMessage regRes = await client.PostAsJsonAsync("/api/auth/register", reg);
            Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);

            // 2. Login to initialize session cookie
            LoginRequest login = new LoginRequest
            {
                Email = participantEmail,
                Password = "Password123!"
            };
            HttpResponseMessage loginRes = await client.PostAsJsonAsync("/api/auth/login", login);
            Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);

            // 3. Act: Attempt to create an event with Participant credentials
            CreateEventRequest eventRequest = new CreateEventRequest
            {
                EventName = "Trail Challenge",
                Description = "Fun trail run",
                EventDate = DateTime.UtcNow.AddDays(14),
                Location = "Polokwane",
                DistanceKm = 10.0m,
                EventType = "Run",
                BannerUrl = string.Empty
            };
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", eventRequest);

            // Assert: Must be rejected with 403 Forbidden
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Test 5: Role-Based Success - Organiser can create Events (201 Created)
        [Fact]
        public async Task CreateEvent_WhenUserIsOrganiser_Returns201Created()
        {
            // Arrange
            WebApplicationFactoryClientOptions options = new WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            };
            HttpClient client = _factory.CreateClient(options);

            string organiserEmail = "organiser_" + Guid.NewGuid().ToString() + "@raceday.co.za";

            // 1. Register as Organiser
            RegisterRequest reg = new RegisterRequest
            {
                FullName = "Ansie Organiser",
                Email = organiserEmail,
                Password = "Password123!",
                Role = "Organiser"
            };
            HttpResponseMessage regRes = await client.PostAsJsonAsync("/api/auth/register", reg);
            string regContent = await regRes.Content.ReadAsStringAsync();
            Assert.True(regRes.IsSuccessStatusCode, $"Organiser registration failed ({regRes.StatusCode}): {regContent}");

            // 2. Login to establish session
            LoginRequest login = new LoginRequest
            {
                Email = organiserEmail,
                Password = "Password123!"
            };
            HttpResponseMessage loginRes = await client.PostAsJsonAsync("/api/auth/login", login);
            string loginContent = await loginRes.Content.ReadAsStringAsync();
            Assert.True(loginRes.IsSuccessStatusCode, $"Organiser login failed ({loginRes.StatusCode}): {loginContent}");

            // 3. Act: Post the event
            CreateEventRequest eventRequest = new CreateEventRequest
            {
                EventName = "Cape Town 10k",
                Description = "Coastal road race",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                DistanceKm = 10.0m,
                EventType = "Run",
                BannerUrl = string.Empty
            };
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", eventRequest);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert: Must return 201 Created
            Assert.True(response.StatusCode == HttpStatusCode.Created, $"Expected 201 Created but received {response.StatusCode}. Details: {responseContent}");
        }

        // Test 6: Participant Enrolment Success (201 Created)
        [Fact]
        public async Task EnrolInEvent_ValidParticipant_Returns201Created()
        {
            // 1. Setup Organiser to create Event and Category
            WebApplicationFactoryClientOptions orgOptions = new WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            };
            HttpClient orgClient = _factory.CreateClient(orgOptions);

            string orgEmail = "org_" + Guid.NewGuid().ToString() + "@raceday.co.za";
            await orgClient.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                FullName = "Race Boss",
                Email = orgEmail,
                Password = "Password123!",
                Role = "Organiser"
            });
            await orgClient.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = orgEmail,
                Password = "Password123!"
            });

            HttpResponseMessage eventRes = await orgClient.PostAsJsonAsync("/api/events", new CreateEventRequest
            {
                EventName = "Soweto 10k",
                Description = "Annual Soweto Race",
                EventDate = DateTime.UtcNow.AddDays(45),
                Location = "Soweto",
                DistanceKm = 10.0m,
                EventType = "Run",
                BannerUrl = string.Empty
            });
            EventDto createdEvent = await eventRes.Content.ReadFromJsonAsync<EventDto>();

            HttpResponseMessage catRes = await orgClient.PostAsJsonAsync($"/api/events/{createdEvent.EventID}/categories", new CreateCategoryRequest
            {
                CategoryName = "Open 10km",
                Description = "All ages"
            });
            CategoryDto createdCategory = await catRes.Content.ReadFromJsonAsync<CategoryDto>();

            // 2. Setup Participant to enrol
            WebApplicationFactoryClientOptions partOptions = new WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            };
            HttpClient partClient = _factory.CreateClient(partOptions);

            string partEmail = "runner_" + Guid.NewGuid().ToString() + "@raceday.co.za";
            await partClient.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                FullName = "Themba Athlete",
                Email = partEmail,
                Password = "Password123!",
                Role = "Participant"
            });
            await partClient.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = partEmail,
                Password = "Password123!"
            });

            // 3. Act: Enrol into the created event
            CreateEnrolmentRequest enrolReq = new CreateEnrolmentRequest
            {
                EventID = createdEvent.EventID,
                CategoryID = createdCategory.CategoryID
            };
            HttpResponseMessage enrolRes = await partClient.PostAsJsonAsync("/api/enrolments", enrolReq);

            // Assert
            Assert.Equal(HttpStatusCode.Created, enrolRes.StatusCode);
        }
    }
}