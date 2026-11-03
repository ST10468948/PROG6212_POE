using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class AppUser
    {
        [Key]
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? ProfilePicUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        
        public Role Role { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<Enrolment> Enrolments { get; set; }
        public ICollection<Result> Results { get; set; }
    }
}