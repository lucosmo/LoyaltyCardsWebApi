using LoyaltyCardsWebApi.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCardsWebApi.API.Controllers.Results;

public class ApiResult<T> : IActionResult
{
    private readonly Result<T> _result;
    private const string errorTitle = "An error occured.";
    public ApiResult(Result<T> result)
    {
        _result = result;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ObjectResult response;

        if (_result.Success)
        {
            int statusCode = _result.SuccessType == SuccessTypes.Ok ? StatusCodes.Status200OK : StatusCodes.Status201Created;
            response = new ObjectResult(_result.Value) { StatusCode = statusCode };
            if (statusCode == StatusCodes.Status201Created && !string.IsNullOrEmpty(_result.Location))
            {
                context.HttpContext.Response.Headers.Location = _result.Location;
            }
        }
        else
        {
            response = new ObjectResult(CreateProblemDetails(
                    _result.Error,
                    GetStatusCode(_result.ErrorType),
                    context.HttpContext.Request.Path,
                    errorTitle
                    ));
        }    
        await response.ExecuteResultAsync(context);
    }

    private ProblemDetails CreateProblemDetails(string? message, int statusCode, string instance, string title)
    {
        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = message,
            Instance = instance
        };
    }

    static int GetStatusCode(ErrorTypes errorType)
    {
        return errorType switch
        {
            ErrorTypes.NotFound => StatusCodes.Status404NotFound,
            ErrorTypes.BadRequest => StatusCodes.Status400BadRequest,
            ErrorTypes.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorTypes.Forbidden => StatusCodes.Status403Forbidden,
            ErrorTypes.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

}