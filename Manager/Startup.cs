using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.SQLite;
using Manager.Models;
using Manager.Services;
using Manager.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Manager;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MirrorContext>(opt => opt.UseSqlite(Utils.Constants.MirrorSqliteConnectionString));
        services.AddAutoMapper(typeof(MapperProfile));
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
            });
        services.AddSwaggerGen(c => { c.SwaggerDoc(Constants.ApiVersion, new OpenApiInfo { Title = "Manager", Version = Constants.ApiVersion }); });
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage(Constants.HangFireSqliteConnectionString));
        services.AddHangfireServer(configuration =>
        {
            configuration.WorkerCount = 1;
            configuration.ServerName = Constants.HangFireServerName;
            configuration.HeartbeatInterval = TimeSpan.FromMinutes(1);
        });
        services.AddIndexService();
        services.AddConfigService();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{Constants.ApiVersion}/swagger.json", $"Mirrors.NET API {Constants.ApiVersion}"));;
        }

        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
        {
            if (serviceScope != null)
            {
                var configService = serviceScope.ServiceProvider.GetRequiredService<IConfigService>();
                var mirrorContext = serviceScope.ServiceProvider.GetRequiredService<MirrorContext>();

                mirrorContext.Database.EnsureCreated();

                var task = configService.LoadConfigAsync();
                task.Wait();
            }
        }

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new [] { new HangFireAuthorizationFilter() }
        });

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHangfireDashboard();
        });
    }
}
