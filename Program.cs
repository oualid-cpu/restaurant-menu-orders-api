using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.Endpoints;
using RestaurantApi.Validation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// ✅ CORS (allow React dev server)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173" // Vite default
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateMenuItemDtoValidator>();

// DB
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=restaurant.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Global exception handler (ProblemDetails)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "An unexpected error occurred.";
        var detail = "Please try again later.";

        if (ex is DbUpdateException)
        {
            statusCode = StatusCodes.Status409Conflict;
            title = "Database update failed.";
            detail = "Your request could not be saved due to a database constraint or conflict.";
        }
        else if (ex is DbUpdateConcurrencyException)
        {
            statusCode = StatusCodes.Status409Conflict;
            title = "Concurrency conflict.";
            detail = "The resource was modified by another process. Please retry.";
        }

        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = app.Environment.IsDevelopment()
                ? ex?.Message
                : detail
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
    });
});

// Migrations + Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

// OpenAPI + Scalar UI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// ✅ Use CORS before endpoints
app.UseCors("AllowFrontend");

// Health check
app.MapGet("/", () => Results.Ok(new
{
    name = "Restaurant API",
    status = "running",
    openApi = "/openapi/v1.json",
    scalar = "/scalar"
}));

// Endpoint groups
app.MapMenuEndpoints();
app.MapOrdersEndpoints();
app.MapReportsEndpoints();

app.Run();
