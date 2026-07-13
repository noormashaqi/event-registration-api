using DotNetEnv;
using EventRegistration.Api.Features.Categories;
using EventRegistration.Api.Interfaces;
using EventRegistration.Api.Behaviors;
using EventRegistration.Api.Database;
using EventRegistration.Api.Features.Participants;
using EventRegistration.Api.Middleware;
using FluentValidation;
using MediatR;
using Serilog;

Env.Load();

// Bootstrap logger: captures anything that happens before the host
// (and its configuration-driven logger) is fully built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

    if (!string.IsNullOrWhiteSpace(dbConnectionString))
    {
        builder.Configuration["ConnectionStrings:Default"] = dbConnectionString;
    }

    var corsAllowedOrigin = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGIN")
        ?? "http://localhost:5173";

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    });

    builder.Services.AddSingleton<IEventRegistrationDatabase, EventRegistrationDatabase>();

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining<CreateParticipantCommand>();
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    builder.Services.AddValidatorsFromAssemblyContaining<CreateParticipantValidator>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactApp", policy =>
        {
            policy
                .WithOrigins(corsAllowedOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("ReactApp");

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}