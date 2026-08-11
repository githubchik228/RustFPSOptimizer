namespace RustFPSOptimizer.Network;
public class NetworkTestResult
{
    public long? AveragePing { get; set; }
    public long? MinimumPing { get; set; }
    public long? MaximumPing { get; set; }
    public double Jitter { get; set; }
    public double PacketLoss { get; set; }
}
public class NetworkDiagnostics
{
    private readonly PingService pingService =
        new();
    public async Task<NetworkTestResult>
        TestAsync(
            string host,
            int count = 10)
    {
        List<long> results = new();
        int lost = 0;
        for (int i = 0; i < count; i++)
        {
            long? ping =
                await pingService.PingAsync(
                    host);
            if (ping.HasValue)
                results.Add(ping.Value);
            else
                lost++;
            await Task.Delay(100);
        }
        if (results.Count == 0)
        {
            return new NetworkTestResult
            {
                PacketLoss = 100
            };
        }
        double average =
            results.Average();
        double jitter =
            results.Count > 1
                ? results
                    .Zip(
                        results.Skip(1),
                        (a, b) =>
                            Math.Abs(a - b))
                    .Average()
                : 0;
        return new NetworkTestResult
        {
            AveragePing =
                (long)average,
            MinimumPing =
                results.Min(),
            MaximumPing =
                results.Max(),
            Jitter = jitter,
            PacketLoss =
                lost * 100.0 / count
        };
    }
}
