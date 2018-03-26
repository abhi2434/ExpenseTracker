using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Expense.Tracker.Web.Startup))]
namespace Expense.Tracker.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
