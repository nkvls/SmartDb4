using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using SmartDb4.DAL;
using WebMatrix.WebData;

namespace SmartDb4.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<SmartDbContext>(null);

                try
                {
                    using (var context = new SmartDbContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }

                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);

                    var roles = (SimpleRoleProvider)System.Web.Security.Roles.Provider;
                    //var membership = (SimpleMembershipProvider)System.Web.Security.Membership.Provider;

                    if (!roles.RoleExists("Admin"))
                        roles.CreateRole("Admin");

                    if (!roles.RoleExists("Staff"))
                        roles.CreateRole("Staff");

                    if (!roles.RoleExists("Referrer"))
                        roles.CreateRole("Referrer");

                    ////following code will check user with name admin and if not found can create a default admin user with password as 'passowrd'
                    //var usr = membership.GetUser("admin", false);
                    //if (usr == null)
                    //{
                    //    WebSecurity.CreateUserAndAccount("admin", "changeme");
                    //    //WebSecurity.CreateAccount("admin", "password");
                    //}

                    //if (!roles.GetRolesForUser("admin").Contains("Admin"))
                    //    roles.AddUsersToRoles(new[] { "admin" }, new[] { "Admin" });
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }
        }
    }
}
