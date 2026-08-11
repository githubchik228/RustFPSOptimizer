using System.Net.NetworkInformation;
namespace RustFPSOptimizer.Network;
public class NetworkTestResult
{
    public double AveragePing { get; set; }
    public long MinimumPing { get; set; }
    public long MaximumPing { get; set; }
    public double Jitter { get; set; }
    public double PacketLoss { get; set; }
    public int SuccessfulPackets { get; set; }
    public int TotalPackets { get; set; }
}
public class NetworkDiagnostics
{
    public async Task<NetworkTestResult> TestAsync(
        string host,
        int count = 10)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException(
                "Host is empty.");
        count =
            Math.Clamp(
                count,
                1,
                50);
        List<long> successful =
            new();
        using Ping ping = new();
        for (int i = 0; i < count; i++)
        {
            try
            {
                PingReply reply =
                    await ping.SendPingAsync(
                        host,
                        2000);
                if (reply.Status ==
                    IPStatus.Success)
                {
                    successful.Add(
                        reply.RoundtripTime);
                }
            }
            catch
            {
            }
            await Task.Delay(100);
        }
        if (successful.Count == 0)
        {
            return new NetworkTestResult
            {
                AveragePing = 0,
                MinimumPing = 0,
                MaximumPing = 0,
                Jitter = 0,
                PacketLoss = 100,
                SuccessfulPackets = 0,
                TotalPackets = count
            };
        }
        double average =
            successful.Average();
        double jitter =
            successful
                .Select(x =>
                    Math.Abs(x - average))
                .Average();
        return new NetworkTestResult
        {
            AveragePing = average,
            MinimumPing = successful.Min(),
            MaximumPing = successful.Max(),
            Jitter = jitter,
            PacketLoss =
                (count - successful.Count) *
                100.0 /
                count,
            SuccessfulPackets =
                successful.Count,
            TotalPackets =
                count
        };
    }
}
