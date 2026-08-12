namespace RustFPSOptimizer.License;

public class LicenseKey
{
    public string Key { get; set; } = string.Empty;

    public LicenseRole Role { get; set; } = LicenseRole.User;

    public DateTime? ExpiresAt { get; set; }

    public bool IsLifetime =>
        !ExpiresAt.HasValue;
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

public class LicenseActivationResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public LicenseRole Role { get; set; } = LicenseRole.User;

    public DateTime? ExpiresAt { get; set; }
}

public class LicenseValidationResult
{
    public bool Valid { get; set; }

    public string Message { get; set; } = string.Empty;

    public LicenseRole Role { get; set; } = LicenseRole.User;

    public DateTime? ExpiresAt { get; set; }
}

public class LicenseCreateRequest
{
    public int Days { get; set; }

    public LicenseRole Role { get; set; } = LicenseRole.User;
}

public class LicenseActivateRequest
{
    public string Key { get; set; } = string.Empty;
}

public class LicenseDuration
{
    public int Days { get; set; }

    public bool IsLifetime { get; set; }

    public DateTime? ExpiresAt { get; set; }
}

public class LicenseInfo
{
    public string Key { get; set; } = string.Empty;

    public LicenseRole Role { get; set; } = LicenseRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public bool IsLifetime =>
        !ExpiresAt.HasValue;
}
