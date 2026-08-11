using LicenseServer;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
string database =
    Environment.GetEnvironmentVariable("LICENSE_DB")
    ?? "Data Source=licenses.db";
builder.Services.AddDbContext<LicenseDb>(
    options =>
        options.UseSqlite(database));
builder.Services.AddScoped<LicenseService>();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "license",
        limiter =>
        {
            limiter.PermitLimit = 30;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        });
});
var app = builder.Build();
using (IServiceScope scope = app.Services.CreateScope())
{
    LicenseDb db =
        scope.ServiceProvider
            .GetRequiredService<LicenseDb>();
    db.Database.EnsureCreated();
}
app.UseRateLimiter();
app.MapGet("/", () =>
    Results.Ok(new
    {
        service = "Rust Performance Suite License Server",
        version = "1.0",
        status = "online"
    }));
app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "healthy",
        time = DateTime.UtcNow
    }));
app.MapPost(
    "/api/license/activate",
    async (
        ActivateRequest request,
        LicenseService service) =>
    {
        LicenseActivationResult result =
            await service.ActivateAsync(request.Key);
        return result.Success
            ? Results.Ok(result)
            : Results.BadRequest(result);
    })
    .RequireRateLimiting("license");
app.MapPost(
    "/api/license/validate",
    async (
        ValidateRequest request,
        LicenseService service) =>
    {
        LicenseValidationResult result =
            await service.ValidateAsync(request.Key);
        return result.Valid
            ? Results.Ok(result)
            : Results.BadRequest(result);
    })
    .RequireRateLimiting("license");
app.Run();
public record ActivateRequest(string Key);
public record ValidateRequest(string Key);
