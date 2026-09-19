using System;
using System.Collections.Generic;
using System.Text;

namespace JopApplicationPlatform.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Recruiter" or "Candidate"
    }
}
