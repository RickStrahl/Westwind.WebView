using Westwind.WebView.HtmlToPdf;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


// ***IMPORTANT!***
// Initialize Server so that WebView can initialize even if it fails
HtmlToPdfHost.ServerPreInitialize(  Path.Combine(builder.Environment.ContentRootPath,"WebViewEnvironment") );


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();