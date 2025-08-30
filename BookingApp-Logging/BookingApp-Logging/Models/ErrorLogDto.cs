namespace BookingApp_Logging.Models
{
    public class ErrorLogDto
    {
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public DateTime Timestamp { get; set; }
        public string SourceService { get; set; }
        public string ExceptionType { get; set; }
    }
}
