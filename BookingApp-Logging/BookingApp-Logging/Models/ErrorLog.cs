using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookingApp_Logging.Models
{
    public class ErrorLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ErrorMessage { get; set; }    
        public string StackTrace { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        public string SourceService { get; set; }
        public string ExceptionType { get; set; }
    }
}
