using System.ComponentModel.DataAnnotations;

namespace JopApplicationPlatform.Application.DTOs.Requestes
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
