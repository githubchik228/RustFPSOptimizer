namespace RustFPSOptimizer.Core;
public class RestoreManager
{
    private readonly ChangeTracker tracker;
    public RestoreManager(ChangeTracker tracker)
    {
        this.tracker = tracker;
    }
    public void RestoreAll()
    {
        foreach (ChangeRecord change in tracker.Changes)
        {
            if (change.Restored)
                continue;
            // Реальные restore-действия подключим
            // вместе с соответствующими твиками.
            tracker.MarkRestored(change.Id);
        }
    }
}
