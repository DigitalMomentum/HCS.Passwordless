using System.Security.Cryptography;
using System.Text;

namespace HCS.Umbraco.Passwordless.Endpoints.Shared;

internal static class Sha256Helper
{
    public static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
