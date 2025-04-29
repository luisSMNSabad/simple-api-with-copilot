using NUnit.Framework;
using SecureApp.Services;

namespace SecureApp.Tests;

[TestFixture]
public class InputValidatorTests
{
    private InputValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new InputValidator();
    }

    [Test]
    public void TestForSQLInjection()
    {
        // Test malicious SQL input
        string maliciousInput = "'; DROP TABLE Users;--";
        var result = _validator.ValidateUsername(maliciousInput);
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void TestForXSS()
    {
        // Test malicious script input
        string maliciousInput = "<script>alert('xss')</script>";
        var result = _validator.ValidateUsername(maliciousInput);
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void TestValidUsername()
    {
        string validUsername = "john_doe123";
        var result = _validator.ValidateUsername(validUsername);
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.SanitizedValue, Is.EqualTo(validUsername));
    }

    [Test]
    public void TestValidEmail()
    {
        string validEmail = "john.doe@example.com";
        var result = _validator.ValidateEmail(validEmail);
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.SanitizedValue, Is.EqualTo(validEmail.ToLower()));
    }
}
