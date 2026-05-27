using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Repositories.Data;
using Microsoft.OpenApi.Models;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Repositories;
using PRN232.LMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(modelState => modelState.Value?.Errors.Count > 0)
            .SelectMany(modelState => modelState.Value!.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "Invalid request value."
                : error.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", errors));
    };
});
builder.Services.AddLmsRepositories(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured."));
builder.Services.AddLmsServices();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 LMS REST API",
        Version = "v1",
        Description = "RESTful API for students, courses, subjects, semesters, and enrollments with CRUD, paging, sorting, field selection, and expandable related resources."
    });

    options.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time",
        Example = new Microsoft.OpenApi.Any.OpenApiString("2026-05-26T00:00:00")
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:EnsureCreatedOnStartup"))
{
    await EnsureDatabaseCreatedAsync(app);
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS REST API v1");
    options.DocumentTitle = "PRN232 LMS API Docs";
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task EnsureDatabaseCreatedAsync(WebApplication app)
{
    const int maxAttempts = 20;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            return;
        }
        catch when (attempt < maxAttempts)
        {
            await Task.Delay(delay);
        }
    }
}
