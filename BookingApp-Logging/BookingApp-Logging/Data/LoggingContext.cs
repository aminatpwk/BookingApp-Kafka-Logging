using Microsoft.EntityFrameworkCore;
using BookingApp_Logging.Models;

namespace BookingApp_Logging.Data
{
    public class LoggingContext : DbContext
    {
        public LoggingContext(DbContextOptions<LoggingContext> options) : base(options)
        {
        }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
    }
}
