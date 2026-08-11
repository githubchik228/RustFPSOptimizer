namespace RustFPSOptimizer.Core;
public enum OptimizationProfile
{
    MaxFps,
    Competitive,
    Balanced,
    Quality
}
public class OptimizationProfileService
{
    private readonly WindowsTweaks tweaks;
    public OptimizationProfileService(
        ChangeTracker tracker)
    {
        tweaks =
            new WindowsTweaks(
                tracker);
    }
    public void Apply(
        OptimizationProfile profile)
    {
        switch (profile)
        {
            case OptimizationProfile.MaxFps:
                ApplyMaxFps();
                break;
            case OptimizationProfile.Competitive:
                ApplyCompetitive();
                break;
            case OptimizationProfile.Balanced:
                ApplyBalanced();
                break;
            case OptimizationProfile.Quality:
                ApplyQuality();
                break;
        }
    }
    private void ApplyMaxFps()
    {
        tweaks.ApplySafeGamingProfile();
    }
    private void ApplyCompetitive()
    {
        tweaks.ApplySafeGamingProfile();
    }
    private void ApplyBalanced()
    {
        tweaks.EnableGameMode();
    }
    private void ApplyQuality()
    {
        tweaks.EnableGameMode();
    }
}
