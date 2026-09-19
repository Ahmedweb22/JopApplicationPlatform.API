using System.ComponentModel.DataAnnotations;

namespace JopApplicationPlatform.Application.DTOs.Requestes
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // "Recruiter" or "Candidate"
    }
}
