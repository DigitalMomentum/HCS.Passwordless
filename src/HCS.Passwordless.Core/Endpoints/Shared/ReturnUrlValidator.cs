namespace HCS.Passwordless.Endpoints.Shared;

internal static class ReturnUrlValidator
{
    public static string Sanitize(string? returnUrl, string fallback = "/")
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return fallback;
        // Only allow local (relative) URLs to prevent open redirect.
        if (returnUrl.StartsWith('/') && !returnUrl.StartsWith("//")) return returnUrl;
        return fallback;
    }
}
