using Manager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Manager.Utils;

public static class ServiceCollection
{
    public static IServiceCollection AddIndexService(this IServiceCollection services)
    {
        return services.AddTransient<IIndexService, IndexService>();
    }

    public static IServiceCollection AddConfigService(this IServiceCollection services)
    {
        return services.AddTransient<IConfigService, ConfigService>();
    }
}