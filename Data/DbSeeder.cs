using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Models.Entities;

namespace TaskManagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleMgr = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var ctx = sp.GetRequiredService<ApplicationDbContext>();

        string[] roles = { "Admin", "Client" };
        foreach (var r in roles)
        {
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new ApplicationRole { Name = r, Description = $"{r} role" });
        }

        var perms = new[]
        {
            ("users.manage","Users"),("roles.manage","Roles"),("projects.manage","Projects"),
            ("projects.view","Projects"),("tasks.manage","Tasks"),("tasks.own","Tasks"),
            ("kanban.global","Kanban"),("kanban.personal","Kanban"),("reports.full","Reports"),
            ("reports.personal","Reports"),("audit.view","Audit"),("config.manage","Config")
        };
        foreach (var (name, mod) in perms)
        {
            if (!await ctx.Permissions.AnyAsync(p => p.Name == name))
                ctx.Permissions.Add(new Permission { Name = name, Module = mod, Description = name });
        }
        await ctx.SaveChangesAsync();

        var adminRole = await roleMgr.FindByNameAsync("Admin");
        var clientRole = await roleMgr.FindByNameAsync("Client");
        var allPerms = await ctx.Permissions.ToListAsync();

        foreach (var p in allPerms)
        {
            if (!await ctx.RolePermissions.AnyAsync(x => x.RoleId == adminRole!.Id && x.PermissionId == p.Id))
                ctx.RolePermissions.Add(new RolePermission { RoleId = adminRole!.Id, PermissionId = p.Id });
        }
        foreach (var p in allPerms.Where(p => p.Name is "tasks.own" or "kanban.personal" or "reports.personal" or "projects.view"))
        {
            if (!await ctx.RolePermissions.AnyAsync(x => x.RoleId == clientRole!.Id && x.PermissionId == p.Id))
                ctx.RolePermissions.Add(new RolePermission { RoleId = clientRole!.Id, PermissionId = p.Id });
        }
        await ctx.SaveChangesAsync();

        var admin = await userMgr.FindByEmailAsync("admin@admin.com");
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                EmailConfirmed = true,
                FullName = "System Administrator",
                IsActive = true
            };
            await userMgr.CreateAsync(admin, "Admin123");
            await userMgr.AddToRoleAsync(admin, "Admin");
        }

        var client = await userMgr.FindByEmailAsync("client@client.com");
        if (client == null)
        {
            client = new ApplicationUser
            {
                UserName = "client@client.com",
                Email = "client@client.com",
                EmailConfirmed = true,
                FullName = "Demo Client",
                IsActive = true
            };
            await userMgr.CreateAsync(client, "Client123");
            await userMgr.AddToRoleAsync(client, "Client");
        }
    }
}
