using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<AuthResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
