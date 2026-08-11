using System.Net.NetworkInformation;
namespace RustFPSOptimizer.Network;
public class PingService
{
    public async Task<long?> PingAsync(
        string host,
        int timeout = 2000)
    {
        if (string.IsNullOrWhiteSpace(host))
            return null;
        try
        {
            using Ping ping = new();
            PingReply reply =
                await ping.SendPingAsync(
                    host,
                    timeout);
            if (reply.Status !=
                IPStatus.Success)
            {
                return null;
            }
            return reply.RoundtripTime;
        }
        catch
        {
            return null;
        }
    }
    public async Task<List<long>> RunTestAsync(
        string host,
        int count = 10,
        int timeout = 2000)
    {
        List<long> results = new();
        count =
            Math.Clamp(
                count,
                1,
                100);
        for (int i = 0;
             i < count;
             i++)
        {
            long? result =
                await PingAsync(
                    host,
                    timeout);
            if (result.HasValue)
                results.Add(result.Value);
            await Task.Delay(100);
        }
        return results;
    }
}
