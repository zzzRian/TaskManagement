using TaskManagement.Services;

namespace TaskManagement.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _log;
    public ExceptionMiddleware(RequestDelegate n, ILogger<ExceptionMiddleware> l) { _next = n; _log = l; }
    public async Task Invoke(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception");
            ctx.Response.Redirect("/Home/Error");
        }
    }
}

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    public AuditMiddleware(RequestDelegate n) => _next = n;
    public async Task Invoke(HttpContext ctx, IAuditService audit)
    {
        await _next(ctx);
        if (ctx.User.Identity?.IsAuthenticated == true &&
            HttpMethods.IsPost(ctx.Request.Method) &&
            ctx.Response.StatusCode < 400)
        {
            try { await audit.LogAsync(ctx.Request.Method, ctx.Request.Path, null, ctx.Response.StatusCode.ToString()); }
            catch { }
        }
    }
}
