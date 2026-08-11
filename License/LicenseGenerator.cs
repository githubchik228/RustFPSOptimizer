using System.Security.Cryptography;
namespace RustFPSOptimizer.License;
public class LicenseGenerator
{
    public LicenseKey Generate(
        LicenseRole role,
        LicenseDuration duration,
        string createdBy)
    {
        if (role == LicenseRole.Owner)
            throw new InvalidOperationException(
                "OWNER keys cannot be generated here.");
        DateTime created =
            DateTime.UtcNow;
        DateTime? expires =
            duration switch
            {
                LicenseDuration.OneDay =>
                    created.AddDays(1),
                LicenseDuration.SevenDays =>
                    created.AddDays(7),
                LicenseDuration.ThirtyDays =>
                    created.AddDays(30),
                LicenseDuration.OneYear =>
                    created.AddYears(1),
                LicenseDuration.Lifetime =>
                    null,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(duration))
            };
        return new LicenseKey
        {
            Key = CreateKey(),
            Role = role,
            Duration = duration,
            CreatedAt = created,
            ExpiresAt = expires,
            CreatedBy = createdBy
        };
    }
    private static string CreateKey()
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
