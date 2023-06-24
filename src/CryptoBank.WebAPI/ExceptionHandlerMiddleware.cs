using System.Diagnostics;
using CryptoBank.WebAPI.Common.Errors.Exceptions;
using CryptoBank.WebAPI.Features.Auth.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebAPI;

public class ExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ProblemDetails? problemDetails = null;
        try
        {
            await next(context);
        }
        catch (ApiModelValidationException apiModelValidationException)
        {
            _logger.LogInformation(apiModelValidationException, "Api model validation failed");
            problemDetails = CreateProblemDetails(apiModelValidationException, StatusCodes.Status400BadRequest);
        }
        catch (ApiValidationException apiValidationException)
        {
            _logger.LogInformation(apiValidationException, "Api validation failed");
            problemDetails = CreateProblemDetails(apiValidationException, StatusCodes.Status400BadRequest);
        }
        catch (AuthenticationException authenticationException)
        {
            _logger.LogInformation(authenticationException, "Authentication failed");
            problemDetails = CreateProblemDetails(StatusCodes.Status401Unauthorized, "Authentication failed");
        }
        catch (LogicConflictException logicConflictException)
        {
            _logger.LogInformation(logicConflictException, "Logic conflict occured");
            problemDetails = new ProblemDetails
            {
                Title = "Logic conflict",
                Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422",
                Detail = logicConflictException.Message,
                Status = StatusCodes.Status422UnprocessableEntity,
            };

            problemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? context.TraceIdentifier);

            problemDetails.Extensions["code"] = logicConflictException.ErrorCode;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occured");
            problemDetails = CreateProblemDetails(StatusCodes.Status500InternalServerError, "Internal server error");
        }

        if (problemDetails is null)
            return;

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        //todo: use correct json serializer. What if we want to use NewtonsoftJson? 
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetails CreateProblemDetails(int httpStatusCode, string title)
    {
        return new ProblemDetails { Status = httpStatusCode, Title = title };
    }

    private static ProblemDetails CreateProblemDetails(
        ApiModelValidationException validationException,
        int httpStatusCode)
    {
        var validationFailures = validationException.Errors;
        var problemDetails = new ProblemDetails
        {
            Status = httpStatusCode,
            Title = "Api model validation exception",
            Detail = validationException.Message,
            Extensions =
            {
                ["errors"] = validationException.Errors.Select(
                    validationFailure => new
                    {
                        Field = validationFailure.PropertyName,
                        Message = validationFailure.ErrorMessage,
                        Code = validationFailure.ErrorCode,
                    }),
            },
        };

        return problemDetails;
    }

    private static ProblemDetails CreateProblemDetails(
        ApiValidationException validationException,
        int httpStatusCode)
    {
        var problemDetails = new ProblemDetails
        {
            Status = httpStatusCode,
            Title = "Api validation failed",
            Extensions = { ["code"] = validationException.ErrorCode },
        };

        return problemDetails;
    }
}
