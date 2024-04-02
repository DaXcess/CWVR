using System.Security.Cryptography;

namespace CWVR;

internal static class Utils
{
    public static byte[] ComputeHash(byte[] input)
    {
        using var sha = SHA256.Create();

        return sha.ComputeHash(input);
    }
}