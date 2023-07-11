using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using CryptoBank.Common;
using CryptoBank.Database;
using CryptoBank.Domain.Authorization;
using CryptoBank.WebAPI;
using CryptoBank.WebAPI.Authorization;
using CryptoBank.WebAPI.Common;
using CryptoBank.WebAPI.Common.Validation;
using CryptoBank.WebAPI.Features.Accounts.DependencyRegistration;
using CryptoBank.WebAPI.Features.Auth.DependencyRegistration;
using CryptoBank.WebAPI.Features.Auth.Options;
using CryptoBank.WebAPI.Features.Deposits.DependencyRegistration;
using CryptoBank.WebAPI.Features.Users.DependencyRegistration;
using CryptoBank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(
    cfg => cfg
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
    .AddJwtBearer(
        options =>
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

builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy(PolicyName.AdministratorRole, policy => policy.RequireRole(Role.Administrator.ToString()));
        options.AddPolicy(PolicyName.AnalystRole, policy => policy.RequireRole(Role.Analyst.ToString()));
    });

//TODO Think about using Enhanced.DependencyInjection
builder.Services.AddCommonProject();
builder.Services.AddDatabaseProject(builder.Configuration);

builder.Services.AddCommon(builder.Configuration);

builder.Services
    .AddAccounts(builder.Configuration)
    .AddAuth(builder.Configuration)
    .AddDeposits(builder.Configuration)
    .AddUsers(builder.Configuration);

builder.Services.EnsureValidatorsAreRegistered<Program>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
