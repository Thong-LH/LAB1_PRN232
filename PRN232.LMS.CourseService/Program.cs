using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PRN232.LMS.CourseService.Data;
using PRN232.LMS.CourseService.Grpc;
using PRN232.LMS.CourseService.Middleware;
using PRN232.LMS.Grpc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (HTTP {Method} {Path} status {StatusCode} took {Elapsed:0.0000} ms){NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

// JWT configurations
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "PRN232_LMS_LAB3_MICROSERVICES_SECRET_KEY_2026_LONG";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PRN232.LMS.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PRN232.LMS.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Database context setup
builder.Services.AddDbContext<CourseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// gRPC Client
builder.Services.AddGrpcClient<StudentGrpc.StudentGrpcClient>(options =>
{
    var url = builder.Configuration["GrpcServices:StudentServiceUrl"] ?? "http://localhost:5002";
    options.Address = new Uri(url);
});
builder.Services.AddScoped<IStudentGrpcClient, StudentGrpcClient>();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Course Service API", Version = "v1" });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Course Service API v1");
    });
}

// Database startup & seeding
await EnsureDatabaseCreatedAsync(app);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task EnsureDatabaseCreatedAsync(WebApplication app)
{
    const int maxAttempts = 60;
    var delay = TimeSpan.FromSeconds(2);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CourseDbContext>();

            if (app.Configuration.GetValue<bool>("Database:RecreateOnStartup"))
            {
                await dbContext.Database.EnsureDeletedAsync();
            }

            await dbContext.Database.EnsureCreatedAsync();
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Course database is not ready. Retry {Attempt}/{MaxAttempts}", attempt, maxAttempts);
            await Task.Delay(delay);
        }
    }
}
