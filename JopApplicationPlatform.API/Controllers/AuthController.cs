using System;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.DTOs.Requestes;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using JopApplicationPlatform.Application.Features.Auth.Commands.Register;
using JopApplicationPlatform.Application.Features.Auth.Commands.Login;

namespace JopApplicationPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                var result = await _mediator.Send(new RegisterCommand
                {
                    Email = dto.Email,
                    Password = dto.Password,
                    Role = dto.Role
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var result = await _mediator.Send(new LoginCommand
                {
                    Email = dto.Email,
                    Password = dto.Password
                });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
