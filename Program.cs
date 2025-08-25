using Microsoft.EntityFrameworkCore;
using PruebaTecnica2025.Data;
using PruebaTecnica2025.Services;
using PruebaTecnica2025.Services.Abstractions;
using PruebaTecnica2025.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Lee proveedor y cadena de conexión desde config/env
var provider = builder.Configuration["DatabaseProvider"] ?? "SQLite";
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

// EF Core: elige proveedor
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
    {
        // Opcional: compatibilidad de timestamps
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        opt.UseNpgsql(connStr);
    }
    else
    {
        opt.UseSqlite(connStr);
    }
});

builder.Services.AddScoped<IPublicationService, PublicationService>();

// CORS (Vite)
const string FrontendOrigin = "FrontendOrigin";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(FrontendOrigin, p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error applying migrations");
    }
}


app.UseCors(FrontendOrigin);

if (!app.Environment.IsDevelopment() &&
    (builder.Configuration.GetValue<bool>("UseHttpsRedirection") ||
     Environment.GetEnvironmentVariable("USE_HTTPS_REDIRECTION") == "true"))
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
