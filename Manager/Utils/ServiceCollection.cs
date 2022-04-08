using Manager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Manager.Utils;

public static class ServiceCollection
{
    public static IServiceCollection AddZjuService(this IServiceCollection services)
    {
        return services.AddTransient<IIndexService, IndexService>();
    }
}