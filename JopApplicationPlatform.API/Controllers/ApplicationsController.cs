using JopApplicationPlatform.Application.DTOs.Requestes;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using JopApplicationPlatform.Domain.Constants;
using MediatR;
using JopApplicationPlatform.Application.Features.Applications.Queries.GetMyApplications;
using JopApplicationPlatform.Application.Features.Applications.Commands.CancelApplication;

namespace JopApplicationPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApplicationsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get the applications of the currently authenticated candidate.
        /// </summary>
        /// <returns>A list of applications belonging to the authenticated candidate.</returns>

        [HttpGet("my")]
        [Authorize(Roles = StaticRoles.Candidate)]
        public async Task<IActionResult> GetMyApplications()
        {
            var candidateIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(candidateIdClaim, out int candidateId))
            {
                return Unauthorized("Candidate ID not found in token.");
            }

            var applications = await _mediator.Send(new GetMyApplicationsQuery { CandidateId = candidateId });
            return Ok(applications);
        }
        /// <summary>
        /// Cancel an application for the currently authenticated candidate.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

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
                await _mediator.Send(new CancelApplicationCommand { ApplicationId = id, CandidateId = candidateId });
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
