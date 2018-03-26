using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Expense.Tracker.Web.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {
            Exception exception = Server.GetLastError();
            Response.StatusCode = 500;
            return View(exception);
        }

        public ActionResult NotFound()
        {
            Response.AddHeader("Status Code", "404");
            //Response.StatusCode = 404;
            return View();
        }

        public ActionResult BadRequest()
        {
            Response.AddHeader("Status Code", "403");
            //Response.StatusCode = 403;
            return View();
        }
    }
}