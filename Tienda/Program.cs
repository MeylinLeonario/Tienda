using Tienda.src.Domain.Models;
using Tienda.src.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
#region Logging Configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
#endregion

builder.Services.AddOpenApi();

#region Identity Configuration
Log.Information("Configurando Identity");
builder.Services.AddIdentity<User, Role>(options =>
{
    //Configuración contraseña
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;

    // Configuración email
    options.User.RequireUniqueEmail = true;

    //Configuración de UserName
    options.User.AllowedUserNameCharacters = builder.Configuration["IdentityConfiguration: AllowedUserNameCharacters"] ?? throw new InvalidOperationException();
})

.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();
#endregion

#region Database Configuration
Log.Information("Configurando base de datos SQLite");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetSection("ConnectionStrings:SqliteDatabase").Value));
#endregion

var app = builder.Build();
var _logger = app.Services.GetRequiredService<ILogger<Program>>();

_logger.LogInformation("Application is starting...");

app.MapOpenApi();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataSeeder.Initialize(services);
}

app.Run();
