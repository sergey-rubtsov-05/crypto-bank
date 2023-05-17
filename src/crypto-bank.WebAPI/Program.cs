using crypto_bank.Database;
using crypto_bank.Domain;
using crypto_bank.Domain.Validators;
using crypto_bank.WebAPI;
using crypto_bank.WebAPI.Models;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
builder.Services.AddDatabase();
builder.Services.AddSingleton<IValidator<User>, UserValidator>();
builder.Services.AddScoped<ExceptionHandlerMiddleware>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapPost("/user/register",
    async (UserRegistrationRequest request, CryptoBankDbContext cryptoBankDb, IValidator<User> userValidator) =>
    {
        logger.LogInformation($"Registering user with email [{request.Email}]");

        var user = new User(request.Email, request.Password);

        await userValidator.ValidateAndThrowAsync(user);

        var entityEntry = await cryptoBankDb.Users.AddAsync(user);
        await cryptoBankDb.SaveChangesAsync();

        var registeredUser = entityEntry.Entity;

        logger.LogInformation($"User was registered with id [{registeredUser.Id}]");

        return Results.Created($"/user/{registeredUser.Id}", new UserRegistrationResponse { Id = registeredUser.Id });
    });

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Run();
