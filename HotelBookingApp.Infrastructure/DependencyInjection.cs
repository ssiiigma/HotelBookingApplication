using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Application.Services;
using HotelBookingApp.Infrastructure.Data;
using HotelBookingApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace HotelBookingApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                        connectionString,
                        new MySqlServerVersion(new Version(8, 0, 26)),
                        mysqlOptions =>
                        {
                            mysqlOptions.SchemaBehavior(MySqlSchemaBehavior.Ignore);
                        })
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            );

            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IHotelService, HotelService>();

            return services;
        }
    }
}