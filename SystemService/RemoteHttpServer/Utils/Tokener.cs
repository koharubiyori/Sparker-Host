using System.Security.Claims;
using SparkerCommons;

namespace SparkerSystemService.RemoteHttpServer.Utils;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

public class Tokener
{
  private static byte[]? _key;

  private static async Task<byte[]> LoadKeyAsync()
  {
    if (_key != null) return _key;

    if (File.Exists(Constants.TokenKeyFile))
    {
      var base64 = await File.ReadAllTextAsync(Constants.TokenKeyFile);
      _key = Convert.FromBase64String(base64);
    }
    else
    {
      _key = new byte[32];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(_key);
      var base64 = Convert.ToBase64String(_key);
      await File.WriteAllTextAsync(Constants.TokenKeyFile, base64);
    }

    return _key;
  }

  public static async Task<string> SignAsync(DecodedToken data)
  {
    var key = await LoadKeyAsync();
    var securityKey = new SymmetricSecurityKey(key);
    var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var handler = new JwtSecurityTokenHandler();

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      SigningCredentials = creds,
      Claims = data.ToDictionary()
    };

    var token = handler.CreateToken(tokenDescriptor);
    return handler.WriteToken(token);
  }

  public static async Task<DecodedToken> DecodeAsync(string token)
  {
    var key = await LoadKeyAsync();
    var validationParameters = new TokenValidationParameters
    {
      ValidateIssuer = false,
      ValidateAudience = false,
      ValidateLifetime = false,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(key),
      ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
    };

    var handler = new JwtSecurityTokenHandler();
    var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

    var claims = principal.Claims;
    return new DecodedToken(claims);
  }
}

public record DecodedToken(string DeviceId, string Username, string Domain, string Password)
{
  public DecodedToken(IEnumerable<Claim> claims) : this(
    GetClaimValue(claims, "deviceId"),
    GetClaimValue(claims, "username"),
    GetClaimValue(claims, "domain"),
    GetClaimValue(claims, "password")
  ) {}
  public Dictionary<string, object> ToDictionary()
  {
    return new Dictionary<string, object>
    {
      ["deviceId"] = DeviceId,
      ["username"] = Username,
      ["domain"] = Domain,
      ["password"] = Password
    };
  }
  
  private static string GetClaimValue(IEnumerable<Claim> claims, string type)
  {
    foreach (var c in claims)
    {
      if (c.Type == type)
        return c.Value;
    }

    return "";
  }
}