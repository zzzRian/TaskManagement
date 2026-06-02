using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TaskManagement.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseMySql(
            "Server=localhost;Port=3306;Database=TaskManagementDb;User=root;Password=CHANGE_ME;",
            new MySqlServerVersion(new Version(8, 0, 42))
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}