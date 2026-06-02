using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models.Entities;

namespace TaskManagement.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    IQueryable<T> Query();
}

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _ctx;
    protected readonly DbSet<T> _set;
    public GenericRepository(ApplicationDbContext ctx) { _ctx = ctx; _set = ctx.Set<T>(); }
    public async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> p) => await _set.Where(p).ToListAsync();
    public async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();
    public async Task<T?> GetByIdAsync(object id) => await _set.FindAsync(id);
    public IQueryable<T> Query() => _set.AsQueryable();
    public void Remove(T entity) => _set.Remove(entity);
    public void Update(T entity) => _set.Update(entity);
}

public interface IUnitOfWork
{
    Task<int> SaveAsync();
}
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _ctx;
    public UnitOfWork(ApplicationDbContext ctx) => _ctx = ctx;
    public Task<int> SaveAsync() => _ctx.SaveChangesAsync();
}

public interface IUserRepository : IGenericRepository<ApplicationUser> { }
public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
{ public UserRepository(ApplicationDbContext c) : base(c) { } }

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<Project?> GetWithDetailsAsync(int id);
}
public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext c) : base(c) { }
    public Task<Project?> GetWithDetailsAsync(int id) =>
        _set.Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks).ThenInclude(t => t.AssignedTo)
            .FirstOrDefaultAsync(p => p.Id == id);
}

public interface ITaskRepository : IGenericRepository<TaskItem>
{
    Task<TaskItem?> GetWithDetailsAsync(int id);
    Task<IEnumerable<TaskItem>> GetByUserAsync(string userId);
}
public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext c) : base(c) { }
    public Task<TaskItem?> GetWithDetailsAsync(int id) =>
        _set.Include(t => t.Project).Include(t => t.AssignedTo)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id);
    public async Task<IEnumerable<TaskItem>> GetByUserAsync(string userId) =>
        await _set.Include(t => t.Project)
            .Where(t => t.AssignedToId == userId || t.CreatedById == userId)
            .ToListAsync();
}
