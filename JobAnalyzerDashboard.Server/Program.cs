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

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 64; // Varsayılan değer 32
    options.JsonSerializerOptions.WriteIndented = true;
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

// PostgreSQL veritabanı bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Veritabanı bağlantı dizesi: {connectionString}");
    options.UseNpgsql(connectionString);
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<JobAnalyzerDashboard.Server.Repositories.IJobRepository, JobAnalyzerDashboard.Server.Repositories.JobRepository>();
builder.Services.AddScoped<JobAnalyzerDashboard.Server.Repositories.IApplicationRepository, JobAnalyzerDashboard.Server.Repositories.ApplicationRepository>();
builder.Services.AddScoped<JobAnalyzerDashboard.Server.Repositories.IProfileRepository, JobAnalyzerDashboard.Server.Repositories.ProfileRepository>();

builder.Services.AddScoped<OAuthService>();
builder.Services.AddScoped<GmailApiService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuthService>();

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

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "application/pdf");
        }

        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

string wwwrootFolder = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootFolder))
{
    Directory.CreateDirectory(wwwrootFolder);
    Console.WriteLine($"wwwroot klasörü oluşturuldu: {wwwrootFolder}");
}
else
{
    Console.WriteLine($"wwwroot klasörü mevcut: {wwwrootFolder}");
    var files = Directory.GetFiles(wwwrootFolder, "*.*", SearchOption.AllDirectories);
    Console.WriteLine($"wwwroot içindeki dosya sayısı: {files.Length}");
    foreach (var file in files.Take(5))
    {
        Console.WriteLine($"Örnek dosya: {file}");
    }
}

string uploadsFolder = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "resumes");
if (!Directory.Exists(uploadsFolder))
{
    Directory.CreateDirectory(uploadsFolder);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream",
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "application/pdf");
        }

        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
        ctx.Context.Response.Headers.Append("Cross-Origin-Resource-Policy", "cross-origin");

        ctx.Context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self'");

        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

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

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    Console.WriteLine($"Using PORT environment variable: {port}");
    app.Urls.Clear();
    app.Urls.Add($"http://*:{port}");
    Console.WriteLine($"Application listening on: {string.Join(", ", app.Urls)}");
}

app.Run();
