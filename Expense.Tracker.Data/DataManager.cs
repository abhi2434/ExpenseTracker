using Expense.Tracker.Data.DataUtilities;
using Expense.Tracker.Data.EntityModel;
using ExpenseTracker.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data
{
    public class DataManager
    {

        private ConnectionBuilder builder = null;
        private ExpenseTrackerEntities db = null;

        public DataManager(string connectionString)
        {
            this.builder = new ConnectionBuilder(connectionString);
        }
        public DataManager(ExpenseTrackerEntities db)
        {
            this.db = db;
        }


        public ExpenseTrackerEntities DataFactory
        {
            get
            {
                if (this.db == null)
                {
                    this.db = new ExpenseTrackerEntities(this.builder.ConnectionString);
                    if (this.db.Database.Connection.State != System.Data.ConnectionState.Open)

                        this.db.Database.Connection.Open();
                }
                return this.db;
            }
        }

        private UserDataUtil _user;

        public UserDataUtil User
        {
            get
            {
                this._user = this._user ?? new UserDataUtil(this);
                return this._user;
            }
        }
        private UserProfileDataUtils _userProfileUtils;

        public UserProfileDataUtils UserProfileUtils
        {
            get
            {
                if (this._userProfileUtils == null)
                {
                    string userName = Web.GetCurrentUserName();
                    this._userProfileUtils = new UserProfileDataUtils(userName, this);
                }
                return this._userProfileUtils;
            }
        }
        private AuthorizationDataUtils _authUtils;
        public AuthorizationDataUtils AuthUtils
        {
            get
            {
                this._authUtils = this._authUtils ?? new AuthorizationDataUtils(this);
                return this._authUtils;
            }
        }

        private ExpenseDataUtil _expenses;

        public ExpenseDataUtil Expenses
        {
            get
            {
                this._expenses = this._expenses ?? new ExpenseDataUtil(this);
                return this._expenses;
            }
        }
    }
}
