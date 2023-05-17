using crypto_bank.Infrastructure;
using crypto_bank.WebAPI;
using crypto_bank.WebAPI.Models;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterDependencies();

var app = builder.Build();

app.MapPost("/user/register",
    async (
        UserRegistrationRequest request,
        UserService userService,
        IValidator<UserRegistrationRequest> requestValidator,
        ILogger<Program> logger) =>
    {
        logger.LogInformation($"Registering user with email [{request.Email}]");

        await requestValidator.ValidateAndThrowAsync(request);

        var registeredUser = await userService.Register(request.Email, request.Password);

        logger.LogInformation($"User was registered with id [{registeredUser.Id}]");

        return Results.Created($"/user/{registeredUser.Id}", new UserRegistrationResponse { Id = registeredUser.Id });
    });

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Run();
