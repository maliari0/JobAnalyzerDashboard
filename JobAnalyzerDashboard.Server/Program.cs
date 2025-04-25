using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// E-posta servisini ekle
builder.Services.AddSingleton<JobAnalyzerDashboard.Server.Services.EmailService>();

// CORS politikasını ekle - n8n entegrasyonu için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowN8n", policy =>
    {
        policy.WithOrigins("http://localhost:5678", "https://n8n-service-a2yz.onrender.com") // n8n'in çalıştığı adresler
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS middleware'ini etkinleştir
app.UseCors("AllowN8n");

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
