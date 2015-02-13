using System;
using System.Web.Mvc;

namespace SmartDb4.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index(int statusCode, Exception exception)
        {
            Response.StatusCode = statusCode;
            return View();
        }

        //public HttpStatusCodeResult GetStatusCodeResult(int statusCode)
        //{
        //    switch (statusCode)
        //    {
        //            case HttpStatusCode.BadRequest
        //    }
        //}
    }
}
