using System.Net;
using System.Net.Sockets;

namespace ServiceShared.Utils;

public static class PortFinder
{
  public static int FindFreePort(int startingPort, int maxPort = 65535)
  {
    if (startingPort > maxPort) throw new ArgumentException("Starting port cannot be greater than the maximum port.");
    for (var port = startingPort; port <= maxPort; port++)
    {
      if (IsPortAvailable(port))
        return port;
    }
    throw new Exception("No available port found in the given range.");
  }

  private static bool IsPortAvailable(int port)
  {
    try
    {
      var listener = new TcpListener(IPAddress.Loopback, port);
      listener.Start();
      listener.Stop();
      return true;
    }
    catch (SocketException)
    {
      return false;
    }
  }
}