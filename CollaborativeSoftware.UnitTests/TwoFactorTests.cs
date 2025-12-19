using CollaborativeSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;


[TestClass]
public class TwoFactorTests
{
    [TestMethod]
    public void GenerateCode_ReturnsSixDigitCode()
    {
        string code = TwoFactorManager.GenerateCode();

        Assert.AreEqual(6, code.Length);
        Assert.IsTrue(int.TryParse(code, out _));
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
}
