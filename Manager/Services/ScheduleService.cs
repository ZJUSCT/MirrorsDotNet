using System;
using System.Threading.Tasks;
using Manager.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Manager.Services;

public class ScheduleService
{
    private readonly IServiceProvider _serviceProvider;
    public ScheduleService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Schedule(string id)
    {
        using var scope = _serviceProvider.CreateScope();
        await using var ctx = scope.ServiceProvider.GetRequiredService<MirrorContext>();
        var mirrorItem = await ctx.Mirrors.FindAsync(id);

        if (mirrorItem == null) return;
        Console.WriteLine($"{mirrorItem.Id} is scheduled. Upstream: {mirrorItem.Upstream}");
    }
}
