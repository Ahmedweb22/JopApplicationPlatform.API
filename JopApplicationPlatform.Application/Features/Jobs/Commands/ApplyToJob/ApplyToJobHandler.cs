using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Domain.Entities;
using JopApplicationPlatform.Domain.Enums;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Commands.ApplyToJob
{
    public class ApplyToJobHandler : IRequestHandler<ApplyToJobCommand, int>
    {
        private readonly IRepository<JobApplication> _applicationRepository;
        private readonly IRepository<Job> _jobRepository;

        public ApplyToJobHandler(IRepository<JobApplication> applicationRepository, IRepository<Job> jobRepository)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
        }

        public async Task<int> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == request.JobId);
            if (job == null) throw new Exception("Job not found.");
            if (!job.IsActive) throw new Exception("Cannot apply to an inactive job.");

            var existingApplication = await _applicationRepository.GetOneAsync(a => a.CandidateId == request.CandidateId && a.JobId == request.JobId);
            if (existingApplication != null) throw new Exception("You have already applied to this job.");

            var application = new JobApplication
            {
                CandidateId = request.CandidateId,
                JobId = request.JobId,
                Status = JobApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                StatusUpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.CreateAsync(application);
            await _applicationRepository.CommitAsync();

            return application.Id;
        }
    }
}
