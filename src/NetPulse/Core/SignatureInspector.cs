using System.Security.Cryptography.X509Certificates;

namespace NetPulse.Core;

public static class SignatureInspector
{
    public static string? ResolveSubject()
    {
        try
        {
            string? exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return null;

            using X509Certificate certificate = X509Certificate.CreateFromSignedFile(exePath!);
            string subject = certificate.Subject ?? string.Empty;
            return ExtractCommonName(subject) ?? subject;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static string? ExtractCommonName(string subject)
    {
        const string prefix = "CN=";
        int start = subject.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0) return null;
        start += prefix.Length;
        int end = subject.IndexOf(',', start);
        return end < 0 ? subject[start..] : subject[start..end];
    }
}
