using JopApplicationPlatform.Application.DTOs.Requestes;
using JopApplicationPlatform.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using JopApplicationPlatform.Domain.Constants;

namespace JopApplicationPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpGet("my")]
        [Authorize(Roles = StaticRoles.Candidate)]
        public async Task<IActionResult> GetMyApplications()
        {
            var candidateIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(candidateIdClaim, out int candidateId))
            {
                return Unauthorized("Candidate ID not found in token.");
            }

            var applications = await _applicationService.GetMyApplicationsAsync(candidateId);
            return Ok(applications);
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = StaticRoles.Candidate)]
        public async Task<IActionResult> CancelApplication(int id)
        {
            var candidateIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(candidateIdClaim, out int candidateId))
            {
                return Unauthorized("Candidate ID not found in token.");
            }

            try
            {
                await _applicationService.CancelApplicationAsync(id, candidateId);
                return Ok(new { Message = "Application cancelled successfully." });
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
