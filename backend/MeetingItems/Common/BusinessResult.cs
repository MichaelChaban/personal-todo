#nullable enable
namespace MeetingItemsApp.Common;

public class BusinessResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; }

    private BusinessResult(bool isSuccess, T? data, string? errorMessage, int statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static BusinessResult<T> Success(T data) =>
        new(true, data, null, 200);

    public static BusinessResult<T> NotFound(string message) =>
        new(false, default, message, 404);

    public static BusinessResult<T> BadRequest(string message) =>
        new(false, default, message, 400);

    public static BusinessResult<T> Unauthorized(string message) =>
        new(false, default, message, 401);

    public static BusinessResult<T> Forbidden(string message) =>
        new(false, default, message, 403);

    public static BusinessResult<T> Failure(string message, int statusCode = 500) =>
        new(false, default, message, statusCode);
}

public static class BusinessResult
{
    public static BusinessResult<T> Success<T>(T data) =>
        BusinessResult<T>.Success(data);

    public static BusinessResult<T> NotFound<T>(string message) =>
        BusinessResult<T>.NotFound(message);

    public static BusinessResult<T> BadRequest<T>(string message) =>
        BusinessResult<T>.BadRequest(message);

    public static BusinessResult<T> Unauthorized<T>(string message) =>
        BusinessResult<T>.Unauthorized(message);

    public static BusinessResult<T> Forbidden<T>(string message) =>
        BusinessResult<T>.Forbidden(message);

    public static BusinessResult<T> Failure<T>(string message, int statusCode = 500) =>
        BusinessResult<T>.Failure(message, statusCode);
}

public static class BusinessErrorMessage
{
    public const string Required = "This field is required";
    public const string NotFound = "The requested resource was not found";
    public const string Unauthorized = "You are not authorized to perform this action";
    public const string Forbidden = "You do not have permission to access this resource";
    public const string ValidationFailed = "Validation failed";
}
