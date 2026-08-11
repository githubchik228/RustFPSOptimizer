namespace RustFPSOptimizer.License;
public class LicenseInfo
{
    public string KeyId { get; set; } = "";
    public LicenseRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public int MaxActivations { get; set; } = 1;
    public int CurrentActivations { get; set; }
    public bool IsExpired =>
        ExpiresAt.HasValue &&
        DateTime.UtcNow >= ExpiresAt.Value;
    public bool IsValid =>
        !Revoked &&
        !IsExpired &&
        CurrentActivations < MaxActivations;
}
