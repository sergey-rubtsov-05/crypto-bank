using System.Reflection;
using crypto_bank.Common;
using crypto_bank.Database;
using crypto_bank.Infrastructure;
using crypto_bank.Infrastructure.Authentication;
using crypto_bank.WebAPI;
using crypto_bank.WebAPI.Features.Users.DependencyRegistration;
using crypto_bank.WebAPI.Models;
using crypto_bank.WebAPI.Pipeline;
using crypto_bank.WebAPI.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

builder.Services.AddCommon();
builder.Services.AddDatabase();
builder.Services.AddInfrastructure(builder.Configuration);

builder.RegisterDependencies();

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddSingleton<Dispatcher>();

builder.Services.AddControllers(options => options.ModelValidatorProviders.Clear());

builder.Services.AddUsers();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapPost("/user/login",
    async (UserLoginRequest request, TokenService tokenService) =>
    {
        logger.LogInformation($"Logging in user with email [{request.Email}]");

        var token = await tokenService.Create(request.Email, request.Password);

        return Results.Ok(new UserLoginResponse(token.AccessToken, token.RefreshToken));
    });

app.MapGet("/auth/validate", () => Results.Ok()).WithMetadata(new AuthenticationAttribute());

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();

app.MapControllers();

app.Run();
