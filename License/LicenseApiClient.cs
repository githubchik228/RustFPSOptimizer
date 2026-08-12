using System.Net.Http;
using System.Net.Http.Json;
namespace RustFPSOptimizer.License;
public class LicenseApiClient
{
    private readonly HttpClient client;
    public LicenseApiClient(
        string serverUrl)
    {
        client = new HttpClient
        {
            BaseAddress =
                new Uri(
                    serverUrl.TrimEnd('/') + "/")
        };
        client.Timeout =
            TimeSpan.FromSeconds(10);
    }
    public async Task<
        ServerActivationResponse?>
        ActivateAsync(
            string key)
    {
        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "api/license/activate",
                    new
                    {
                        key
                    });
            return await response.Content
                .ReadFromJsonAsync<
                    ServerActivationResponse>();
        }
        catch
        {
            return null;
        }
    }
    public async Task<
        ServerValidationResponse?>
        ValidateAsync(
            string key)
    {
        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "api/license/validate",
                    new
                    {
                        key
                    });
            return await response.Content
                .ReadFromJsonAsync<
                    ServerValidationResponse>();
        }
        catch
        {
            return null;
        }
    }
}
public class ServerActivationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? Role { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
public class ServerValidationResponse
{
    public bool Valid { get; set; }
    public string Message { get; set; } = "";
    public string? Role { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
