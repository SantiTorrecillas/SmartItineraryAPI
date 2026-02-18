using SmartItineraryAPI.Application.Interfaces;
using SmartItineraryAPI.Infrastructure.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IItineraryGenerator, OpenAiItineraryGenerator>();

builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection("OpenAI"));

builder.Services.AddSingleton<OpenAI.OpenAIClient>(sp =>
{
    OpenAiOptions options = sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<OpenAiOptions>>().Value;

    return new OpenAI.OpenAIClient(options.ApiKey);
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

app.Run();
