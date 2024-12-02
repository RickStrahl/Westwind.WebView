using Microsoft.Extensions.FileProviders;
using Westwind.WebView.HtmlToPdf;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Physical File Provider
builder.Services.AddSingleton<IFileProvider>(
    new PhysicalFileProvider(Directory.GetCurrentDirectory()));

// ***IMPORTANT!***
// Initialize Server so that WebView can initialize even if it fails
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