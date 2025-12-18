using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Resend;
using Serilog;
using Tienda.src.Application.Services.Implements;
using Tienda.src.Application.Services.Interfaces;
using Tienda.src.Domain.Models;
using Tienda.src.Infrastructure.Data;
using Tienda.src.Interfaces;
using Tienda.src.Repositories;
using Tienda.src.Services;
using Tienda.src.Infrastructure.Middlewares;
using Tienda.src.Infrastructure.Repositories.Implements;
using Tienda.src.Infrastructure.Repositories.Interfaces;
var builder = WebApplication.CreateBuilder(args);

#region Logging Configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
#endregion

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.Configure<ResendClientOptions>(builder.Configuration.GetSection("Resend"));
builder.Services.AddHttpClient<ResendClient>();
builder.Services.AddScoped<IResend>(sp => sp.GetRequiredService<ResendClient>());
#endregion


var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataSeeder.Initialize(services);
}

app.Run();
