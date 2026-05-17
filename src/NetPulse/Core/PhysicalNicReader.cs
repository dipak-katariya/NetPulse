using System.Net.NetworkInformation;

namespace NetPulse.Core;

public sealed class PhysicalNicReader : INicReader
{
    private static readonly string[] VirtualMarkers =
    {
        "virtual", "hyper-v", "vmware", "vbox", "virtualbox",
        "wsl", "loopback", "pseudo", "tap-", "tunnel", "miniport",
        "bluetooth", "teredo", "isatap"
    };

    public (long BytesReceived, long BytesSent) ReadTotals()
    {
        long totalDown = 0;
        long totalUp = 0;

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up) continue;
            if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;
            if (LooksVirtual(nic.Description) || LooksVirtual(nic.Name)) continue;

            IPInterfaceStatistics stats;
            try
            {
                stats = nic.GetIPStatistics();
            }
            catch (NetworkInformationException)
            {
                continue;
            }

            totalDown += stats.BytesReceived;
            totalUp += stats.BytesSent;
        }

        return (totalDown, totalUp);
    }

    private static bool LooksVirtual(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (string marker in VirtualMarkers)
        {
            if (text.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
