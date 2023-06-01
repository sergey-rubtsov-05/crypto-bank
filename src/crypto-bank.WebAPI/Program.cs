using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using crypto_bank.Common;
using crypto_bank.Database;
using crypto_bank.Domain.Authorization;
using crypto_bank.WebAPI;
using crypto_bank.WebAPI.Authorization;
using crypto_bank.WebAPI.Common;
using crypto_bank.WebAPI.Common.Validation;
using crypto_bank.WebAPI.Features.Accounts.DependencyRegistration;
using crypto_bank.WebAPI.Features.Auth.DependencyRegistration;
using crypto_bank.WebAPI.Features.Auth.Options;
using crypto_bank.WebAPI.Features.Users.DependencyRegistration;
using crypto_bank.WebAPI.Pipeline;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
    options.AddPolicy(PolicyName.AnalystRole, policy => policy.RequireRole(Role.Analyst.ToString()));
});

//TODO Think about using Enhanced.DependencyInjection
builder.Services.AddCommonProject();
builder.Services.AddDatabaseProject();

builder.Services.AddCommon();
builder.Services.AddAccounts(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddUsers(builder.Configuration);

builder.Services.EnsureValidatorsAreRegistered<Program>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//TODO Rename solution to CryptoBank
