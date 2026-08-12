namespace RustFPSOptimizer.License;
public enum LicenseDuration
{
    OneDay = 1,
    SevenDays = 7,
    ThirtyDays = 30,
    OneYear = 365,
    Lifetime = 0
}
public enum LicenseRole
{
    User,
    Helper,
    Admin,
    Owner
}
public class LicenseKey
{
    public string Key { get; set; } = string.Empty;
    public LicenseDuration Duration { get; set; }
    public LicenseRole Role { get; set; } = LicenseRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsLifetime =>
        Duration == LicenseDuration.Lifetime;
    public bool IsExpired =>
        !IsLifetime &&
        ExpiresAt.HasValue &&
        ExpiresAt.Value <= DateTime.UtcNow;
}
public class LicenseSession
{
    public string Key { get; set; } = string.Empty;
    public LicenseRole Role { get; set; } = LicenseRole.User;
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsLifetime =>
        !ExpiresAt.HasValue;
    public bool IsExpired =>
        ExpiresAt.HasValue &&
        ExpiresAt.Value <= DateTime.UtcNow;
}
public class LicenseInfo
{
    public string Key { get; set; } = string.Empty;
    public LicenseRole Role { get; set; } = LicenseRole.User;
    public DateTime? ExpiresAt { get; set; }
    public bool IsLifetime =>
        !ExpiresAt.HasValue;
    public bool IsExpired =>
        ExpiresAt.HasValue &&
        ExpiresAt.Value <= DateTime.UtcNow;
}
public class LicenseActivationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public LicenseInfo? License { get; set; }
    public DateTime? ExpiresAt =>
        License?.ExpiresAt;
    public LicenseRole Role =>
        License?.Role ?? LicenseRole.User;
}
public class LicenseCreateRequest
{
    public int Days { get; set; }
    public LicenseRole Role { get; set; } =
        LicenseRole.User;
}
public class LicenseActivateRequest
{
    public string Key { get; set; } =
        string.Empty;
}
