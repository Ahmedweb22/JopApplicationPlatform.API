using System.Threading.Tasks;

namespace JopApplicationPlatform.Application.Interfaces.IServices
{
    public interface IJobBackgroundService
    {
        /// <summary>
        /// Recurring Job: Automatically closes active jobs that have passed the expiration threshold (e.g. 30 days open).
        /// Also updates pending applications for closed jobs.
        /// </summary>
        Task AutoCloseExpiredJobsAsync();

        /// <summary>
        /// Fire-and-Forget Job: Sends an email/system notification when a candidate applies to a job.
        /// </summary>
        Task SendApplicationSubmittedNotificationAsync(int applicationId);

        /// <summary>
        /// Delayed Job: Sends a reminder to the recruiter to review an application after a specified delay.
        /// </summary>
        Task SendApplicationReviewReminderAsync(int applicationId);
    }
}
