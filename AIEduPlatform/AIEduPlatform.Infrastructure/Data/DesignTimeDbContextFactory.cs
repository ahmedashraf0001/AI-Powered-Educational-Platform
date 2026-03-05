using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AIEduPlatform.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "AIEduPlatform.Api");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.Development.json"), optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection"),
            o => o.UseVector());

        return new AppDbContext(optionsBuilder.Options);
    }
}
