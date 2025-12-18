using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Tienda.src.Domain.Models;
using Tienda.src.Infrastructure.Data;
using Tienda.src.Interfaces;
using Tienda.src.Repositories;
using Tienda.src.Services;

var builder = WebApplication.CreateBuilder(args);
#region Logging Configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
#endregion

builder.Services.AddOpenApi();

#region Identity Configuration
Log.Information("Configurando Identity");
builder.Services.AddIdentityCore<User>(options =>
{
    //Configuración de contraseña
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;

    //Configuración de Email
    options.User.RequireUniqueEmail = true;

    //Configuración de UserName
    options.User.AllowedUserNameCharacters = builder.Configuration["IdentityConfiguration:AllowedUserNameCharacters"] ?? throw new InvalidOperationException("Los caracteres permitidos para UserName no están configurados.");
})
.AddRoles<Role>()

.AddEntityFrameworkStores<DataContext>();
#endregion

#region Database Configuration
Log.Information("Configurando base de datos SQLite");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetSection("ConnectionStrings:SqliteDatabase").Value ?? throw new InvalidOperationException("La cadena de conexión para la base de datos SQLite no está configurada.")));
#endregion

#region Dependency Injection
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
#endregion


var app = builder.Build();

app.MapOpenApi();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataSeeder.Initialize(services);
}

app.Run();
