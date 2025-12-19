using CollaborativeSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;


[TestClass]
public class PasswordTests
{
    [TestMethod]
    public void HashPassword_SameInput_ProducesSameHash()
    {
        string password = "password123";

        string hash1 = StudentLoginWindow.HashPassword(password);
        string hash2 = StudentLoginWindow.HashPassword(password);

        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        string password = "password123";
        string hash = StudentLoginWindow.HashPassword(password);

        bool result = StudentLoginWindow.VerifyPassword(password, hash);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        string hash = StudentLoginWindow.HashPassword("password123");

        bool result = StudentLoginWindow.VerifyPassword("wrongpass", hash);

        Assert.IsFalse(result);
    }
}
