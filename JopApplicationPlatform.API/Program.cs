
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Application.Interfaces.IServices;
using JopApplicationPlatform.Application.Services;
using JopApplicationPlatform.Domain.Entities;
using JopApplicationPlatform.Infrastructure.Persistence;
using JopApplicationPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.SqlServer;
using System.Text;
using JopApplicationPlatform.Application.Features.Jobs.Commands.CreateJob;

namespace JopApplicationPlatform.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Generic repository registration for all entity types
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Application & background services
            builder.Services.AddScoped<IJobService, JobService>();
            builder.Services.AddScoped<IApplicationService, ApplicationService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJobBackgroundService, JobBackgroundService>();

            // Hangfire background processing configuration
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            builder.Services.AddHangfireServer();

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateJobCommand).Assembly);
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JwtSettings:Key is missing."))),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // OpenAPI & API documentation
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            // Hangfire Dashboard configuration
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new JopApplicationPlatform.API.Filters.HangfireAuthorizationFilter() },
                DashboardTitle = "Job Application Platform - Hangfire"
            });

            // Register Hangfire Recurring Job: Runs daily at midnight UTC to auto-close expired jobs
            RecurringJob.AddOrUpdate<IJobBackgroundService>(
                "auto-close-expired-jobs",
                service => service.AutoCloseExpiredJobsAsync(),
                Cron.Daily);

            app.UseAuthentication();     
            app.UseAuthorization();

            app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
            app.MapControllers();

            app.Run();
        }
    }
}
