using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Application.Interfaces.IServices;
using JopApplicationPlatform.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace JopApplicationPlatform.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<JobApplication> _applicationRepository;
        public JobService(IRepository<Job> jobRepository, IRepository<JobApplication> applicationRepository)
        {
            _jobRepository = jobRepository;
            _applicationRepository = applicationRepository;
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

        public async Task<IEnumerable<JobDto>> GetAvailableJobsAsync()
        {
            var jobs = await _jobRepository.GetAsync(j => j.IsActive);
            return jobs.Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                IsActive = j.IsActive,
                RecruiterId = j.RecruiterId
            });
        }

        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == id);
            if (job == null) return null;

            return new JobDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                IsActive = job.IsActive,
                RecruiterId = job.RecruiterId
            };
        }

        public async Task<IEnumerable<JobApplicationDto>> GetApplicantsForJobAsync(int jobId, int recruiterId)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == jobId);
            if (job == null) throw new Exception("Job not found.");
            if (job.RecruiterId != recruiterId) throw new UnauthorizedAccessException("Only the owning recruiter can view applicants.");

            var applications = await _applicationRepository.GetAsync(a => a.JobId == jobId);
            return applications.Select(a => new JobApplicationDto
            {
                Id = a.Id,
                CandidateId = a.CandidateId,
                JobId = a.JobId,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                StatusUpdatedAt = a.StatusUpdatedAt
            });
        }

        public async Task CloseJobAsync(int jobId, int recruiterId)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == jobId);
            if (job == null) throw new Exception("Job not found.");
            if (job.RecruiterId != recruiterId) throw new UnauthorizedAccessException("Only the owning recruiter can close the job.");

            job.IsActive = false;
            job.ClosedAt = DateTime.UtcNow;
            job.ClosedBy = recruiterId;

            _jobRepository.Update(job);
            await _jobRepository.CommitAsync();
        }
    }
}
