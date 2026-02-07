using Microsoft.EntityFrameworkCore;
using ApiBackend.Data;
// using ApiBackend.Services; // Ýleride Auth servisi gelince burayý açacaðýz

//using ApiBackend.Models;
//using ApiBackend.Models.Context;

using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// 1. Veritabaný Baðlantýsýný Servise Ekle
// appsettings.json dosyasýndaki "DefaultConnection"ý okur.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// 2. Controllerlarý Ekle
builder.Services.AddControllers();



// JWT Authentication Ayarlarý
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



// 3. Swagger (API Dokümantasyonu) Ekle
builder.Services.AddEndpointsApiExplorer();

// Swagger'a JWT Desteði Ekleme
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SmartVision API", Version = "v1" });

    option.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Lütfen token'ý 'Bearer [boþluk] TOKENINIZ' formatýnda girin",
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


// 4. Middleware (Ara Yazýlým) Ayarlarý
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


app.UseAuthentication();  // Önce kimlik kontrolü
app.UseAuthorization(); //Sonra yetki kontrolü

app.MapControllers();



// Uygulama ayaða kalkarken veritabanýný tohumla (Seed)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabaný oluþturulurken bir hata oluþtu.");
    }
}



app.Run();



//builder.Services.AddDbContext<TestContext>(opt =>
//    opt.UseInMemoryDatabase("TestList"));
//builder.Services.AddDbContext<AnalyticsReportContext>(opt =>
//    opt.UseInMemoryDatabase("AnalyticsReportList"));
//builder.Services.AddDbContext<ComplienceReportContext>(opt =>
//    opt.UseInMemoryDatabase("ComplienceReportList"));
//builder.Services.AddDbContext<TaskContext>(opt =>
//    opt.UseInMemoryDatabase("TaskList"));
//builder.Services.AddDbContext<UserContext>(opt =>
//    opt.UseInMemoryDatabase("UserList"));

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
//            ValidateAudience = true,
//            ValidAudience = builder.Configuration["AppSettings:Audience"],
//            ValidateLifetime = true,
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
//            ValidateIssuerSigningKey = true
//        };
//    });



//builder.Services.AddScoped<IAuthService,AuthService>();