namespace testing.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse(T data, string message = "Success")
    {
        Success = true;
        Message = message;
        Data = data;
    }

    public ApiResponse(string errorMessage, string? errorCode = null)
    {
        Success = false;
        Message = errorMessage;
        ErrorCode = errorCode;
        Data = default;
    }

    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        => new ApiResponse<T>(data, message);

    public static ApiResponse<T> ErrorResult(string message, string? errorCode = null)
        => new ApiResponse<T>(message, errorCode);
}

public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PagedResponse(
        IEnumerable<T> data,
        int page,
        int pageSize,
        int totalCount,
        string message = "Success")
        : base(data, message)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    public virtual bool IsValid()
    {
        return Page > 0 && PageSize > 0 && PageSize <= 1000;
    }
}