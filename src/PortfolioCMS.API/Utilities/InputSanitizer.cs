using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PortfolioCMS.API.Utilities;

/// <summary>
/// Utility class for sanitizing user input to prevent XSS and injection attacks
/// </summary>
public static class InputSanitizer
{
    private static readonly Regex HtmlTagPattern = new(
        @"<[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ScriptPattern = new(
        @"<script[^>]*>.*?</script>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex EventHandlerPattern = new(
        @"\s*on\w+\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] DangerousPatterns = new[]
    {
        "javascript:",
        "vbscript:",
        "data:text/html",
        "data:application/",
        "<iframe",
        "<embed",
        "<object",
        "eval(",
        "expression("
    };

    /// <summary>
    /// Sanitizes HTML content by removing dangerous tags and attributes
    /// </summary>
    public static string SanitizeHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove script tags
        var sanitized = ScriptPattern.Replace(input, string.Empty);

        // Remove event handlers
        sanitized = EventHandlerPattern.Replace(sanitized, string.Empty);

        // Remove dangerous patterns
        foreach (var pattern in DangerousPatterns)
        {
            sanitized = sanitized.Replace(pattern, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return sanitized;
    }

    /// <summary>
    /// Strips all HTML tags from input
    /// </summary>
    public static string StripHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove all HTML tags
        var stripped = HtmlTagPattern.Replace(input, string.Empty);

        // Decode HTML entities
        stripped = HttpUtility.HtmlDecode(stripped);

        return stripped.Trim();
    }

    /// <summary>
    /// Encodes HTML to prevent XSS attacks
    /// </summary>
    public static string EncodeHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return HttpUtility.HtmlEncode(input);
    }

    /// <summary>
    /// Sanitizes input for use in SQL queries (use parameterized queries instead when possible)
    /// </summary>
    public static string SanitizeSql(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove SQL comment markers
        var sanitized = input.Replace("--", string.Empty)
                            .Replace("/*", string.Empty)
                            .Replace("*/", string.Empty);

        // Remove semicolons (statement terminators)
        sanitized = sanitized.Replace(";", string.Empty);

        return sanitized.Trim();
    }

    /// <summary>
    /// Validates and sanitizes email addresses
    /// </summary>
    public static string? SanitizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        // Basic email validation pattern
        var emailPattern = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        var sanitized = email.Trim().ToLowerInvariant();

        return emailPattern.IsMatch(sanitized) ? sanitized : null;
    }

    /// <summary>
    /// Sanitizes URL to prevent javascript: and data: schemes
    /// </summary>
    public static string? SanitizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var sanitized = url.Trim();

        // Check for dangerous schemes
        if (sanitized.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
            sanitized.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase) ||
            sanitized.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Ensure URL starts with http:// or https://
        if (!sanitized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !sanitized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitizes file names to prevent path traversal
    /// </summary>
    public static string SanitizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        // Remove path separators and special characters
        var sanitized = fileName.Trim();
        
        // Remove directory separators
        sanitized = sanitized.Replace("/", string.Empty)
                            .Replace("\\", string.Empty)
                            .Replace("..", string.Empty);

        // Remove other dangerous characters
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c.ToString(), string.Empty);
        }

        return sanitized;
    }

    /// <summary>
    /// Truncates string to maximum length
    /// </summary>
    public static string Truncate(string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Length <= maxLength ? input : input[..maxLength];
    }

    /// <summary>
    /// Removes excessive whitespace and normalizes line endings
    /// </summary>
    public static string NormalizeWhitespace(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Normalize line endings
        var normalized = input.Replace("\r\n", "\n").Replace("\r", "\n");

        // Remove excessive whitespace
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized.Trim();
    }
}
