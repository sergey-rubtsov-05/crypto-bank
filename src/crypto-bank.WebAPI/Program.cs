using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using crypto_bank.Common;
using crypto_bank.Database;
using crypto_bank.Domain;
using crypto_bank.Domain.Authorization;
using crypto_bank.Infrastructure;
using crypto_bank.Infrastructure.Features.Auth.Options;
using crypto_bank.WebAPI;
using crypto_bank.WebAPI.Authorization;
using crypto_bank.WebAPI.Features.Accounts.DependencyRegistration;
using crypto_bank.WebAPI.Features.Auth.DependencyRegistration;
using crypto_bank.WebAPI.Features.Users.DependencyRegistration;
using crypto_bank.WebAPI.Pipeline;
using crypto_bank.WebAPI.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddSingleton<Dispatcher>();

builder.Services
    .AddControllers(options => options.ModelValidatorProviders.Clear())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ExceptionHandlerMiddleware>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("Features:Auth").Get<AuthOptions>()!.Jwt;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            //todo I have two places for configuring how to work with tokens. I need to refactor it
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyName.AdministratorRole, policy => policy.RequireRole(Role.Administrator.ToString()));
});

builder.Services.AddCommon();
builder.Services.AddDatabase();
builder.Services.AddDomain();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAccounts(builder.Configuration);
builder.Services.AddAuth();
builder.Services.AddUsers();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//TODO Refactorings:
// 1. Get rid of the project Infrastructure and move its content to the project WebAPI
// 2. Use the same structure in features
// 3. Get rid of validators in Domain project
