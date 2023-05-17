using crypto_bank.Domain.Validators.Base;
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
        try
        {
            await next(context);
        }
        catch (DomainModelValidationException domainModelValidationException)
        {
            _logger.LogInformation(domainModelValidationException, "Domain model validation failed");
            var validationFailures = domainModelValidationException.Errors ?? Enumerable.Empty<ValidationFailure>();
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = domainModelValidationException.Message,
                Detail = string.Join(", ",
                    validationFailures.Select(error => error.ErrorMessage)),
            };

            context.Response.StatusCode = problemDetails.Status.Value;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occured");
        }
    }
}
