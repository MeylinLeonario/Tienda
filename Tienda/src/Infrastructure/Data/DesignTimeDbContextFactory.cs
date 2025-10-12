using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tienda.src.Infrastructure.Data
{
    // Esta clase le dice a EF cómo crear la base de datos en tiempo de diseño
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            // Le decimos a EF que use SQLite y que la base se llame app.db
            optionsBuilder.UseSqlite("Data Source=app.db");

            return new DataContext(optionsBuilder.Options);
        }
    }
}
