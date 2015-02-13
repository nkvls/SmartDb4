using System;
using System.Web.Security;
using SmartDb4.Models;

namespace SmartDb4.DAL.Interfaces
{
    public interface IUserRepository : IDisposable
    {
        RoleProvider GetAllRoles();
        UserProfile GetUserByUserName(string userName);
    }
}
