using System.Threading.RateLimiting;
using Serilog;
using EventiveAPI.CSharp.Services;
using EventiveAPI.CSharp.Middleware;
using EventiveAPI.CSharp.Configuration;
using FluentValidation;
using Microsoft.OpenApi.Models;

// Load .env file
DotNetEnv.Env.Load();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add environment variables configuration
    builder.Configuration.AddEnvironmentVariables();

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Add configuration validation
    var supabaseConfig = new SupabaseConfig
    {
        Url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? builder.Configuration["Supabase:Url"] ?? "",
        ServiceRoleKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY") ?? builder.Configuration["Supabase:ServiceRoleKey"] ?? "",
        AnonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? builder.Configuration["Supabase:AnonKey"] ?? ""
    };
    supabaseConfig.Validate();

    // Add services
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "EventiveAPI",
            Version = "v1",
            Description = "REST API for Eventive application"
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Add Supabase service
    builder.Services.AddSingleton<SupabaseService>();

    // Add FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Add CORS
    var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS_ORIGINS");
    
    // Split the env string by comma, OR fallback to appsettings.json
    var corsOrigins = !string.IsNullOrEmpty(corsOriginsEnv)
        ? corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries)
        : builder.Configuration.GetSection("Cors").Get<CorsConfig>()?.AllowedOrigins 
          ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Add rate limiting
    var rateLimitConfig = builder.Configuration.GetSection("RateLimit").Get<RateLimitConfig>() ?? new RateLimitConfig();
    if (rateLimitConfig.EnableRateLimiting)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rateLimitConfig.PermitLimit,
                        QueueLimit = rateLimitConfig.QueueLimit,
                        Window = rateLimitConfig.Window
                    }));
            
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }

    // Add compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Add health checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // Configure middleware pipeline
    app.UseSerilogRequestLogging();

    app.UseErrorHandling();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventiveAPI v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseResponseCompression();

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });

    app.UseCors();

    if (rateLimitConfig.EnableRateLimiting)
    {
        app.UseRateLimiter();
    }

    // Custom authentication middleware
    app.UseSupabaseAuthentication();

    app.MapControllers();
    app.MapHealthChecks("/health");

    var port = builder.Configuration.GetValue<int>("PORT", 3000);
    app.Urls.Add($"http://0.0.0.0:{port}");

    Log.Information("Starting EventiveAPI on port {Port}", port);
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

// Make Program class accessible for integration tests
public partial class Program { }
