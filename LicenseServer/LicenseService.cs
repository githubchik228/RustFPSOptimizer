using RustFPSOptimizer.License;

using System.Security.Cryptography;
namespace LicenseServer;
public class LicenseService
{
    private readonly string filePath;
    private readonly object sync = new();
    public LicenseService()
    {
        filePath = Path.Combine(
            AppContext.BaseDirectory,
            "licenses.txt"
        );
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "");
        }
    }
    public LicenseActivationResult Activate(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new LicenseActivationResult
            {
                Success = false,
                Message = "License key is required."
            };
        }
        lock (sync)
        {
            var licenses = Load();
            var license = licenses.FirstOrDefault(
                x => x.Key.Equals(
                    key.Trim(),
                    StringComparison.OrdinalIgnoreCase)
            );
            if (license == null)
            {
                return new LicenseActivationResult
                {
                    Success = false,
                    Message = "Invalid license key."
                };
            }
            if (license.ExpiresAt != null &&
                license.ExpiresAt <= DateTime.UtcNow)
            {
                return new LicenseActivationResult
                {
                    Success = false,
                    Message = "License has expired."
                };
            }
            return new LicenseActivationResult
            {
                Success = true,
                Message = "License activated.",
                ExpiresAt = license.ExpiresAt,
                Role = license.Role
            };
        }
    }
    public LicenseInfo Create(
        int days,
        string role = "USER")
    {
        string key = GenerateKey();
        DateTime? expires = days <= 0
            ? null
            : DateTime.UtcNow.AddDays(days);
        var license = new LicenseInfo
        {
            Key = key,
            Role = role,
            ExpiresAt = expires
        };
        lock (sync)
        {
            var licenses = Load();
            licenses.Add(license);
            Save(licenses);
        }
        return license;
    }
    public List<LicenseInfo> GetAll()
    {
        lock (sync)
        {
            return Load();
        }
    }
    public bool Delete(string key)
    {
        lock (sync)
        {
            var licenses = Load();
            var license = licenses.FirstOrDefault(
                x => x.Key.Equals(
                    key,
                    StringComparison.OrdinalIgnoreCase)
            );
            if (license == null)
                return false;
            licenses.Remove(license);
            Save(licenses);
            return true;
        }
    }
    private List<LicenseInfo> Load()
    {
        var result = new List<LicenseInfo>();
        if (!File.Exists(filePath))
            return result;
        foreach (string line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            string[] parts = line.Split('|');
            if (parts.Length < 3)
                continue;
            DateTime? expires = null;
            if (!string.Equals(
                    parts[2],
                    "LIFETIME",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (DateTime.TryParse(
                    parts[2],
                    out DateTime parsed))
                {
                    expires = parsed;
                }
            }
            result.Add(
                new LicenseInfo
                {
                    Key = parts[0],
                    Role = parts[1],
                    ExpiresAt = expires
                });
        }
        return result;
    }
    private void Save(List<LicenseInfo> licenses)
    {
        var lines = licenses.Select(
            x =>
                $"{x.Key}|{x.Role}|" +
                $"{(x.ExpiresAt.HasValue ? x.ExpiresAt.Value.ToString("O") : "LIFETIME")}"
        );
        File.WriteAllLines(filePath, lines);
    }
    private static string GenerateKey()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(18);
        string value =
            Convert.ToHexString(bytes);
        return
            $"RFO-{value[..6]}-" +
            $"{value[6..12]}-" +
            $"{value[12..18]}";
    }
}
