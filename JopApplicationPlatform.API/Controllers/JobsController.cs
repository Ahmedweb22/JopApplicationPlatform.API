using JopApplicationPlatform.Application.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using JopApplicationPlatform.Domain.Constants;
using JopApplicationPlatform.Application.DTOs.Requestes;
using MediatR;
using JopApplicationPlatform.Application.Features.Jobs.Commands.CreateJob;
using JopApplicationPlatform.Application.Features.Jobs.Queries.GetAvailableJobs;
using JopApplicationPlatform.Application.Features.Jobs.Queries.GetJobById;
using JopApplicationPlatform.Application.Features.Jobs.Queries.GetApplicantsForJob;
using JopApplicationPlatform.Application.Features.Jobs.Commands.CancelJob;
using JopApplicationPlatform.Application.Features.Jobs.Commands.ApplyToJob;

namespace JopApplicationPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost]
        [Authorize(Roles = StaticRoles.Recruiter)]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var jobId = await _mediator.Send(new CreateJobCommand
            {
                Title = createJobDto.Title,
                Description = createJobDto.Description,
                IsActive = createJobDto.IsActive
            });
            return Ok(new { Id = jobId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableJobs()
        {
            var jobs = await _mediator.Send(new GetAvailableJobsQuery());
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobById(int id)
        {
            var job = await _mediator.Send(new GetJobByIdQuery { Id = id });
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpGet("{jobId}/applications")]
        [Authorize(Roles = StaticRoles.Recruiter)]
        public async Task<IActionResult> GetApplicantsForJob(int jobId)
        {
            // Assuming RecruiterId is stored in NameIdentifier claim
            var recruiterIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(recruiterIdClaim, out int recruiterId))
            {
                return Unauthorized("Recruiter ID not found in token.");
            }

            try
            {
                var applications = await _mediator.Send(new GetApplicantsForJobQuery { JobId = jobId, RecruiterId = recruiterId });
                return Ok(applications);
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = StaticRoles.Recruiter)]
        public async Task<IActionResult> CancelJob(int id)
        {
            var recruiterIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(recruiterIdClaim, out int recruiterId))
            {
                return Unauthorized("Recruiter ID not found in token.");
            }

            try
            {
                await _mediator.Send(new CancelJobCommand { JobId = id, RecruiterId = recruiterId });
                return Ok(new { Message = "Job closed successfully." });
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

        [HttpPost("{id}/apply")]
        [Authorize(Roles = StaticRoles.Candidate)]
        public async Task<IActionResult> ApplyToJob(int id)
        {
            var candidateIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(candidateIdClaim, out int candidateId))
            {
                return Unauthorized("Candidate ID not found in token.");
            }

            try
            {
                var applicationId = await _mediator.Send(new ApplyToJobCommand { JobId = id, CandidateId = candidateId });
                return Ok(new { Id = applicationId });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
