using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JopApplicationPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var jobId = await _jobService.CreateAsync(createJobDto);
            return Ok(new { Id = jobId });
        }
    }
}
