namespace Oqtane.Models
{
    public class Result
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public Result(bool success)
        {
            Success = success;
            Message = "";
        }

        public Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
