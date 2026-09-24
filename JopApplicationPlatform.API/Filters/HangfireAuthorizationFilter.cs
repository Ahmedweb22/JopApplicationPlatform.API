using Hangfire.Dashboard;

namespace JopApplicationPlatform.API.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // Allow unrestricted dashboard access for development / demonstration purposes.
            // In a strict production environment, this would verify admin claims or basic auth.
            return true;
        }
    }
}
