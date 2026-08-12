using WoofBnB.Application.Common.Abstractions;

namespace WoofBnB.Application.Common.Responses;

public sealed class ApiResponseFactory : IApiResponseFactory
{
    private readonly IClock _clock;

    public ApiResponseFactory(IClock clock)
    {
        _clock = clock;
    }

    public ApiResponse<T> Success<T>(int statusCode, string message, T? data) =>
        ApiResponse<T>.Create(statusCode, message, data, _clock.UtcNow);
}
