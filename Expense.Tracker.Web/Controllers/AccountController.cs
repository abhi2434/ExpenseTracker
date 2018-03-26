using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Expense.Tracker.Web.Models;
using Expense.Tracker.Web.Controllers.Base;
using System.Web.Security;
using Expense.Tracker.Data.EntityModel;
using ExpenseTracker.Utilities;

namespace Expense.Tracker.Web.Controllers
{
    public class AccountController : DbController 
    {
        public IFormsAuthentication FormsAuth = null;
        private MembershipProvider provider = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        public AccountController()
        {
            this.FormsAuth = new FormsAuthenticationService(this.DataBridge);
            this.provider = new ExpenseTrackerCloudMembershipProvider();
        }
        /// <summary>
        /// Logins the specified return URL.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            return View("LoginFlat");
        }

        /// <summary>
        /// Logins the specified loginviewmodel.
        /// </summary>
        /// <param name="loginviewmodel">The loginviewmodel.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginViewModel loginviewmodel, string returnUrl)
        {
            if (ModelState.IsValid)
            {

                //ToDo : Encrypt the password before sending the password
                string password = CryptUtils.GetPasswordEncrypted(loginviewmodel.Password);
                if (provider.ValidateUser(loginviewmodel.UserName, password))
                {

                    FormsAuth.SignIn(loginviewmodel.UserName, false);
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
                ViewBag.ErrorMessage = "The user name or password provided is incorrect.";
            }


            return View("LoginFlat", loginviewmodel);
        }

        //public ActionResult LogOut()
        //{
        //    return View();
        //}

        /// <summary>
        /// Logs the out.
        /// </summary>
        /// <returns></returns>
        [HttpPost] 
        public ActionResult LogOut()
        {
            this.FormsAuth.SignOut();

            // Delete all cookies
            string[] myCookies = Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
            }
            //System.Web.HttpContext.Current.Application.Remove(System.Web.HttpContext.Current.User.Identity.Name);
            return RedirectToAction("Login");
        }

        public ActionResult LockOut(string fullname, string username, string profile, string returnUrl)
        {

            ViewBag.UserName = username;
            ViewBag.FullName = fullname;
            ViewBag.PicPath = profile;
            ViewBag.ReturnUrl = returnUrl;

            return View("LockOutScreen");
        }
        /// <summary>
        /// Logs the out.
        /// </summary>
        /// <returns></returns>
        [HttpPost] 
        public ActionResult LockOut(string ReturnUrl)
        {
            try
            {
                User appuser = this.DatabaseFactory.UserProfileUtils.AppUser;

                this.FormsAuth.SignOut();

                // Delete all cookies
                string[] myCookies = Request.Cookies.AllKeys;
                foreach (string cookie in myCookies)
                {
                    Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
                }
                System.Web.HttpContext.Current.Application.Remove(System.Web.HttpContext.Current.User.Identity.Name);
                return this.RedirectToAction("LockOut", new { fullname = appuser.UserFullName, username = appuser.UserEmail, profile = appuser.ProfilePic, returnUrl = ReturnUrl });
            }
            catch
            {
                return this.LogOut();
            }
        }
        /// <summary>
        /// Redirects to local.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns> 
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard", new { area = "" });
            }
        }

        /// <summary>
        /// Used to reset password when password is forgotten
        /// </summary>
        /// <returns></returns>
        [HttpPost] 
        public JsonResult ForgetPassword(string email)
        {
            var result = this.DatabaseFactory.User.GenerateForgetPassword(email);
            if (result.Status)
            {
                var appuser = result.Value;
                //SendMail mail = new SendMail(this.Store);
                //mail.ForgetPasswordMail(appuser);

                return Json(new AjaxSuccess { Success = true }, JsonRequestBehavior.DenyGet);
            }
            else
                ModelState.AddModelError("Failure", result.Message);

            return Json(new AjaxFailure { Errors = ModelState.GetErrorsFromModelState() }, JsonRequestBehavior.DenyGet);
        }
    }
}