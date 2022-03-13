using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Manager.Models;
using Manager.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        services.AddDbContext<MirrorContext>(opt => opt.UseSqlite(Utils.Constants.SqliteConnectionString));
        services.AddAutoMapper(typeof(MapperProfile));
        services.AddDistributedMemoryCache();
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
        services.AddSwaggerGen(c => { c.SwaggerDoc(Constants.ApiVersion, new OpenApiInfo { Title = "Manager", Version = Constants.ApiVersion }); });
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
                var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
                var mirrorContext = serviceScope.ServiceProvider.GetRequiredService<MirrorContext>();
                var mapper = serviceScope.ServiceProvider.GetRequiredService<IMapper>();

                mirrorContext.Database.EnsureCreated();

                var task = ConfigLoader.LoadConfigAsync(mirrorContext, mapper, logger);
                task.Wait();
            }
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}