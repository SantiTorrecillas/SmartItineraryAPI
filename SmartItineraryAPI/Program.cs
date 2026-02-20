using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using SmartItineraryAPI.Application.Interfaces;
using SmartItineraryAPI.Infrastructure.AI;
using SmartItineraryAPI.Models.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<ItineraryRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IItineraryGenerator, OpenAiItineraryGenerator>();
builder.Services.AddMemoryCache();

builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection("OpenAI"));

builder.Services.AddSingleton<OpenAI.OpenAIClient>(sp =>
{
    OpenAiOptions options = sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<OpenAiOptions>>().Value;

    return new OpenAI.OpenAIClient(options.ApiKey);
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("itinerary-policy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsync(
            """
            {
                "error": "Too many requests. Please try again later."
            }
            """,
            token);
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseRateLimiter();

app.Run();
