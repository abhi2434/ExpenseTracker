using Expense.Tracker.Data.EntityModel;
using Expense.Tracker.Security;
using Expense.Tracker.Web.Controllers.Base;
using ExpenseTracker.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Expense.Tracker.Web.Controllers
{
    [AuthorizeRoles]
    public class ExpensesController : DbController
    {
        // GET: Expenses
        public ActionResult Index(int? days, string SearchString)
        {
            //if (!days.HasValue)
            //    days = 7; 
            return View(this.DatabaseFactory.Expenses.GetExpenseData(days, SearchString));
        }

        public ActionResult Create()
        {
            ViewBag.Users = this.DataBridge.Users.AsEnumerable();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Expens expense)
        {
            if (ModelState.IsValid)
            {
                expense.ExpenseId = Guid.NewGuid();
                this.DataBridge.Expenses.Add(expense);
                this.DataBridge.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(expense);
        }

        public ActionResult Edit(Guid expenseId)
        {
            var expense = this.DataBridge.Expenses.Find(expenseId);
            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Expens expense)
        {
            if (ModelState.IsValid)
            {
                this.DataBridge.Entry(expense).State = System.Data.Entity.EntityState.Modified; 
                this.DataBridge.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(expense);
        }

        [HttpPost]
        public JsonResult FileUpload()
        {
            if (this.Request.Files.Count > 0)
            {
                var file = this.Request.Files[0];
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    fileName = Guid.NewGuid().ToString() + fileName;
                    var appUploader = new AppUploader(ConfigurationManager.AppSettings["AzureStoreConnection"]);
                    var path = appUploader.UploadUserLogo(file.InputStream, fileName); 
                    return Json(path);
                }
            }

            return Json("");
        }

        [HttpPost]
        public JsonResult RemoveFile(string filePath)
        {
            //var appUser = this.DataBridge.Users.Find(id);
            //appUser.ProfilePic = null;
            //this.DataBridge.SaveChanges();
            var appUploader = new AppUploader(ConfigurationManager.AppSettings["AzureStoreConnection"]);
            appUploader.RemoveFile(filePath);
            return Json("");
        }
    }
}