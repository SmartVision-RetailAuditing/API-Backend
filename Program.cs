using Microsoft.EntityFrameworkCore;
using ApiBackend.Data;
// using ApiBackend.Services; 
//using ApiBackend.Models;
//using ApiBackend.Models.Context;


var builder = WebApplication.CreateBuilder(args);


// Gets appsettings.<ENVIRONMENT>.json file's "DefaultConnection"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// 1. Repositories (Data Access Layer) Injection
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IUserRepository, ApiBackend.Repositories.Impl.UserRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.ITaskRepository, ApiBackend.Repositories.Impl.TaskRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IStoreRepository, ApiBackend.Repositories.Impl.StoreRepository>();
builder.Services.AddScoped<ApiBackend.Repositories.Interfaces.IAuditRepository, ApiBackend.Repositories.Impl.AuditRepository>();

// 2. Services (Business Logic Layer) Injection
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IAuthService, ApiBackend.Services.Impl.AuthService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.ITaskService, ApiBackend.Services.Impl.TaskService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IStoreService, ApiBackend.Services.Impl.StoreService>();
builder.Services.AddScoped<ApiBackend.Services.Interfaces.IUserService, ApiBackend.Services.Impl.UserService>();

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

// Add Authorization Support to Swagger
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


var app = builder.Build();


// Checks 'ASPNETCORE_Environment' Environment Variable can be set from Properties/launchProfiles.json
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Seed database(dummy data)
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            DbInitializer.SeedDevData(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Database couldn't seed");
        }
    }
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllers();

app.Run();
