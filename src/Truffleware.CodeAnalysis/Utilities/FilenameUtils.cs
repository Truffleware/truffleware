using System.Security.Cryptography;
using System.Text;

namespace Truffleware.CodeAnalysis.Utilities;

public static class FilenameUtils
{
    private const char ReplacementChar = '_';
    private const int StringSafeTotalLength = 50; // Arbitrarily chosen
    private const int StringHashLength = 8; // Arbitrarily chosen
    private const int StringSafeLength = StringSafeTotalLength - StringHashLength - 1; // 1 for joining underscore

    private static readonly char[] _invalidFilenameChars = [..Path.GetInvalidFileNameChars(), '<', '>', ',', '.', ' '];

    /// <summary>
    /// Uniquely shortens a string by suffixing it with a hash of the full value.
    ///
    /// Does nothing if the string is already short enough.
    /// </summary>
    /// <param name="input">String to be shortened.</param>
    public static string ToSafeShort(string input)
    {
        var safeNameFull = ToSafe(input);
        if (safeNameFull.Length < StringSafeTotalLength)
        {
            return safeNameFull;
        }

        var hashString = CreateHash(input);
        var safeNameShort = safeNameFull.Substring(0, StringSafeLength);
        var safeNameSuffixed = $"{safeNameShort}_{hashString}";

        return safeNameSuffixed;
    }

    private static string ToSafe(string input)
    {
        var sb = new StringBuilder(input.Length);

        foreach (var c in input)
        {
            sb.Append(_invalidFilenameChars.Contains(c) ? ReplacementChar : c);
        }

        return sb.ToString();
    }

    private static string CreateHash(string input)
    {
        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        string hashString = BitConverter
            .ToString(hashBytes)
            .Replace("-", "")
            .Substring(0, StringHashLength);

        return hashString;
    }
}
