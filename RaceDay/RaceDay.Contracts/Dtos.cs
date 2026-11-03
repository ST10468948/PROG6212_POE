using System;

namespace RaceDay.Contracts
{
    // Authentication
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Role { get; set; }

    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    // User Profile
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ProfilePicUrl { get; set; }

    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; }
        public string ProfilePicUrl { get; set; }
    }

    // Event

    public class CreateEventRequest
    {
        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } 
        public string? BannerUrl { get; set; }
    }

    public class UpdateEventRequest
    {
        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; }
        public string? BannerUrl { get; set; }
    }

    public class EventDto
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; }
        public string BannerUrl { get; set; }
    }

    
    // Category

    public class CreateCategoryRequest
    {
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }

    
    // Enrolment DTOs
    
    public class CreateEnrolmentRequest
    {
        public int EventID { get; set; }
        public int CategoryID { get; set; }
    }

    public class EnrolmentDto
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; }
    }

    
    // 6. Result DTOs
    
    public class CreateResultRequest
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string FinishTime { get; set; }
        public int Position { get; set; }
        public int TotalFinishers { get; set; }
    }

    public class ResultDto
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; }
        public string EventName { get; set; }
        public string CategoryName { get; set; }
        public string FinishTime { get; set; }
        public int Position { get; set; }
        public int TotalFinishers { get; set; }
        public DateTime RecordedDate { get; set; }
    }
}
