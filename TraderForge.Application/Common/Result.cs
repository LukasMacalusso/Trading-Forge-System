namespace TraderForge.Application.Common;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    
    public static Result Success() => new Result {IsSuccess = true};
    public static Result Failure(string errorMessage) => new Result { IsSuccess = false, ErrorMessage = errorMessage };
}