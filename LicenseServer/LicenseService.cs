using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
namespace LicenseServer;
public class LicenseService
{
    private readonly LicenseDb db;
    public LicenseService(
        LicenseDb db)
    {
        this.db = db;
    }
    public async Task<
        LicenseActivationResult>
        ActivateAsync(
            string key)
    {
        ServerLicense? license =
            await db.Licenses
                .FirstOrDefaultAsync(
                    x => x.Key == key.Trim());
        if (license == null)
            return Fail(
                "Invalid license key.");
        if (license.Revoked)
            return Fail(
                "License revoked.");
        if (IsExpired(license))
            return Fail(
                "License expired.");
        license.LastActivatedAt =
            DateTime.UtcNow;
        await db.SaveChangesAsync();
        return new LicenseActivationResult
        {
            Success = true,
            Message = "License activated.",
            Role = license.Role,
            ExpiresAt = license.ExpiresAt
        };
    }
    public async Task<
        LicenseValidationResult>
        ValidateAsync(
            string key)
    {
        ServerLicense? license =
            await db.Licenses
                .FirstOrDefaultAsync(
                    x => x.Key == key.Trim());
        if (license == null)
            return Invalid(
                "License not found.");
        if (license.Revoked)
            return Invalid(
                "License revoked.");
        if (IsExpired(license))
            return Invalid(
                "License expired.");
        return new LicenseValidationResult
        {
            Valid = true,
            Message = "License active.",
            Role = license.Role,
            ExpiresAt = license.ExpiresAt
        };
    }
    public async Task<
        ServerLicense>
        CreateAsync(
            string role,
            string duration,
            string createdBy)
    {
        DateTime now =
            DateTime.UtcNow;
        DateTime? expires =
            duration switch
            {
                "OneDay" =>
                    now.AddDays(1),
                "SevenDays" =>
                    now.AddDays(7),
                "ThirtyDays" =>
                    now.AddDays(30),
                "OneYear" =>
                    now.AddYears(1),
                "Lifetime" =>
                    null,
                _ => throw new ArgumentException(
                    "Invalid duration.")
            };
        ServerLicense license =
            new()
            {
                Key = GenerateKey(),
                Role = role,
                Duration = duration,
                CreatedAt = now,
                ExpiresAt = expires,
                CreatedBy = createdBy
            };
        db.Licenses.Add(license);
        await db.SaveChangesAsync();
        return license;
    }
    private static bool IsExpired(
        ServerLicense license)
    {
        return license.ExpiresAt.HasValue &&
               DateTime.UtcNow >=
               license.ExpiresAt.Value;
    }
    private static LicenseActivationResult
        Fail(string message)
    {
        return new LicenseActivationResult
        {
            Success = false,
            Message = message
        };
    }
    private static LicenseValidationResult
        Invalid(string message)
    {
        return new LicenseValidationResult
        {
            Valid = false,
            Message = message
        };
    }
    private static string GenerateKey()
    {
        byte[] bytes =
            RandomNumberGenerator.GetBytes(18);
        string value =
            Convert.ToHexString(bytes);
        return
            $"UNDEQ-{value[..6]}-" +
            $"{value[6..12]}-" +
            $"{value[12..18]}";
    }
}
public class LicenseActivationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? Role { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
public class LicenseValidationResult
{
    public bool Valid { get; set; }
    public string Message { get; set; } = "";
    public string? Role { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
