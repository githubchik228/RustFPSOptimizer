namespace RustFPSOptimizer.License;

public class LicenseInfo
{
    public string Key { get; set; } = string.Empty;

    public LicenseRole Role { get; set; } =
        LicenseRole.User;

    public DateTime? ExpiresAt { get; set; }

    public bool IsLifetime =>
        !ExpiresAt.HasValue;

    public bool IsExpired =>
        ExpiresAt.HasValue &&
        ExpiresAt.Value <= DateTime.UtcNow;
}
