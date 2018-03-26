using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Expense.Tracker.Web.Models
{
    public class AccountViewModel
    {
    }
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "ComputerName")]
        public string ComputerName { get; set; }

        [Display(Name = "ClientEmail")]
        public string ClientEmail { get; set; }

        [Display(Name = "IP")]
        public string IP { get; set; }

        [Display(Name = "MAC")]
        public string MAC { get; set; }

        [Display(Name = "Version")]
        public string Version { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
