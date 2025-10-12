using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tienda.src.Domain.Models;

namespace Tienda.src.Infrastructure.Data
{
    public class DataSeeder
    {
        /// <summary>
        /// Método para inicializar la base de datos con datos de prueba.
        /// </summary>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<DataContext>();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

                await context.Database.EnsureCreatedAsync();
                await context.Database.MigrateAsync();

                if (!context.Roles.Any())
                {
                    var roles = new List<Role>
                    {
                        new Role { Name = "Admin", NormalizedName = "ADMIN" },
                        new Role { Name = "Customer", NormalizedName = "CUSTOMER" }
                    };

                    foreach (var role in roles)
                    {
                        var result = await roleManager.CreateAsync(role);
                        if (!result.Succeeded)
                        {
                            Log.Error("Error creando rol {RoleName}: {Errors}", 
                                role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                            throw new InvalidOperationException($"No se pudo crear el rol {role.Name}.");
                        }
                    }

                    Log.Information("Roles creados con éxito.");
                }

                // Crear categorías
                if (!context.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Electronics" },
                        new Category { Name = "Clothing" },
                        new Category { Name = "Home Appliances" },
                        new Category { Name = "Books" },
                        new Category { Name = "Sports" }
                    };

                    await context.Categories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                    Log.Information("Categorías creadas con éxito.");
                }

                // 🟧 Crear marcas
                if (!await context.Brands.AnyAsync())
                {
                    var brands = new List<Brand>
                    {
                        new Brand { Name = "Sony" },
                        new Brand { Name = "Apple" },
                        new Brand { Name = "HP" }
                    };

                    await context.Brands.AddRangeAsync(brands);
                    await context.SaveChangesAsync();
                    Log.Information("Marcas creadas con éxito.");
                }

                // 🟪 Crear usuarios
                if (!await context.Users.AnyAsync())
                {
                    Role customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer") 
                        ?? throw new InvalidOperationException("El rol de cliente no está configurado.");
                    Role adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin") 
                        ?? throw new InvalidOperationException("El rol de administrador no está configurado.");

                    // 👑 Crear usuario administrador
                    User adminUser = new User
                    {
                        FirstName = configuration["User:AdminUser:FirstName"] ?? "Admin",
                        LastName = configuration["User:AdminUser:LastName"] ?? "User",
                        Email = configuration["User:AdminUser:Email"] ?? "admin@tienda.com",
                        EmailConfirmed = true,
                        Gender = Gender.Masculino,
                        Rut = configuration["User:AdminUser:Rut"] ?? "12345678-9",
                        BirthDate = DateTime.Parse(configuration["User:AdminUser:BirthDate"] ?? "1990-01-01"),
                        PhoneNumber = configuration["User:AdminUser:PhoneNumber"] ?? "+56912345678",
                        UserName = configuration["User:AdminUser:Email"] ?? "admin@tienda.com"
                    };

                    var adminPassword = configuration["User:AdminUser:Password"] ?? "Admin123!";
                    var adminResult = await userManager.CreateAsync(adminUser, adminPassword);

                    if (adminResult.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(adminUser, adminRole.Name!);
                        if (!roleResult.Succeeded)
                        {
                            Log.Error("Error asignando rol de administrador: {Errors}", 
                                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                            throw new InvalidOperationException("No se pudo asignar el rol de administrador.");
                        }
                        Log.Information("Usuario administrador creado con éxito.");
                    }

                    // 👥 Crear usuarios aleatorios
                    var randomPassword = configuration["User:RandomUserPassword"] ?? "User123!";
                    var userFaker = new Faker<User>()
                        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                        .RuleFor(u => u.LastName, f => f.Name.LastName())
                        .RuleFor(u => u.Email, f => f.Internet.Email())
                        .RuleFor(u => u.EmailConfirmed, f => true)
                        .RuleFor(u => u.Gender, f => f.PickRandom<Gender>())
                        .RuleFor(u => u.Rut, f => RandomRut())
                        .RuleFor(u => u.BirthDate, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
                        .RuleFor(u => u.PhoneNumber, f => RandomPhoneNumber())
                        .RuleFor(u => u.UserName, (f, u) => u.Email);

                    var users = userFaker.Generate(99);

                    foreach (var user in users)
                    {
                        var result = await userManager.CreateAsync(user, randomPassword);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, customerRole.Name!);
                        }
                        else
                        {
                            Log.Warning("Error creando usuario {Email}: {Errors}", 
                                user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }

                    Log.Information("Usuarios creados con éxito.");
                }

                // 🧾 Crear productos e imágenes
                if (!await context.Products.AnyAsync())
                {
                    var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();
                    var brandIds = await context.Brands.Select(b => b.Id).ToListAsync();

                    if (categoryIds.Any() && brandIds.Any())
                    {
                        var productFaker = new Faker<Product>()
                            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
                            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                            .RuleFor(p => p.Price, f => f.Random.Int(1000, 100000))
                            .RuleFor(p => p.Stock, f => f.Random.Int(1, 100))
                            .RuleFor(p => p.CategoryId, f => f.PickRandom(categoryIds))
                            .RuleFor(p => p.BrandId, f => f.PickRandom(brandIds))
                            .RuleFor(p => p.Status, f => f.PickRandom<Status>());

                        var products = productFaker.Generate(50);
                        await context.Products.AddRangeAsync(products);
                        await context.SaveChangesAsync();
                        Log.Information("Productos creados con éxito.");
                    }

                    if (!await context.Images.AnyAsync())
                    {
                        var productIds = await context.Products.Select(p => p.Id).ToListAsync();
                        var imageFaker = new Faker<Image>()
                            .RuleFor(i => i.ImageUrl, f => f.Image.PicsumUrl())
                            .RuleFor(i => i.PublicId, f => f.Random.Guid().ToString())
                            .RuleFor(i => i.ProductId, f => f.PickRandom(productIds));

                        var images = imageFaker.Generate(20);
                        await context.Images.AddRangeAsync(images);
                        await context.SaveChangesAsync();
                        Log.Information("Imágenes creadas con éxito.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al inicializar la base de datos: {Message}", ex.Message);
            }
        }

        //  Generar RUT aleatorio
        private static string RandomRut()
        {
            var faker = new Faker();
            var rut = faker.Random.Int(1000000, 99999999).ToString();
            var dv = faker.Random.Int(0, 9).ToString();
            return $"{rut}-{dv}";
        }

        //  Generar número de teléfono aleatorio
        private static string RandomPhoneNumber()
        {
            var faker = new Faker();
            string firstPart = faker.Random.Int(1000, 9999).ToString();
            string secondPart = faker.Random.Int(1000, 9999).ToString();
            return $"+569 {firstPart}{secondPart}";
        }
    }
}
