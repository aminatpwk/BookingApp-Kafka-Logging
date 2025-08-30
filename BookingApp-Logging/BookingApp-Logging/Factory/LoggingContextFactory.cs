using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using BookingApp_Logging.Data;

namespace BookingApp_Logging.Factory
{
    public class LoggingContextFactory : IDesignTimeDbContextFactory<LoggingContext>
    {
        public LoggingContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<LoggingContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if(string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            builder.UseSqlServer(connectionString);
            return new LoggingContext(builder.Options);
        }
    }
}
