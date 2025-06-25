using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SparkerSystemService.Utils;

public static class NetworkUtils
{
  public static bool IsLocalNetwork(IPAddress ipAddress)
  {
    if (IPAddress.IsLoopback(ipAddress))
    {
      return true;
    }

    var bytes = ipAddress.GetAddressBytes();

    // IPv4 checking
    if (ipAddress.AddressFamily != AddressFamily.InterNetwork) return false;
    switch (bytes[0])
    {
      // 10.0.0.0/8
      case 10:
      // 172.16.0.0/12
      case 172 when bytes[1] >= 16 && bytes[1] <= 31:
        return true;
    }

    // 192.168.0.0/16
    return bytes[0] == 192 && bytes[1] == 168;
  }

  public static int GetAvailablePort(int startPort)
  {
    const int maxPort = 65535;
    for (var port = startPort; port <= maxPort; port++)
    {
      if (!IsPortInUse(port))
      {
        return port;
      }
    }
    throw new InvalidOperationException("No available ports found.");
  }

  private static bool IsPortInUse(int port)
  {
    try
    {
      var listener = new TcpListener(IPAddress.Loopback, port);
      listener.Start();
      listener.Stop();
      return false;
    }
    catch (SocketException)
    {
      return true;
    }
  }

  public static string? GetEthernetMacAddress()
  {
    var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
    var plainMacAddress = networkInterfaces.ToList().Find(item => item.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
      ?.GetPhysicalAddress().ToString()!;

    // AABBCC -> AA:BB:CC
    string FormatMacAddress(string macAddress)
    {
      return string.Join(":", macAddress.ToUpper().ToCharArray()
        .Select((value, index) => new { value, index })
        .Aggregate(new List<string>(), (result, item) =>
        {
          if (item.index % 2 == 0)
          {
            result.Add(item.value.ToString());
          }
          else
          {
            result[^1] = result.Last() + item.value;
          }
          
          return result;
        }));
    }

    return FormatMacAddress(plainMacAddress);
  }
}
