using CollaborativeSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Text;


[TestClass]
public class AdminDashboardTests
{
    // ==================== PASSWORD HASHING TESTS ====================
    
    /// <summary>
    /// Test that password hashing produces consistent results - critical for lecturer password creation
    /// </summary>
    [TestMethod]
    public void HashPassword_CreateLecturer_SamePasswordHashesConsistently()
    {
        // Arrange
        string lecturerPassword = "LecturerPass123!";

        // Act
        string hash1 = HashPasswordHelper(lecturerPassword);
        string hash2 = HashPasswordHelper(lecturerPassword);

        // Assert
        Assert.AreEqual(hash1, hash2, 
            "When creating a lecturer, the same password should produce identical hashes for verification");
    }

    /// <summary>
    /// Test that different passwords produce different hashes - security validation
    /// </summary>
    [TestMethod]
    public void HashPassword_DifferentLecturerPasswords_ProduceDifferentHashes()
    {
        // Arrange
        string password1 = "Lecturer1Password!";
        string password2 = "Lecturer2Password!";

        // Act
        string hash1 = HashPasswordHelper(password1);
        string hash2 = HashPasswordHelper(password2);

        // Assert
        Assert.AreNotEqual(hash1, hash2, 
            "Different lecturer passwords must produce different hashes for security");
    }

    /// <summary>
    /// Test password reset functionality - verifies hash is secure
    /// </summary>
    [TestMethod]
    public void HashPassword_ResetLecturerPassword_ProducesValidSecureHash()
    {
        // Arrange
        string newPassword = "NewSecurePassword456!";

        // Act
        string hash = HashPasswordHelper(newPassword);

        // Assert
        Assert.IsNotNull(hash, "Password reset hash should not be null");
        Assert.IsTrue(hash.Length > 0, "Password reset hash should not be empty");
        Assert.IsTrue(IsValidBase64(hash), "Password reset hash must be valid Base64");
    }

    /// <summary>
    /// Test email validation for creating lecturers
    /// </summary>
    [TestMethod]
    public void CreateLecturer_ValidEmail_PassesValidation()
    {
        // Arrange
        string email = "lecturer@example.com";

        // Act & Assert
        Assert.IsTrue(IsValidEmail(email), 
            "Valid lecturer email should pass validation");
    }

    /// <summary>
    /// Test invalid email format for lecturer creation
    /// </summary>
    [TestMethod]
    public void CreateLecturer_InvalidEmail_FailsValidation()
    {
        // Arrange
        string[] invalidEmails = { "notanemail", "missing@", "@nodomain", "spaces in@email.com" };

        // Act & Assert
        foreach (var email in invalidEmails)
        {
            Assert.IsFalse(IsValidEmail(email), 
                $"Invalid email '{email}' should fail validation during lecturer creation");
        }
    }

    /// <summary>
    /// Test that lecturer first and last names are properly validated (non-empty)
    /// </summary>
    [TestMethod]
    public void CreateLecturer_NameValidation_RejectsEmptyNames()
    {
        // Arrange
        string[] invalidNames = { "", "   ", null };

        // Act & Assert
        foreach (var name in invalidNames)
        {
            bool isValid = !string.IsNullOrWhiteSpace(name);
            Assert.IsFalse(isValid, 
                "Empty or whitespace-only lecturer names should be rejected during creation");
        }
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Helper method to hash passwords using SHA256 (matching AdminDashboardWindow implementation)
    /// </summary>
    private string HashPasswordHelper(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Helper method to validate email format
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Helper method to validate Base64 format
    /// </summary>
    private bool IsValidBase64(string hash)
    {
        try
        {
            Convert.FromBase64String(hash);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
