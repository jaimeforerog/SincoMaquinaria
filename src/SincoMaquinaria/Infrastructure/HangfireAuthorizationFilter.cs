using Hangfire.Dashboard;

namespace SincoMaquinaria.Infrastructure;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Verificar que el usuario esté autenticado y sea Admin
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Admin");
    }
}
