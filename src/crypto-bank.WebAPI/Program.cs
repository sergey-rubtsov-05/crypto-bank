using crypto_bank.Infrastructure;
using crypto_bank.WebAPI;
using crypto_bank.WebAPI.Models;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterDependencies();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapPost("/user/register",
    async (
        UserRegistrationRequest request,
        UserService userService,
        IValidator<UserRegistrationRequest> requestValidator) =>
    {
        logger.LogInformation($"Registering user with email [{request.Email}]");

        await requestValidator.ValidateAndThrowAsync(request);

        var registeredUser = await userService.Register(request.Email, request.Password);

        logger.LogInformation($"User was registered with id [{registeredUser.Id}]");

        return Results.Created($"/user/{registeredUser.Id}", new UserRegistrationResponse { Id = registeredUser.Id });
    });

app.MapPost("/user/login",
    async (UserLoginRequest request, TokenService tokenService) =>
    {
        logger.LogInformation($"Logging in user with email [{request.Email}]");

        var token = await tokenService.Create(request.Email, request.Password);

        return Results.Ok(new UserLoginResponse(token.AccessToken, token.RefreshToken));
    });

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Run();
