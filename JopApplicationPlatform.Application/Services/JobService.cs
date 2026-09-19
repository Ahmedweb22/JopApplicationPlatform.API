using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Application.Interfaces.IServices;
using JopApplicationPlatform.Domain.Entities;

namespace JopApplicationPlatform.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IRepository<Job> _jobRepository;
        public JobService(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }
        public async Task<int> CreateAsync(CreateJobDto createJobDto)
        {
         var job = new Job
            {
                Title = createJobDto.Title,
                Description = createJobDto.Description,
                IsActive = createJobDto.IsActive
            };
            await _jobRepository.CreateAsync(job);
            await _jobRepository.CommitAsync();
            return job.Id;

        }
    }
}
