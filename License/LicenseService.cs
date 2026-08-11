namespace RustFPSOptimizer.License;
public class LicenseService
{
    public LicenseInfo? CurrentLicense { get; private set; }
    public bool Activate(
        LicenseInfo license)
    {
        if (!license.IsValid)
            return false;
        CurrentLicense = license;
        return true;
    }
    public void Logout()
    {
        CurrentLicense = null;
    }
    public bool HasRole(LicenseRole role)
    {
        if (CurrentLicense == null)
            return false;
        return CurrentLicense.Role >= role;
    }
    public bool IsExpired =>
        CurrentLicense?.IsExpired ?? false;
    public bool IsOwner =>
        HasRole(LicenseRole.Owner);
    public bool IsAdmin =>
        HasRole(LicenseRole.Admin);
    public bool IsHelper =>
        HasRole(LicenseRole.Helper);
}
