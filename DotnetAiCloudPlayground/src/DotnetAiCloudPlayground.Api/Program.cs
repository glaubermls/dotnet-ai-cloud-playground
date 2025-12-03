using DotnetAiCloudPlayground.Core.Application;
using DotnetAiCloudPlayground.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure OpenAI client with Polly resilience policies
builder.Services.AddOpenAIClient(builder.Configuration);

// Register application services
builder.Services.AddScoped<ChatUseCase>();

// Add logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DotnetAiCloudPlayground API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
        c.DocumentTitle = "DotnetAiCloudPlayground API Documentation";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health")
.WithSummary("Check API health status")
.WithDescription("Returns the current health status of the API");

app.Run();
