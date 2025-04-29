// Tests/SecurityTests.cs
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SecureApp.Controllers;
using SecureApp.Repositories;
using SecureApp.Services;

namespace SecureApp.Tests;

[TestFixture]
public class SecurityTests
{
    private InputValidator _validator;
    private UserController _controller;
    private Mock<IUserRepository> _mockRepository;

    [SetUp]
    public void Setup()
    {
        _validator = new InputValidator();
        _mockRepository = new Mock<IUserRepository>();
        _controller = new UserController(_mockRepository.Object, _validator);
    }

    // Replace all instances of Assert.IsFalse with Assert.That and use the Is.False constraint
    // Example: Assert.IsFalse(result.IsValid, "message") becomes Assert.That(result.IsValid, Is.False, "message")

    [Test]
    [TestCase("'; DROP TABLE Users; --")]
    [TestCase("' OR '1'='1")]
    [TestCase("admin'--")]
    [TestCase("' UNION SELECT * FROM Users; --")]
    [TestCase("\'; exec sp_executesql N'DROP TABLE Users;--")]
    public void TestSQLInjectionAttempts(string maliciousInput)
    {
        // Arrange & Act
        var result = _validator.ValidateUsername(maliciousInput);

        // Assert
        Assert.That(result.IsValid, Is.False, $"SQL Injection attempt should be invalid: {maliciousInput}");
    }

    [Test]
    [TestCase("<script>alert('xss')</script>")]
    [TestCase("<img src='x' onerror='alert(1)'>")]
    [TestCase("<svg onload='alert(1)'>")]
    [TestCase("javascript:alert(1)")]
    [TestCase("<a onclick='alert(1)'>Click me</a>")]
    public void TestXSSAttempts(string maliciousInput)
    {
        // Arrange & Act
        var result = _validator.ValidateUsername(maliciousInput);

        // Assert
        Assert.That(result.IsValid, Is.False, $"XSS attempt should be invalid: {maliciousInput}");
    }

    [Test]
    public async Task TestSQLInjectionInController()
    {
        // Arrange
        string sqlInjection = "'; DROP TABLE Users; --";

        // Act
        var result = await _controller.ProcessForm(sqlInjection, "test@test.com") as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task TestXSSInController()
    {
        // Arrange
        string xssScript = "<script>alert('xss')</script>";

        // Act
        var result = await _controller.ProcessForm("validuser", xssScript) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void TestEmailValidationWithSQLInjection()
    {
        // Arrange
        string[] maliciousEmails = new[]
        {
            "attack'@example.com",
            "user@example.com'; DROP TABLE Users; --",
            "admin'); DROP TABLE Users; --@example.com"
        };

        // Act & Assert
        foreach (var email in maliciousEmails)
        {
            var result = _validator.ValidateEmail(email);
            Assert.That(result.IsValid, Is.False, $"Email with SQL injection should be invalid: {email}");
        }
    }

    [Test]
    public void TestUsernameWithEncodedXSS()
    {
        // Arrange
        string[] encodedXSS = new[]
        {
            "%3Cscript%3Ealert(1)%3C/script%3E",
            "&#60;script&#62;alert(1)&#60;/script&#62;",
            "&lt;script&gt;alert(1)&lt;/script&gt;"
        };

        // Act & Assert
        foreach (var input in encodedXSS)
        {
            var result = _validator.ValidateUsername(input);
            Assert.That(result.IsValid, Is.False, $"Encoded XSS should be invalid: {input}");
        }
    }

    [Test]
    public void TestInputLengthLimits()
    {
        // Arrange
        string longInput = new string('a', 1000);

        // Act
        var result = _validator.ValidateUsername(longInput);

        // Assert
        Assert.That(result.IsValid, Is.False, "Extremely long input should be rejected");
    }

    [Test]
    public void TestValidInputs()
    {
        // Arrange
        string[] validInputs = new[]
        {
            "normal_user123",
            "john.doe",
            "user-name",
            "ValidUser"
        };

        // Act & Assert
        foreach (var input in validInputs)
        {
            var result = _validator.ValidateUsername(input);
            Assert.That(result.IsValid, Is.True, $"Valid input should be accepted: {input}");
        }
    }
}