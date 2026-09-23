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
        /// <summary>
        /// Creates a new job posting. Only users with the "Recruiter" role can access this endpoint.
        /// </summary>
        /// <param name="createJobDto"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Retrieves a list of available job postings. This endpoint is accessible to all users, including unauthenticated users.
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableJobs()
        {
            var jobs = await _mediator.Send(new GetAvailableJobsQuery());
            return Ok(jobs);
        }
        /// <summary>
        /// Retrieves the details of a specific job posting by its ID. This endpoint is accessible to all users, including unauthenticated users.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobById(int id)
        {
            var job = await _mediator.Send(new GetJobByIdQuery { Id = id });
            if (job == null) return NotFound();
            return Ok(job);
        }
        /// <summary>
        /// Retrieves a list of applicants for a specific job posting. Only users with the "Recruiter" role can access this endpoint, and they can only view applicants for jobs they have posted.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>

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
        /// <summary>
        /// Cancels a job posting, effectively closing it. Only users with the "Recruiter" role can access this endpoint, and they can only cancel jobs they have posted.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Allows a candidate to apply for a specific job posting. Only users with the "Candidate" role can access this endpoint.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
