using LicenseServer;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LicenseService>();
var app = builder.Build();
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        name = "Rust FPS Optimizer License Server",
        status = "online",
        version = "1.0.0"
    });
});
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy"
    });
});
app.MapPost("/api/license/activate",
    (ActivateRequest request,
     LicenseService service) =>
{
    var result =
        service.Activate(request.Key);
    return result.Success
        ? Results.Ok(result)
        : Results.BadRequest(result);
});
app.MapPost("/api/license/create",
    (CreateRequest request,
     LicenseService service) =>
{
    if (request.Days < 0)
    {
        return Results.BadRequest(
            new
            {
                message =
                    "Days cannot be negative."
            });
    }
    var license =
        service.Create(
            request.Days,
            request.Role);
    return Results.Ok(license);
});
app.MapGet("/api/license/list",
    (LicenseService service) =>
{
    return Results.Ok(
        service.GetAll());
});
app.MapDelete("/api/license/{key}",
    (string key,
     LicenseService service) =>
{
    bool deleted =
        service.Delete(key);
    return deleted
        ? Results.Ok(new
        {
            success = true
        })
        : Results.NotFound(new
        {
            success = false
        });
});
app.Run();
public class ActivateRequest
{
    public string Key { get; set; } = "";
}
public class CreateRequest
{
    public int Days { get; set; }
    public string Role { get; set; } = "USER";
}
