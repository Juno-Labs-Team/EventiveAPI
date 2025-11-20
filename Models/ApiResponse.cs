namespace EventiveAPI.CSharp.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ErrorDetails? Error { get; set; }

    public static ApiResponse<T> CreateSuccess(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> CreateError(string message, int statusCode = 500)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ErrorDetails { Message = message, StatusCode = statusCode }
        };
    }
}

public class ErrorDetails
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 500;
}
