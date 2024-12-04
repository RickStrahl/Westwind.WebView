using Microsoft.Extensions.FileProviders;
using Westwind.WebView.HtmlToPdf;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ***IMPORTANT!***
// Initialize Server so that WebView can render - required only for non-desktop environments/services/IIS
HtmlToPdfDefaults.ServerPreInitialize(  Path.Combine(builder.Environment.ContentRootPath,"WebViewEnvironment") );

var app = builder.Build();

// Configure the HTTP request pipeline.

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();