using System.Reflection;
using System.Text.Json.Serialization;
using crypto_bank.Common;
using crypto_bank.Database;
using crypto_bank.Domain;
using crypto_bank.Infrastructure;
using crypto_bank.WebAPI;
using crypto_bank.WebAPI.Features.Auth.DependencyRegistration;
using crypto_bank.WebAPI.Features.Users.DependencyRegistration;
using crypto_bank.WebAPI.Pipeline;
using crypto_bank.WebAPI.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

builder.Services.AddCommon();
builder.Services.AddDatabase();
builder.Services.AddDomain();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddSingleton<Dispatcher>();

builder.Services
    .AddControllers(options => options.ModelValidatorProviders.Clear())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<ExceptionHandlerMiddleware>();
builder.Services.AddScoped<AuthenticationMiddleware>();

builder.Services.AddAuth();
builder.Services.AddUsers();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();

app.MapControllers();

app.Run();
