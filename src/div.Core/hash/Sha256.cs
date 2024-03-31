using System.Security.Cryptography;
using System.Text;

namespace div;

public static class ShaUtils
{
    public static string ComputeSha256Hash(byte[] b)
    {
        using var sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(b);

        var builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}