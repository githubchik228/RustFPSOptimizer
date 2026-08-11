namespace RustFPSOptimizer.Network;
public class NetworkDiagnostics
{
    private readonly PingService pingService = new();
    public async Task<(long? ping, double packetLoss)>
        TestAsync(
            string host,
            int count = 5)
    {
        int lost = 0;
        List<long> results = new();
        for (int i = 0; i < count; i++)
        {
            long? result =
                await pingService.PingAsync(host);
            if (result.HasValue)
                results.Add(result.Value);
            else
                lost++;
            await Task.Delay(100);
        }
        if (results.Count == 0)
            return (null, 100);
        double loss =
            lost * 100.0 / count;
        return (
            (long)results.Average(),
            loss
        );
    }
}
