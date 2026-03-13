using Microsoft.EntityFrameworkCore;
using ApiBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. Repositories (Data Access Layer) Injection
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IUserRepository, ApiBackend.Repositories.Impl.UserRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.ITaskRepository, ApiBackend.Repositories.Impl.TaskRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IStoreRepository, ApiBackend.Repositories.Impl.StoreRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IAuditRepository, ApiBackend.Repositories.Impl.AuditRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IAuditProductRepository, ApiBackend.Repositories.Impl.AuditProductRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IAuditIssueRepository, ApiBackend.Repositories.Impl.AuditIssueRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IDashboardRepository, ApiBackend.Repositories.Impl.DashboardRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IAnalyticsRepository, ApiBackend.Repositories.Impl.AnalyticsRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.INotificationRepository, ApiBackend.Repositories.Impl.NotificationRepository>();

// 2. Services (Business Logic Layer) Injection
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IAuthService, ApiBackend.Services.Impl.AuthService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IUserService, ApiBackend.Services.Impl.UserService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.ITaskService, ApiBackend.Services.Impl.TaskService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IStoreService, ApiBackend.Services.Impl.StoreService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IAuditService, ApiBackend.Services.Impl.AuditService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IAuditProductService, ApiBackend.Services.Impl.AuditProductService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IAuditIssueService, ApiBackend.Services.Impl.AuditIssueService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IDashboardService, ApiBackend.Services.Impl.DashboardService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IAnalyticsService, ApiBackend.Services.Impl.AnalyticsService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.INotificationService, ApiBackend.Services.Impl.NotificationService>();

// ── AI Pipeline servisleri ───────────────────────────────────────────────────
builder.Services.AddScoped<ApiBackend.Services.Interfaces.ICloudStorageService, ApiBackend.Services.Impl.CloudStorageService>();
// AIVisionService için named HttpClient — timeout 90 saniye (AI işlem süresi)
// (Python hazır olunca çalışacak):
builder.Services.AddHttpClient<ApiBackend.Services.Interfaces.IAIVisionService, ApiBackend.Services.Impl.AIVisionService>(client => {
    client.Timeout = TimeSpan.FromSeconds(90);
});
builder.Services.AddScoped<ApiBackend.Mappers.AiResponseMapper>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IAuditSubmissionService, ApiBackend.Services.Impl.AuditSubmissionService>();

builder.Services.AddControllers();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key)
    };
});

// Swagger API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SmartVision API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter your JWT Token in 'Bearer <JWT_TOKEN>' format",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// CORS
// Development: "http://localhost:5173"
// Production:  "http://localhost:5173,https://smartvision.azurewebsites.net"
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]
    ?? "http://localhost:5173";

var origins = allowedOrigins
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ── Otomatik Migration ────────────────────────────────────────────────────────
// Her deploy'da pending migration'ları uygular — idempotent, güvenli
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pending = db.Database.GetPendingMigrations().ToList();

        if (pending.Any())
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pending.Count, string.Join(", ", pending));
            db.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("Database is up to date, no migrations needed.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration failed. Application will continue but DB may be out of date.");
        // Throw etmiyoruz — migration hatası uygulamayı çökertmesin,
        // log'dan takip edilir
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { } //To make app accessible from ApiBackend.Tests 
