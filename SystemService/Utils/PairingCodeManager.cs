using System.Collections.Concurrent;

namespace SparkerSystemService.Utils;

public static class PairingCodeManager
{
  private static readonly TimeSpan Life = TimeSpan.FromMinutes(5);
  private static readonly ConcurrentDictionary<string, string> Sessions = new();

  public static string Get(string sessionId)
  {
    var pairingCode = Generate();
    Sessions[sessionId] = pairingCode;

    Task.Run(async () =>
    {
      await Task.Delay(Life);
      Sessions.TryRemove(sessionId, out _);
    });

    return pairingCode;
  }

  public static bool Validate(string sessionId, string pairingCode)
  {
    if (!Sessions.TryGetValue(sessionId, out var stored)) return false;
    if (stored != pairingCode) return false;
    
    Sessions.TryRemove(sessionId, out var _);
    return true;
  }

  private static string Generate()
  {
    var random = new Random();
    var code = random.Next(1000, 10000).ToString();
    return code;
  }
}