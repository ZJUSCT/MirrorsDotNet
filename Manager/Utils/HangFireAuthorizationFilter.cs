using Hangfire.Dashboard;

namespace Manager.Utils;

public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
