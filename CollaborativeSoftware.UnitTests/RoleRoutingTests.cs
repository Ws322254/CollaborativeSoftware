using CollaborativeSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;


[TestClass]
public class RoleTests
{
    [TestMethod]
    public void LecturerRole_IsLecturer()
    {
        UserRole role = UserRole.Lecturer;
        Assert.AreEqual(UserRole.Lecturer, role);
    }

    [TestMethod]
    public void AdminRole_IsAdmin()
    {
        UserRole role = UserRole.Admin;
        Assert.AreEqual(UserRole.Admin, role);
    }
}
