using crypto_bank.Database;
using crypto_bank.Domain;
using crypto_bank.WebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
builder.Services.AddDatabase();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapPost("/user/register", async (UserRegistrationRequest request, CryptoBankDbContext cryptoBankDb) =>
{
    logger.LogInformation($"Registering user with email [{request.Email}]");

    var entityEntry = await cryptoBankDb.Users.AddAsync(new User(request.Email, request.Password));
    await cryptoBankDb.SaveChangesAsync();

    var registeredUser = entityEntry.Entity;

    logger.LogInformation($"User was registered with id [{registeredUser.Id}]");

    return Results.Created($"/user/{registeredUser.Id}", new UserRegistrationResponse { Id = registeredUser.Id });
});

app.Run();
