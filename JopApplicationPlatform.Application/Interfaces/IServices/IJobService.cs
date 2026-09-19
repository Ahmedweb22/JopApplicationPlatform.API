using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;

namespace JopApplicationPlatform.Application.Interfaces.IServices
{
    public interface IJobService
    {
        Task<int> CreateAsync(CreateJobDto createJobDto);
        Task<List<JobDto>> GetAvailableJobsAsync();
        Task<JobDto> GetJobByIdAsync(int id);
        Task<List<JobApplicationDto>> GetApplicantsForJobAsync(int jobId, int recruiterId);
        Task CloseJobAsync(int jobId, int recruiterId);
    }
}
