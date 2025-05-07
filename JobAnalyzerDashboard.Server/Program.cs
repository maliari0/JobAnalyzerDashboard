using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Services;
using System;
using System.IO;
using System.Text;
using System.Globalization;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Geliştirme ortamında User Secrets kullan
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Loglama yapılandırması
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// UTF-8 kodlamasını varsayılan olarak ayarla
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Döngüsel referanslar için ReferenceHandler.IgnoreCycles kullan
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 64; // Varsayılan değer 32'dir
    // Özel JSON formatını devre dışı bırak
    options.JsonSerializerOptions.WriteIndented = true;
    // Büyük boyutlu verilerin serileştirilmesine izin ver
    options.JsonSerializerOptions.DefaultBufferSize = 1024 * 1024; // 1 MB
});

// Configure JSON content type
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL veritabanı bağlantısını ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT ayarlarını yapılandır
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Kimlik doğrulama servislerini ekle
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
            builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured"))),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS politikalarını yapılandır
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Repository servislerini ekle
builder.Services.AddScoped<JobAnalyzerDashboard.Server.Repositories.IJobRepository, JobAnalyzerDashboard.Server.Repositories.JobRepository>();
builder.Services.AddScoped<JobAnalyzerDashboard.Server.Repositories.IApplicationRepository, JobAnalyzerDashboard.Server.Repositories.ApplicationRepository>();
builder.Services.AddScoped<JobAnalyzerDashboard.Server.Repositories.IProfileRepository, JobAnalyzerDashboard.Server.Repositories.ProfileRepository>();

// OAuth, E-posta ve Kimlik Doğrulama servislerini ekle
builder.Services.AddScoped<OAuthService>();
builder.Services.AddScoped<GmailApiService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuthService>();

// CORS politikasını ekle - n8n entegrasyonu ve statik dosya erişimi için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Type", "Content-Disposition", "Content-Length");
    });

    options.AddPolicy("AllowN8n", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
            new[] { "http://localhost:5678", "https://n8n-service-a2yz.onrender.com" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Type", "Content-Disposition", "Content-Length");
    });
});

var app = builder.Build();

// Statik dosyaları sunmak için middleware'leri yapılandır
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // PDF dosyaları için MIME türünü ayarla
        if (ctx.File.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "application/pdf");
        }

        // Cache kontrolü
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

// Uploads klasörünün varlığını kontrol et ve oluştur
string uploadsFolder = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "resumes");
if (!Directory.Exists(uploadsFolder))
{
    Directory.CreateDirectory(uploadsFolder);
}

// Uploads klasörü için özel dosya sağlayıcısı ekle
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true, // Bilinmeyen dosya türlerini de servis et
    DefaultContentType = "application/octet-stream", // Varsayılan içerik türü
    OnPrepareResponse = ctx =>
    {
        // PDF dosyaları için MIME türünü ayarla
        if (ctx.File.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "application/pdf");
        }

        // CORS başlıklarını ekle
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
        ctx.Context.Response.Headers.Append("Cross-Origin-Resource-Policy", "cross-origin");

        // Content-Security-Policy başlığını ekle
        ctx.Context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self'");

        // Cache kontrolü
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS middleware'ini etkinleştir
app.UseCors("AllowAll");

// JWT middleware'i ekle
app.UseMiddleware<JobAnalyzerDashboard.Server.Middleware.JwtMiddleware>();

// Kimlik doğrulama ve yetkilendirme middleware'lerini ekle
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

// Veritabanını başlat ve admin kullanıcısını oluştur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı başlatılırken bir hata oluştu.");
    }
}

app.Run();
