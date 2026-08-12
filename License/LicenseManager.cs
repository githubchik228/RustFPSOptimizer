namespace RustFPSOptimizer.License;
public class LicenseManager
{
    private readonly LicenseStorage storage;
    private readonly LicenseApiClient api;
    public LicenseSession? CurrentSession
    {
        get;
        private set;
    }
    public bool ServerAvailable
    {
        get;
        private set;
    }
    public LicenseManager(
        string serverUrl)
    {
        storage =
            new LicenseStorage();
        api =
            new LicenseApiClient(
                serverUrl);
        CurrentSession =
            storage.LoadSession();
    }
    public bool IsLicensed =>
        CurrentSession != null &&
        !CurrentSession.IsExpired;
    public LicenseRole CurrentRole =>
        CurrentSession?.Role ??
        LicenseRole.User;
    public bool IsOwner =>
        IsLicensed &&
        CurrentRole == LicenseRole.Owner;
    public bool IsAdmin =>
        IsLicensed &&
        (CurrentRole == LicenseRole.Admin ||
         CurrentRole == LicenseRole.Owner);
    public bool IsHelper =>
        IsLicensed &&
        (CurrentRole == LicenseRole.Helper ||
         IsAdmin);
    public async Task<
        LicenseActivationResult>
        ActivateAsync(
            string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return new LicenseActivationResult
            {
                Success = false,
                Message = "Enter a license key."
            };
        }
        ServerActivationResponse? result =
            await api.ActivateAsync(
                key.Trim());
        if (result == null)
        {
            ServerAvailable = false;
            return new LicenseActivationResult
            {
                Success = false,
                Message =
                    "License server unavailable."
            };
        }
        ServerAvailable = true;
        if (!result.Success)
        {
            return new LicenseActivationResult
            {
                Success = false,
                Message = result.Message
            };
        }
        if (!Enum.TryParse<
                LicenseRole>(
                result.Role,
                true,
                out LicenseRole role))
        {
            return new LicenseActivationResult
            {
                Success = false,
                Message =
                    "Server returned an invalid role."
            };
        }
        CurrentSession =
            new LicenseSession
            {
                Key = key.Trim(),
                Role = role,
                ActivatedAt =
                    DateTime.UtcNow,
                ExpiresAt =
                    result.ExpiresAt,
                IsLifetime =
                    !result.ExpiresAt.HasValue
            };
        storage.SaveSession(
            CurrentSession);
        return new LicenseActivationResult
        {
            Success = true,
            Message = result.Message,
            Role = role,
            ExpiresAt = result.ExpiresAt
        };
    }
    public async Task<
        LicenseValidationResult>
        ValidateAsync()
    {
        if (CurrentSession == null)
        {
            return new LicenseValidationResult
            {
                Valid = false,
                Message = "No active license."
            };
        }
        ServerValidationResponse? result =
            await api.ValidateAsync(
                CurrentSession.Key);
        if (result == null)
        {
            ServerAvailable = false;
            return new LicenseValidationResult
            {
                Valid = false,
                Message =
                    "License server unavailable."
            };
        }
        ServerAvailable = true;
        if (!result.Valid)
        {
            CurrentSession = null;
            storage.ClearSession();
            return new LicenseValidationResult
            {
                Valid = false,
                Message = result.Message
            };
        }
        if (Enum.TryParse<
                LicenseRole>(
                result.Role,
                true,
                out LicenseRole role))
        {
            CurrentSession.Role = role;
        }
        CurrentSession.ExpiresAt =
            result.ExpiresAt;
        CurrentSession.IsLifetime =
            !result.ExpiresAt.HasValue;
        storage.SaveSession(
            CurrentSession);
        return new LicenseValidationResult
        {
            Valid = true,
            Message = result.Message,
            Role = CurrentSession.Role,
            ExpiresAt =
                CurrentSession.ExpiresAt
        };
    }
    public void Logout()
    {
        CurrentSession = null;
        storage.ClearSession();
    }
