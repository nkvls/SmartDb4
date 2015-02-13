using System;
using System.Linq;
using System.Web.Security;
using SmartDb4.DAL.Interfaces;
using SmartDb4.Models;
using WebMatrix.WebData;

namespace SmartDb4.DAL.Repositories
{
    public class UserRepository<T> : GenericRepository<UserProfile>, IUserRepository
{
    public UserRepository(SmartDbContext context) : base(context) { }

    public RoleProvider GetAllRoles()
    {
        return (SimpleRoleProvider) Roles.Provider;
    }

    public UserProfile GetUserByUserName(string userName)
    {
        var user = Context.UserProfiles.FirstOrDefault(a => a.UserName == userName);
        if (user != null)
        {
            user.Password = "password";
            return user;
        }
        return new UserProfile();
    }

    public void Dispose()
    {
        //Dispose(true);
        GC.SuppressFinalize(this);
    }

}
}