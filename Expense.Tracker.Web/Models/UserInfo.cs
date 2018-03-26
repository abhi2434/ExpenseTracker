using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Expense.Tracker.Web.Models
{
    //public enum AuthType : int
    //{
    //    AllOk,
    //    SubscriptionNotPresent,
    //    SubscriptionExpired,
    //    UserDeactivated,
    //    AuthenticationFailed,
    //    AgentDeactivated
    //}
    //public class UserInfo
    //{
    //    public UserInfo() { }
    //    public UserInfo(Token token, AppUser appuser)
    //        : this(token)
    //    {

    //        this.CurrentAppUser = appuser;
    //    }
    //    public UserInfo(Token token)
    //    {
    //        this.CurrentToken = token;
    //    }

    //    [JsonIgnore]
    //    private Token CurrentToken { get; set; }

    //    public Token GetCurrentToken()
    //    {
    //        return this.CurrentToken;
    //    }
    //    public AppUser GetCurrentUser(APPSeCONNECTEntities db)
    //    {
    //        return db.AppUsers.FirstOrDefault(e => e.AppUserId == this.CurrentToken.AppUserId);
    //    }

    //    private AppUser _currentUser;
    //    [JsonIgnore]
    //    private AppUser CurrentAppUser
    //    {
    //        get
    //        {
    //            if (this._currentUser == null)
    //            {
    //                APPSeCONNECTEntities entries = new APPSeCONNECTEntities();
    //                this._currentUser = this.GetCurrentUser(entries);
    //            }
    //            return this._currentUser;
    //        }
    //        set
    //        {
    //            this._currentUser = value;
    //        }
    //    }

    //    public string FullName
    //    {
    //        get
    //        {
    //            try
    //            {
    //                return this.CurrentAppUser.AppUserFullName;
    //            }
    //            catch { }
    //            return string.Empty;
    //        }
    //    }
    //    public string UserName
    //    {
    //        get
    //        {
    //            try
    //            {
    //                return this.CurrentAppUser.AppUserEmail;
    //            }
    //            catch { }
    //            return string.Empty;

    //        }
    //    }
    //    public Guid AccessToken
    //    {
    //        get
    //        {
    //            try
    //            {
    //                if (this.isShowToken)
    //                    return this.CurrentToken.TokenId;
    //            }
    //            catch { }
    //            return new Guid();
    //        }
    //    }

    //    public Guid AdminToken
    //    {
    //        get;
    //        set;
    //    }

    //    public Guid AuthToken { get { return this.AccessToken; } }

    //    public int TTL
    //    {
    //        get
    //        {
    //            try
    //            {
    //                return this.CurrentToken.TTL;
    //            }
    //            catch { }
    //            return 0;
    //        }
    //    }
    //    OrgUser _orguser;
    //    [JsonIgnore]
    //    private OrgUser DefaultOrganizationUser
    //    {
    //        get
    //        {
    //            try
    //            {
    //                if (_orguser == null)
    //                {
    //                    var user = this.CurrentAppUser;
    //                    var orgUsers = user.OrgUsers.Where(o => o.AppUserId == user.AppUserId && o.IsDefaultOrg != false);
    //                    if (orgUsers.Any(e => e.IsDefaultOrg == true))
    //                        _orguser = orgUsers.First(e => e.IsDefaultOrg == true);

    //                    _orguser = orgUsers.FirstOrDefault();
    //                }
    //                return _orguser;
    //            }
    //            catch { }
    //            return null;
    //        }
    //        set
    //        {
    //            this._orguser = value;
    //        }
    //    }
    //    public Org GetDefaultOrganization()
    //    {
    //        return this.DefaultOrganizationUser.Org;
    //    }
    //    public Guid OrganizationId
    //    {
    //        get
    //        {
    //            try
    //            {
    //                return this.DefaultOrganizationUser.OrgId;
    //            }
    //            catch { }
    //            return new Guid();
    //        }
    //    }

    //    private List<OrganizationItem> _organizations = new List<OrganizationItem>();
    //    public List<OrganizationItem> Organizations
    //    {
    //        get { return _organizations; }
    //        set { this._organizations = value; }
    //    }
    //    public bool IsShowWelcome
    //    {
    //        get
    //        {
    //            return this.CurrentAppUser.IsShowWelcome.HasValue && this.CurrentAppUser.IsShowWelcome.Value;
    //        }
    //    }
    //    public bool IsConnectionCreated
    //    {
    //        get
    //        {
    //            if (this.DefaultOrganizationUser != null)
    //                return this.DefaultOrganizationUser.Org.OrgApps.Any();

    //            return false;
    //        }
    //    }

    //    public int RefTyp { get; set; }
    //    public string RefId
    //    {
    //        get;
    //        set;
    //    }

    //    public string RefMsg { get; set; }

    //    public int ConnectionsCount { get; set; }
    //    public int AppsCount { get; set; }
    //    public bool IsAgentDownloaded { get; set; }
    //    private Dictionary<string, string> _additionalConfig = new Dictionary<string, string>();
    //    public Dictionary<string, string> AdditionalConfig
    //    {
    //        get
    //        {
    //            return this._additionalConfig;
    //        }
    //    }
    //    private bool isShowToken = true;
    //    internal void HideToken()
    //    {
    //        this.isShowToken = false;
    //    }

    //    internal void ShowToken()
    //    {
    //        this.isShowToken = true;
    //    }

    //    public Guid AgentIdentification { get; set; }
    //    DateTime LastAccessDateTime { get; set; }

    //    public void SetLastAccessDateTime()
    //    {
    //        if (this.CurrentToken != null)
    //            this.LastAccessDateTime = this.CurrentToken.TokenGeneratedAt;
    //    }
    //    public void SetLastAccessDateTime(DateTime selectedTime)
    //    {
    //        this.LastAccessDateTime = selectedTime;
    //    }
    //    public string RoleName { get; set; }
    //    public string RoleDescirption { get; set; }
    //    private List<RoleItem> _roles = new List<RoleItem>();
    //    public List<RoleItem> Roles
    //    {
    //        get
    //        {
    //            return this._roles;
    //        }
    //        set
    //        {
    //            this._roles = value;
    //        }
    //    }
    //    public bool ValidateTTL()
    //    {
    //        DateTime lastAccessDateTime = this.LastAccessDateTime;
    //        DateTime validDateTime = this.LastAccessDateTime.AddMinutes(this.CurrentToken.TTL);

    //        return DateTime.UtcNow <= validDateTime;
    //    }
    //    public void ResetDefaultOrganization(User newOrgUser)
    //    {
    //        this.DefaultOrganizationUser = newOrgUser;
    //    }

    //    public bool IsTrial { get; set; }

    //    public bool IsEulaSigned { get; set; }

    //    public bool IsPrimaryAgent { get; set; }
    //}
}