using Microsoft.EntityFrameworkCore;
namespace LicenseServer;
public class LicenseDb : DbContext
{
    public LicenseDb(
        DbContextOptions<LicenseDb> options)
        : base(options)
    {
    }
    public DbSet<ServerLicense> Licenses =>
        Set<ServerLicense>();
    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        builder.Entity<ServerLicense>()
            .HasIndex(x => x.Key)
            .IsUnique();
        builder.Entity<ServerLicense>()
            .Property(x => x.Key)
            .IsRequired();
    }
}
public class ServerLicense
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Role { get; set; } =
        "User";
    public string Duration { get; set; } =
        "ThirtyDays";
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime? LastActivatedAt { get; set; }
}
