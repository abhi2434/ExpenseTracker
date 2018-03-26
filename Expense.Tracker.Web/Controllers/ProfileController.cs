using Expense.Tracker.Data.DataModels;
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
    [Authorize]
    public class ProfileController : DbController
    {
 
        public ActionResult Index()
        {
            var pModel = this.DatabaseFactory.UserProfileUtils.GetProfile(this.HttpContext);
            ViewBag.AssignedMessage = "You have full access to your profile";

            return View(pModel);
        }

        public JsonResult EditDetails(string userName, string contactNumber)
        {
            this.DatabaseFactory.UserProfileUtils.EditCurrentDetails(userName, contactNumber);
            var result = new { Response = "Details updated !" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditPassword(string oldPassword, string newPassword)
        {
            this.DatabaseFactory.UserProfileUtils.ChangeUserPassword(oldPassword, newPassword);
            var result = new { Response = "Password updated !" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
 
        [HttpPost]
        public JsonResult FileUpload(Guid id)
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

                    var appUser = this.DataBridge.Users.Find(id);
                    appUser.ProfilePic = path;
                    this.DataBridge.SaveChanges();

                    return Json(path);
                }
            }

            return Json("");
        }

        [HttpPost]
        public JsonResult RemoveFile(Guid id)
        {
            var appUser = this.DataBridge.Users.Find(id);
            appUser.ProfilePic = null;
            this.DataBridge.SaveChanges();

            return Json("");
        }

        

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}