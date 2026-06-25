namespace TraderForge.Application.Common;

public class ResultGeneric<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorMessage { get; set; }
    
    public static ResultGeneric<T> Success(T value) => new ResultGeneric<T> { IsSuccess = true, Value = value };
    public static ResultGeneric<T> Failure(string errorMessage) => new ResultGeneric<T> { IsSuccess = false, ErrorMessage = errorMessage };
}