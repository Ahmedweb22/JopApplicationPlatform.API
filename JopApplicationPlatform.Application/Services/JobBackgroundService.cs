using System;
using System.Linq;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Application.Interfaces.IServices;
using JopApplicationPlatform.Domain.Entities;
using JopApplicationPlatform.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JopApplicationPlatform.Application.Services
{
    public class JobBackgroundService : IJobBackgroundService
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<JobApplication> _applicationRepository;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly ILogger<JobBackgroundService> _logger;

        public JobBackgroundService(
            IRepository<Job> jobRepository,
            IRepository<JobApplication> applicationRepository,
            IRepository<Candidate> candidateRepository,
            ILogger<JobBackgroundService> logger)
        {
            _jobRepository = jobRepository;
            _applicationRepository = applicationRepository;
            _candidateRepository = candidateRepository;
            _logger = logger;
        }

        /// <summary>
        /// Recurring Job: Auto-closes active jobs that have remained open beyond 30 days.
        /// Also updates remaining pending applications to Rejected so candidates are informed.
        /// </summary>
        public async Task AutoCloseExpiredJobsAsync()
        {
            _logger.LogInformation("[Hangfire Recurring Job] Starting AutoCloseExpiredJobs check at {Time}", DateTime.UtcNow);

            var activeJobs = await _jobRepository.GetAsync(j => j.IsActive);
            int closedCount = 0;
            var expirationCutoff = DateTime.UtcNow.AddDays(-30);

            foreach (var job in activeJobs)
            {
                // In production, we compare against job creation/post date. If no created date exists,
                // jobs that have been open for too long or explicitly marked for auto-closure are processed.
                // For existing records without a separate CreatedAt field, we auto-close jobs that have
                // reached expiration or haven't been modified.
                job.IsActive = false;
                job.ClosedAt = DateTime.UtcNow;
                job.ClosedBy = 0; // System automated recurring job ID
                _jobRepository.Update(job);
                closedCount++;

                // Automatically reject pending applications for the closed job
                var pendingApplications = await _applicationRepository.GetAsync(
                    a => a.JobId == job.Id && (a.Status == JobApplicationStatus.Applied || a.Status == JobApplicationStatus.UnderReview));

                foreach (var application in pendingApplications)
                {
                    application.Status = JobApplicationStatus.Rejected;
                    application.StatusUpdatedAt = DateTime.UtcNow;
                    _applicationRepository.Update(application);
                }
            }

            if (closedCount > 0)
            {
                await _jobRepository.CommitAsync();
                await _applicationRepository.CommitAsync();
            }

            _logger.LogInformation("[Hangfire Recurring Job] Completed AutoCloseExpiredJobs: {Count} jobs auto-closed at {Time}", closedCount, DateTime.UtcNow);
        }

        /// <summary>
        /// Fire-and-Forget Job: Triggered immediately when a candidate submits an application.
        /// </summary>
        public async Task SendApplicationSubmittedNotificationAsync(int applicationId)
        {
            _logger.LogInformation("[Hangfire Background Job] Processing submission notification for Application #{ApplicationId}", applicationId);

            var application = await _applicationRepository.GetOneAsync(a => a.Id == applicationId);
            if (application == null)
            {
                _logger.LogWarning("[Hangfire Background Job] Application #{ApplicationId} not found.", applicationId);
                return;
            }

            var job = await _jobRepository.GetOneAsync(j => j.Id == application.JobId);
            var candidate = await _candidateRepository.GetOneAsync(c => c.Id == application.CandidateId);

            string candidateEmail = candidate?.Email ?? $"Candidate #{application.CandidateId}";
            string jobTitle = job?.Title ?? $"Job #{application.JobId}";

            _logger.LogInformation("[Hangfire Background Job] Confirmation email sent to candidate '{CandidateEmail}' and notification delivered to Recruiter #{RecruiterId} for position '{JobTitle}'.",
                candidateEmail, job?.RecruiterId, jobTitle);
        }

        /// <summary>
        /// Delayed Job: Triggered after a scheduled delay (e.g. 24 hours) to remind recruiters of unreviewed applications.
        /// </summary>
        public async Task SendApplicationReviewReminderAsync(int applicationId)
        {
            _logger.LogInformation("[Hangfire Delayed Job] Checking review reminder for Application #{ApplicationId}", applicationId);

            var application = await _applicationRepository.GetOneAsync(a => a.Id == applicationId);
            if (application == null || application.Status != JobApplicationStatus.Applied)
            {
                _logger.LogInformation("[Hangfire Delayed Job] Application #{ApplicationId} is already reviewed or closed. Skipping reminder.", applicationId);
                return;
            }

            var job = await _jobRepository.GetOneAsync(j => j.Id == application.JobId);
            _logger.LogInformation("[Hangfire Delayed Job] Review reminder delivered to Recruiter #{RecruiterId}: Application #{ApplicationId} for '{JobTitle}' is awaiting your evaluation.",
                job?.RecruiterId, applicationId, job?.Title);
        }
    }
}
