using System.Net.NetworkInformation;
namespace RustFPSOptimizer.Network;
public class PingService
{
    public async Task<long?> PingAsync(
        string host,
        int timeout = 2000)
    {
        try
        {
            using Ping ping = new();
            PingReply reply =
                await ping.SendPingAsync(
                    host,
                    timeout);
            if (reply.Status != IPStatus.Success)
                return null;
            return reply.RoundtripTime;
        }
        catch
        {
            return null;
        }
    }
}
