using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using JopApplicationPlatform.Domain.Constants;
using JopApplicationPlatform.Application.DTOs.Requestes;

namespace JopApplicationPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;
        
        public JobsController(IJobService jobService, IApplicationService applicationService)
        {
            _jobService = jobService;
            _applicationService = applicationService;
        }
        
        [HttpPost]
        [Authorize(Roles = StaticRoles.Recruiter)]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var jobId = await _jobService.CreateAsync(createJobDto);
            return Ok(new { Id = jobId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableJobs()
        {
            var jobs = await _jobService.GetAvailableJobsAsync();
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobById(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
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
                var applications = await _jobService.GetApplicantsForJobAsync(jobId, recruiterId);
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
                await _jobService.CloseJobAsync(id, recruiterId);
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
                var applyDto = new ApplyForJobDto { JobId = id };
                var applicationId = await _applicationService.ApplyAsync(candidateId, applyDto);
                return Ok(new { Id = applicationId });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
