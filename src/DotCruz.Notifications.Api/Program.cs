using DotCruz.Notifications.Api.Configurations;
using DotCruz.Notifications.Api.Filters;
using DotCruz.Notifications.Api.Middlewares;
using DotCruz.Notifications.Application;
using DotCruz.Notifications.CrossCutting;
using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Infrastructure;
using DotCruz.Shared.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApiConventions();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSharedSecurity(builder.Configuration);
builder.Services.AddCrossCutting(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseMiddleware<CultureMiddleware>();

app.UseSharedSecurityAuditLog();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
