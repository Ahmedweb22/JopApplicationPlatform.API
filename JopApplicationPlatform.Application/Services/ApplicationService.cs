using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.DTOs.Requestes;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Application.Interfaces.IServices;
using JopApplicationPlatform.Domain.Entities;
using JopApplicationPlatform.Domain.Enums;

namespace JopApplicationPlatform.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IRepository<JobApplication> _applicationRepository;
        private readonly IRepository<Job> _jobRepository;

        public ApplicationService(IRepository<JobApplication> applicationRepository, IRepository<Job> jobRepository)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
        }

        public async Task<int> ApplyAsync(int candidateId, ApplyForJobDto applyDto)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == applyDto.JobId);
            if (job == null) throw new Exception("Job not found.");
            if (!job.IsActive) throw new Exception("Cannot apply to an inactive job.");

            var existingApplication = await _applicationRepository.GetOneAsync(a => a.CandidateId == candidateId && a.JobId == applyDto.JobId);
            if (existingApplication != null) throw new Exception("You have already applied to this job.");

            var application = new JobApplication
            {
                CandidateId = candidateId,
                JobId = applyDto.JobId,
                Status = JobApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                StatusUpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.CreateAsync(application);
            await _applicationRepository.CommitAsync();

            return application.Id;
        }

        public async Task<List<JobApplicationDto>> GetMyApplicationsAsync(int candidateId)
        {
            var applications = await _applicationRepository.GetAsync(a => a.CandidateId == candidateId);
            return applications.Select(a => new JobApplicationDto
            {
                Id = a.Id,
                CandidateId = a.CandidateId,
                JobId = a.JobId,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                StatusUpdatedAt = a.StatusUpdatedAt
            }).ToList();
        }

        public async Task CancelApplicationAsync(int applicationId, int candidateId)
        {
            var application = await _applicationRepository.GetOneAsync(a => a.Id == applicationId);
            if (application == null) throw new Exception("Application not found.");
            if (application.CandidateId != candidateId) throw new UnauthorizedAccessException("Only the owning candidate can cancel this application.");
            
            application.Status = JobApplicationStatus.Cancelled;
            application.StatusUpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(application);
            await _applicationRepository.CommitAsync();
        }
    }
}
