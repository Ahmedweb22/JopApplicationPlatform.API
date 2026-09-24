using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JopApplicationPlatform.Application.Features.Auth.Commands.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly string _jwtSecret;

        public RegisterHandler(
            IRepository<User> userRepository,
            IRepository<Candidate> candidateRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _jwtSecret = configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JwtSettings:Key is missing.");
        }

        public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetOneAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role
            };

            await _userRepository.CreateAsync(user);
            await _userRepository.CommitAsync();

            if (string.Equals(request.Role, Domain.Constants.StaticRoles.Candidate, StringComparison.OrdinalIgnoreCase))
            {
                var candidate = new Candidate
                {
                    Name = request.Email.Contains('@') ? request.Email.Split('@')[0] : request.Email,
                    Email = request.Email,
                    CVUrl = string.Empty
                };
                await _candidateRepository.CreateAsync(candidate);
                await _candidateRepository.CommitAsync();
            }

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                Message = "Registration successful."
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
