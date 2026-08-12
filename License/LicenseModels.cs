namespace RustFPSOptimizer.License;

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
