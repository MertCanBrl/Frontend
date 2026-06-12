using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Backend.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // .env dosyasını kök dizinden yükle
        DotNetEnv.Env.Load("../.env");

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}