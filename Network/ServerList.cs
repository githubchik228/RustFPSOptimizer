namespace RustFPSOptimizer.Network;
public class RustServerEndpoint
{
    public string Region { get; set; } = "";
    public string Name { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 28015;
    public string Address =>
        $"{Host}:{Port}";
}
public static class ServerList
{
    public static IReadOnlyList<RustServerEndpoint>
        Servers { get; } =
        new List<RustServerEndpoint>
        {
            new()
            {
                Region = "Europe",
                Name = "Europe Test",
                Host = "1.1.1.1",
                Port = 28015
            },
            new()
            {
                Region = "North America",
                Name = "North America Test",
                Host = "1.0.0.1",
                Port = 28015
            },
            new()
            {
                Region = "Asia",
                Name = "Asia Test",
                Host = "8.8.8.8",
                Port = 28015
            },
            new()
            {
                Region = "Custom",
                Name = "Custom Server",
                Host = "",
                Port = 28015
            }
        };
}
