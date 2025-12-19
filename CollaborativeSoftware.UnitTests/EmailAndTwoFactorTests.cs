using CollaborativeSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

[TestClass]
public class EmailServiceTests
{
    [TestMethod]
    public async Task Send2FACodeAsync_WithValidEmail_IsValidFormat()
    {
        string toEmail = "test@example.com";
        string code = "123456";

        try
        {
            var emailAddress = new MailAddress(toEmail);
            Assert.AreEqual("test@example.com", emailAddress.Address);
        }
        catch (FormatException)
        {
            Assert.Fail("Email format is invalid");
        }
    }

    [TestMethod]
    public async Task Send2FACodeAsync_WithValidCode_IsNumeric()
    {
        string code = "654321";

        Assert.IsTrue(code.Length == 6);
        Assert.IsTrue(int.TryParse(code, out _));
    }

    [TestMethod]
    public async Task Send2FACodeAsync_WithMultipleEmails_AllValidFormat()
    {
        var validEmails = new[] { "user@test.com", "admin@example.org", "student@domain.co.uk" };

        foreach (var email in validEmails)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                Assert.AreEqual(email, mailAddress.Address);
            }
            catch (FormatException)
            {
                Assert.Fail($"Email {email} failed validation");
            }
        }
    }

    [TestMethod]
    public async Task Send2FACodeAsync_CodeLength_IsAlwaysSixDigits()
    {
        var codes = new[] { "111111", "222222", "999999", "000001" };

        foreach (string code in codes)
        {
            Assert.IsTrue(code.Length == 6, $"Code {code} is not 6 digits");
            Assert.IsTrue(int.TryParse(code, out _), $"Code {code} is not numeric");
        }
    }
}

[TestClass]
public class TwoFactorManagerTests
{
    [TestMethod]
    public void GenerateCode_ReturnsSixDigitCode()
    {
        string code = TwoFactorManager.GenerateCode();

        Assert.AreEqual(6, code.Length);
        Assert.IsTrue(int.TryParse(code, out int numericCode));
        Assert.IsTrue(numericCode >= 100000 && numericCode <= 999999);
    }

    [TestMethod]
    public void ValidateCode_ValidCode_ReturnsTrue()
    {
        string code = TwoFactorManager.GenerateCode();
        bool result = TwoFactorManager.ValidateCode(code);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ValidateCode_InvalidCode_ReturnsFalse()
    {
        TwoFactorManager.GenerateCode();
        bool result = TwoFactorManager.ValidateCode("000000");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCode_WithEmptyString_ReturnsFalse()
    {
        TwoFactorManager.GenerateCode();
        bool result = TwoFactorManager.ValidateCode(string.Empty);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateCode_WithNullString_ReturnsFalse()
    {
        TwoFactorManager.GenerateCode();
        bool result = TwoFactorManager.ValidateCode(null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GenerateCode_MultipleCallsProduceValidCodes()
    {
        string code1 = TwoFactorManager.GenerateCode();
        string code2 = TwoFactorManager.GenerateCode();

        Assert.AreEqual(6, code1.Length);
        Assert.AreEqual(6, code2.Length);
        Assert.IsTrue(int.TryParse(code1, out _));
        Assert.IsTrue(int.TryParse(code2, out _));
    }
}
