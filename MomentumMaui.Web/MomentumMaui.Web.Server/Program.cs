var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // When in development, the ASP.NET Core app should not handle static files.
    // Instead, it will proxy requests to the Vite development server.
    // The default template should have already configured this proxying.
    // This UseStaticFiles call is for production builds.
}
else
{
    // In production, serve the static files from the client app's build output.
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// This fallback is essential. It ensures that any request not handled by the API
// is passed to the client app. This allows client-side routing to work correctly.
app.MapFallbackToFile("/index.html");

app.Run();