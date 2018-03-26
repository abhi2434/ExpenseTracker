using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Expense.Tracker.Web.Models
{
    public class AjaxFailure
    {
        public IEnumerable<string> Errors { get; set; }
    }
}