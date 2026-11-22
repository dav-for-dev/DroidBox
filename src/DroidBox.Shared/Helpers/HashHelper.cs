using System.Security.Cryptography;

namespace DroidBox.Shared.Helpers;

public static class HashHelper
{
    public static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found for hash calculation", filePath);
        }

        using var sha256 = SHA256.Create();
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public static bool VerifyHash(string filePath, string expectedHash)
    {
        var computed = ComputeSha256Async(filePath).GetAwaiter().GetResult();
        return string.Equals(computed, expectedHash.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
