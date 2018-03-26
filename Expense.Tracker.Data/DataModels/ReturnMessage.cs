using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataModels
{
    public class ReturnMessage<T>
    {
        public ReturnMessage(T value, string message, bool status)
        {
            this.Value = value;
            this.Message = message;
            this.Status = status;
        }
        public ReturnMessage(bool status)
            : this(default(T), "", status)
        {

        }
        public ReturnMessage(string message, bool status)
            : this(default(T), message, status)
        { }
        public ReturnMessage()
            : this(default(T), "", false)
        { }
        public T Value { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
