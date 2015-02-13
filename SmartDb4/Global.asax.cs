using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net.Appender;
using SmartDb4.Controllers;
using SmartDb4.Helpers;

namespace SmartDb4
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            //log4net.Config.XmlConfigurator.Configure();

            try
            {
                AreaRegistration.RegisterAllAreas();

                WebApiConfig.Register(GlobalConfiguration.Configuration);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
                AuthConfig.RegisterAuth();
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("Global.Application_Start() : " + ex, "Error");
                throw;
            }

            Utility.LogInfo("Application Started Successfully");
        }

        //protected void Application_PreRequestHandlerExecute()
        //{
        //    var culture = CultureInfo.CreateSpecificCulture("en-GB");
        //    culture.DateTimeFormat = CultureInfo.CreateSpecificCulture("en-GB").DateTimeFormat;
        //    Thread.CurrentThread.CurrentCulture = culture;

        //    culture = new CultureInfo("en-GB") {DateTimeFormat = Thread.CurrentThread.CurrentCulture.DateTimeFormat};
        //    Thread.CurrentThread.CurrentUICulture = culture;
        //}

        protected void Application_Error(object sender, EventArgs e)
        {
            var handleError = Convert.ToBoolean(ConfigurationManager.AppSettings["RedirectToErrorPage"]);

            var exception = Server.GetLastError();

            Utility.WriteToLog("Global.Application_Error() : " + exception, "Error");

            if (!handleError) return;

            Server.ClearError();

            var routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("action", "Index");
            routeData.Values.Add("exception", exception);

            if (exception.GetType() == typeof(HttpException))
            {
                routeData.Values.Add("statusCode", ((HttpException)exception).GetHttpCode());
            }
            else
            {
                routeData.Values.Add("statusCode", 500);
            }

            IController controller = new ErrorController();
            controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            Response.End();
        }
    }

    public class CwdFileAppender : RollingFileAppender
    {
        public override string File
        {
            set
            {
                base.File = AppDomain.CurrentDomain.BaseDirectory + value;
            }
        }
    }
}