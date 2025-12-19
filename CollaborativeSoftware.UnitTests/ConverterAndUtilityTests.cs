using CollaborativeSoftware;
using CollaborativeSoftware.Converters;
using CollaborativeSoftware.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

[TestClass]
public class ConverterTests
{
    [TestMethod]
    public void ApprovedConverter_TrueValue_ReturnsApproved()
    {
        var converter = new ApprovedConverter();
        var result = converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.AreEqual("Approved", result);
    }

    [TestMethod]
    public void ApprovedConverter_FalseValue_ReturnsPending()
    {
        var converter = new ApprovedConverter();
        var result = converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.AreEqual("Pending", result);
    }

    [TestMethod]
    public void ActiveConverter_TrueValue_ReturnsActive()
    {
        var converter = new ActiveConverter();
        var result = converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.AreEqual("Active", result);
    }

    [TestMethod]
    public void ActiveConverter_FalseValue_ReturnsInactive()
    {
        var converter = new ActiveConverter();
        var result = converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.AreEqual("Inactive", result);
    }

    [TestMethod]
    public void ApprovedConverter_NonBoolValue_ReturnsUnknown()
    {
        var converter = new ApprovedConverter();
        var result = converter.Convert("invalid", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.AreEqual("Unknown", result);
    }

    [TestMethod]
    public void ActiveConverter_NonBoolValue_ReturnsUnknown()
    {
        var converter = new ActiveConverter();
        var result = converter.Convert("invalid", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.AreEqual("Unknown", result);
    }

    [TestMethod]
    public void ApprovedConverter_ConvertBack_ThrowsNotImplemented()
    {
        var converter = new ApprovedConverter();
        Assert.ThrowsException<NotImplementedException>(() =>
            converter.ConvertBack("Approved", typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void ActiveConverter_ConvertBack_ThrowsNotImplemented()
    {
        var converter = new ActiveConverter();
        Assert.ThrowsException<NotImplementedException>(() =>
            converter.ConvertBack("Active", typeof(bool), null, CultureInfo.InvariantCulture));
    }
}

[TestClass]
public class ApplicationUserTests
{
    [TestMethod]
    public void ApplicationUser_DefaultValues_InitializedCorrectly()
    {
        var user = new ApplicationUser();

        Assert.AreEqual(0, user.Id);
        Assert.AreEqual(string.Empty, user.Email);
        Assert.AreEqual(string.Empty, user.FirstName);
        Assert.IsTrue(user.IsActive);
    }

    [TestMethod]
    public void ApplicationUser_PropertiesCanBeSet()
    {
        var user = new ApplicationUser
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Student"
        };

        Assert.AreEqual(1, user.Id);
        Assert.AreEqual("test@example.com", user.Email);
        Assert.AreEqual("John", user.FirstName);
        Assert.AreEqual("Doe", user.LastName);
    }
}

[TestClass]
public class SessionTests
{
    [TestInitialize]
    public void TestInitialize()
    {
        Session.CurrentUserEmail = null;
        Session.CurrentUserRole = default;
        Session.CurrentUserId = 0;
    }

    [TestMethod]
    public void Session_CurrentUserEmail_CanBeSet()
    {
        Session.CurrentUserEmail = "test@example.com";
        Assert.AreEqual("test@example.com", Session.CurrentUserEmail);
    }

    [TestMethod]
    public void Session_CurrentUserRole_CanBeSet()
    {
        Session.CurrentUserRole = UserRole.Student;
        Assert.AreEqual(UserRole.Student, Session.CurrentUserRole);
    }

    [TestMethod]
    public void Session_CurrentUserId_CanBeSet()
    {
        Session.CurrentUserId = 42;
        Assert.AreEqual(42, Session.CurrentUserId);
    }

    [TestMethod]
    public void Session_AllPropertiesCanBeSetTogether()
    {
        Session.CurrentUserEmail = "student@example.com";
        Session.CurrentUserRole = UserRole.Student;
        Session.CurrentUserId = 123;

        Assert.AreEqual("student@example.com", Session.CurrentUserEmail);
        Assert.AreEqual(UserRole.Student, Session.CurrentUserRole);
        Assert.AreEqual(123, Session.CurrentUserId);
    }

    [TestMethod]
    public void Session_AdminRole_CanBeSet()
    {
        Session.CurrentUserRole = UserRole.Admin;
        Assert.AreEqual(UserRole.Admin, Session.CurrentUserRole);
    }
}
