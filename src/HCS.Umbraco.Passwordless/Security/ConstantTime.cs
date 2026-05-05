using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace HCS.Umbraco.Passwordless.Security;

internal static class ConstantTime
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool Equals(byte[] a, byte[] b)
        => CryptographicOperations.FixedTimeEquals(a, b);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool Equals(string a, string b)
    {
        var aBytes = System.Text.Encoding.UTF8.GetBytes(a);
        var bBytes = System.Text.Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
