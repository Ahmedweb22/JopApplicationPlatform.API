using System.Threading.Tasks;
using JopApplicationPlatform.Application.DTOs.Requestes;
using JopApplicationPlatform.Application.DTOs.Responses;

namespace JopApplicationPlatform.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
