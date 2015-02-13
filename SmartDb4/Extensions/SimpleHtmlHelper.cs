using System;
using System.Web.Mvc;

namespace SmartDb4.Extensions
{
    public static class SimpleHtmlHelper
    {
        public static String NavActive(this HtmlHelper htmlHelper, string actionName, string controllerName)
        {
            var controller = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");
            var action = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
            if (controller.ToLower() == "home")
            {
                controller = "SmartDbHome";
            }
            //if (controllerName == controller && action == actionName)
            if (controllerName == controller)
                return "active";
            return String.Empty;
        }
    }



    //public class EmployeeBinder1 : IModelBinder
    //{
    //    public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    //    {
    //        var lstAllMembers = new List<CutDownVersionOfMembers>();
    //        var member = new CutDownVersionOfMembers();
    //        for (int i = 0; i < 2; i++)
    //        {
    //            member.MemberId = controllerContext.HttpContext.Request.Form["Id"];
    //        }
    //        emp.Id = "E" + controllerContext.HttpContext.Request.Form["Id"];
    //        emp.FirstName = controllerContext.HttpContext.Request.Form["FirstName"];
    //        emp.LastName = controllerContext.HttpContext.Request.Form["LastName"];
    //         emp.BirthDate = new DateTime(int.Parse(controllerContext.HttpContext.Request.Form["year"]), 
    //            int.Parse(controllerContext.HttpContext.Request.Form["month"]), 
    //            int.Parse(controllerContext.HttpContext.Request.Form["day"]));
    //        return emp;
    //    }
}