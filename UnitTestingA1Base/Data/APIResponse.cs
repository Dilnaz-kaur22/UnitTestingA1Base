public class APIResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public int StatusCode { get; set; }

    public APIResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}
