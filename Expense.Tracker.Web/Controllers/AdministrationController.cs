using Expense.Tracker.Security;
using Expense.Tracker.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Expense.Tracker.Web.Controllers
{
    [AuthorizeRoles]
    public class AdministrationController : DbController
    {
        // GET: Administration
        public ActionResult Index()
        {
            return View();
        }
    }
}