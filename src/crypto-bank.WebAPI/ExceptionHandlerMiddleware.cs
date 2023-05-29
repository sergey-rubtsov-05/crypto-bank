using System.Diagnostics;
using crypto_bank.Domain.Validation;
using crypto_bank.Infrastructure.Features.Auth.Exceptions;
using crypto_bank.Infrastructure.Features.Users.Exceptions;
using crypto_bank.WebAPI.Errors;
using crypto_bank.WebAPI.Validation;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace crypto_bank.WebAPI;

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
        catch (DomainModelValidationException domainModelValidationException)
        {
            _logger.LogInformation(domainModelValidationException, "Domain model validation failed");
            problemDetails = CreateProblemDetails(domainModelValidationException, StatusCodes.Status409Conflict);
        }
        catch (ApiModelValidationException apiModelValidationException)
        {
            _logger.LogInformation(apiModelValidationException, "Api model validation failed");
            problemDetails = CreateProblemDetails(apiModelValidationException, StatusCodes.Status400BadRequest);
        }
        catch (UserAlreadyExistsException userAlreadyExistsException)
        {
            _logger.LogInformation(userAlreadyExistsException, "User already exists");
            problemDetails = CreateProblemDetails(StatusCodes.Status409Conflict, "User already exists");
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

            problemDetails.Extensions["code"] = logicConflictException.Code;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occured");
            problemDetails = CreateProblemDetails(StatusCodes.Status500InternalServerError, "Internal server error");
        }

        if (problemDetails is null)
            return;

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetails CreateProblemDetails(int httpStatusCode, string title)
    {
        return new ProblemDetails { Status = httpStatusCode, Title = title };
    }

    private static ProblemDetails CreateProblemDetails(ValidationException validationException, int httpStatusCode)
    {
        var validationFailures = validationException.Errors ?? Enumerable.Empty<ValidationFailure>();
        var problemDetails = new ProblemDetails
        {
            Status = httpStatusCode,
            Title = validationException.Message,
            Detail = string.Join(", ", validationFailures.Select(error => error.ErrorMessage)),
        };
        return problemDetails;
    }
}
