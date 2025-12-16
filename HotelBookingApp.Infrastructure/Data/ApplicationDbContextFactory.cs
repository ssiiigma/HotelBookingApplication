using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelBookingApp.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Встав свій connection string
            var connectionString = "server=localhost;database=HotelBookingDB;user=root;password=11111";

            optionsBuilder.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 26))
                )
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}