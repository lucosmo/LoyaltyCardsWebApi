using System.Diagnostics;
using LoyaltyCardsWebApi.API.Common;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCardsWebApi.API.Controllers.Results;

public class ApiResult<T> : IActionResult
{
    private readonly Result<T> _result;
    private const string errorTitle = "An error occurred.";
    public ApiResult(Result<T> result)
    {
        _result = result;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ObjectResult response;

        if (_result.Success && _result.SuccessType is not null)
        {
            int statusCode = GetSuccessStatusCode(_result.SuccessType.Value);
            if (statusCode == StatusCodes.Status204NoContent)
            {
                context.HttpContext.Response.StatusCode = statusCode;
                return;
            }
            if (statusCode == StatusCodes.Status201Created && !string.IsNullOrEmpty(_result.Location))
            {
                context.HttpContext.Response.Headers.Location = _result.Location;
            }
            response = new ObjectResult(_result.Value) { StatusCode = statusCode };
        }
        else
        {
            var detail = !string.IsNullOrEmpty(_result.Error) ? _result.Error : "Authentication failed.";
            response = new ObjectResult(ProblemDetailsHelper.CreateProblemDetails(
                    context.HttpContext,
                    errorTitle,
                    GetErrorStatusCode(_result.ErrorType),
                    detail
                    ));
        }
        await response.ExecuteResultAsync(context);
    }

    static int GetErrorStatusCode(ErrorTypes errorType)
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

    static int GetSuccessStatusCode(SuccessTypes successType)
    {
        return successType switch
        {
            SuccessTypes.Created => StatusCodes.Status201Created,
            SuccessTypes.NoContent => StatusCodes.Status204NoContent,
            _ => StatusCodes.Status200OK
        };
    }
}