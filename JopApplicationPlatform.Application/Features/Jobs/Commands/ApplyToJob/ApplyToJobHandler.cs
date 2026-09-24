using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Domain.Entities;
using JopApplicationPlatform.Domain.Enums;
using MediatR;

using Hangfire;
using JopApplicationPlatform.Application.Interfaces.IServices;

namespace JopApplicationPlatform.Application.Features.Jobs.Commands.ApplyToJob
{
    public class ApplyToJobHandler : IRequestHandler<ApplyToJobCommand, int>
    {
        private readonly IRepository<JobApplication> _applicationRepository;
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public ApplyToJobHandler(
            IRepository<JobApplication> applicationRepository,
            IRepository<Job> jobRepository,
            IRepository<Candidate> candidateRepository,
            IRepository<User> userRepository,
            IBackgroundJobClient backgroundJobClient)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<int> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == request.JobId);
            if (job == null) throw new Exception("Job not found.");
            if (!job.IsActive) throw new Exception("Cannot apply to an inactive job.");

            // Resolve or link Candidate entity for foreign key integrity
            var user = await _userRepository.GetOneAsync(u => u.Id == request.CandidateId);
            var candidate = await _candidateRepository.GetOneAsync(c => c.Id == request.CandidateId || (user != null && c.Email == user.Email));
            if (candidate == null)
            {
                candidate = new Candidate
                {
                    Name = user?.Email.Contains('@') == true ? user.Email.Split('@')[0] : $"Candidate_{request.CandidateId}",
                    Email = user?.Email ?? $"candidate{request.CandidateId}@example.com",
                    CVUrl = string.Empty
                };
                await _candidateRepository.CreateAsync(candidate);
                await _candidateRepository.CommitAsync();
            }

            int actualCandidateId = candidate.Id;

            var existingApplication = await _applicationRepository.GetOneAsync(a => a.CandidateId == actualCandidateId && a.JobId == request.JobId);
            if (existingApplication != null) throw new Exception("You have already applied to this job.");

            var application = new JobApplication
            {
                CandidateId = actualCandidateId,
                JobId = request.JobId,
                Status = JobApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                StatusUpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.CreateAsync(application);
            await _applicationRepository.CommitAsync();

            // 1. Fire-and-Forget Job: Send confirmation notification to candidate & alert to recruiter
            _backgroundJobClient.Enqueue<IJobBackgroundService>(service =>
                service.SendApplicationSubmittedNotificationAsync(application.Id));

            // 2. Delayed Job: Schedule review reminder after 24 hours
            _backgroundJobClient.Schedule<IJobBackgroundService>(service =>
                service.SendApplicationReviewReminderAsync(application.Id),
                TimeSpan.FromHours(24));

            return application.Id;
        }
    }
}
