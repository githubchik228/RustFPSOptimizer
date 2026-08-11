namespace RustFPSOptimizer.License;
public enum LicenseRole
{
    User,
    Helper,
    Admin,
    Owner
}
public enum LicenseDuration
{
    OneDay,
    SevenDays,
    ThirtyDays,
    OneYear,
    Lifetime
}
public enum LicenseStatus
{
    Invalid,
    Active,
    Expired,
    Revoked
}
public class LicenseKey
{
    public string Key { get; set; } = "";
    public LicenseRole Role { get; set; } =
        LicenseRole.User;
    public LicenseDuration Duration { get; set; } =
        LicenseDuration.ThirtyDays;
    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public string CreatedBy { get; set; } = "";
    public LicenseStatus Status
    {
        get
        {
            if (Revoked)
                return LicenseStatus.Revoked;
            if (Duration == LicenseDuration.Lifetime)
                return LicenseStatus.Active;
            if (!ExpiresAt.HasValue)
                return LicenseStatus.Invalid;
            return DateTime.UtcNow < ExpiresAt.Value
                ? LicenseStatus.Active
                : LicenseStatus.Expired;
        }
    }
    public bool IsActive =>
        Status == LicenseStatus.Active;
}
public class LicenseSession
{
    public string Key { get; set; } = "";
    public LicenseRole Role { get; set; } =
        LicenseRole.User;
    public DateTime ActivatedAt { get; set; } =
        DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsLifetime { get; set; }
    public bool IsExpired =>
        !IsLifetime &&
        (!ExpiresAt.HasValue ||
         DateTime.UtcNow >= ExpiresAt.Value);
}
