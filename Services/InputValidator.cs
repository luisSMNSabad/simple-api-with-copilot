using System.Text.RegularExpressions;
using System.Web;

namespace SecureApp.Services;

public class InputValidator
{
    // Constants for validation
    private const int MAX_USERNAME_LENGTH = 50;
    private const int MAX_EMAIL_LENGTH = 100;
    private const string EMAIL_PATTERN = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    private const string USERNAME_PATTERN = @"^[a-zA-Z0-9_-]{3,50}$";

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string SanitizedValue { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Sanitizes and validates username input
    /// </summary>
    public ValidationResult ValidateUsername(string username)
    {
        var result = new ValidationResult();

        // Check for null or empty
        if (string.IsNullOrWhiteSpace(username))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Username cannot be empty"
            };
        }

        // Trim and sanitize
        string sanitizedUsername = HttpUtility.HtmlEncode(username.Trim());

        // Check length
        if (sanitizedUsername.Length > MAX_USERNAME_LENGTH)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Username must not exceed {MAX_USERNAME_LENGTH} characters"
            };
        }

        // Validate pattern
        if (!Regex.IsMatch(sanitizedUsername, USERNAME_PATTERN))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens"
            };
        }

        return new ValidationResult
        {
            IsValid = true,
            SanitizedValue = sanitizedUsername
        };
    }

    /// <summary>
    /// Sanitizes and validates email input
    /// </summary>
    public ValidationResult ValidateEmail(string email)
    {
        var result = new ValidationResult();

        // Check for null or empty
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Email cannot be empty"
            };
        }

        // Trim and sanitize
        string sanitizedEmail = HttpUtility.HtmlEncode(email.Trim().ToLower());

        // Check length
        if (sanitizedEmail.Length > MAX_EMAIL_LENGTH)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Email must not exceed {MAX_EMAIL_LENGTH} characters"
            };
        }

        // Validate email pattern
        if (!Regex.IsMatch(sanitizedEmail, EMAIL_PATTERN))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid email format"
            };
        }

        return new ValidationResult
        {
            IsValid = true,
            SanitizedValue = sanitizedEmail
        };
    }

    /// <summary>
    /// Removes potentially dangerous characters from input
    /// </summary>
    public string RemoveDangerousCharacters(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Remove common SQL injection and XSS attack characters
        string cleaned = input
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(";", "")
            .Replace("--", "")
            .Replace("/*", "")
            .Replace("*/", "")
            .Replace("xp_", "")
            .Replace("<script>", "")
            .Replace("</script>", "");

        // Remove any HTML tags
        cleaned = Regex.Replace(cleaned, "<.*?>", string.Empty);

        return cleaned;
    }
}