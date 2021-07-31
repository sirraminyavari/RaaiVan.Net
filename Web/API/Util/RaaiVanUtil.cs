using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Text;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Privacy;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.RaaiVanConfig;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.Knowledge;

namespace RaaiVan.Web.API
{
    [Serializable]
    public class AccessToken
    {
        private DateTime? _ExpiresAt;
        private bool _Expired;

        public string Token;

        public AccessToken()
        {
            Token = PublicMethods.random_string(20).Replace('+', '_');
            _ExpiresAt = null;
            _Expired = false;
        }

        public void Use()
        {
            _ExpiresAt = DateTime.Now.AddMinutes(5);
        }

        [JsonIgnore]
        public bool Expiring
        {
            get { return _ExpiresAt.HasValue && !Expired; }
        }

        [JsonIgnore]
        public bool Expired
        {
            get { return (_Expired = _Expired || (_ExpiresAt.HasValue && _ExpiresAt.Value < DateTime.Now)); }
        }
    }
    
    [Serializable]
    public class AccessTokenList
    {
        private SortedList<string, AccessToken> _Tokens;
        private DateTime _LastClearanceTime;

        public AccessTokenList()
        {
            _Tokens = new SortedList<string, AccessToken>();
            _LastClearanceTime = DateTime.Now;
        }

        private void clear_expired_tokens()
        {
            if ((DateTime.Now - _LastClearanceTime).Minutes < 5) return;

            List<string> keys = _Tokens.Keys.Select(u => u).ToList();

            foreach (string key in keys) if (_Tokens[key].Expired) _Tokens.Remove(key);
        }
        
        private static string _SessionVariableName = "AccessToken";
        private static string _ClientTokenVariableName = "acstkn";

        private static AccessTokenList _get_token_list(HttpContext context, bool create = false)
        {
            if (context.Session == null && !create) return null;

            if (context.Session[_SessionVariableName] == null)
                context.Session[_SessionVariableName] = new AccessTokenList();
            return (AccessTokenList)context.Session[_SessionVariableName];
        }

        private static string _get_client_token(HttpContext context)
        {
            return string.IsNullOrEmpty(context.Request.Params[_ClientTokenVariableName]) ? string.Empty :
                context.Request.Params[_ClientTokenVariableName].Replace(' ', '+');
        }

        public static string new_token(AccessTokenList lst)
        {
            if (lst == null) return string.Empty;

            lst.clear_expired_tokens();

            AccessToken token = new AccessToken();

            if (lst._Tokens.ContainsKey(token.Token)) return string.Empty;
            else
            {
                lst._Tokens.Add(token.Token, token);
                return token.Token;
            }
        }

        public static string new_token(HttpContext context)
        {
            return new_token(_get_token_list(context, true));
        }

        public static string refresh_token(HttpContext context, string ticket = null)
        {
            AccessTokenList lst = string.IsNullOrEmpty(ticket) ? 
                _get_token_list(context) : RestAPI.get_token_list(ticket);
            if (lst == null) return string.Empty;

            lst.clear_expired_tokens();

            string curToken = _get_client_token(context);

            //the the Tokens list is empty, it means user has logged out, so a new token must be generated

            if (lst._Tokens.Count > 0 && (string.IsNullOrEmpty(curToken) || 
                !lst._Tokens.ContainsKey(curToken) || lst._Tokens[curToken].Expired)) return string.Empty;

            if (lst._Tokens.ContainsKey(curToken) && 
                !lst._Tokens[curToken].Expired && !lst._Tokens[curToken].Expiring) lst._Tokens[curToken].Use();

            return string.IsNullOrEmpty(ticket) ? new_token(context) : RestAPI.new_token(ticket);
        }

        public static bool check_token(HttpContext context, string ticket = null)
        {
            if (!string.IsNullOrEmpty(ticket))
                return RestAPI.get_user_id(ticket).HasValue;
            else
            {
                AccessTokenList lst = _get_token_list(context);
                if (lst == null) return false;

                string clientToken = _get_client_token(context);

                return !string.IsNullOrEmpty(clientToken) &&
                    lst._Tokens.ContainsKey(clientToken) && !lst._Tokens[clientToken].Expired;
            }
        }

        public static bool has_token(HttpContext context, string ticket = null)
        {
            if (!string.IsNullOrEmpty(ticket))
                return RestAPI.get_user_id(ticket).HasValue;
            else
            {
                AccessTokenList lst = _get_token_list(context);
                return lst != null && lst._Tokens.Count > 0;
            }
        }
    }
    
    public class RaaiVanUtil
    {
        public static void redirect_if_profile_not_completed(User user, System.Web.UI.Page Page)
        {
            string _currentPath = Path.GetFileName(Page.Request.FilePath);

            //if (_currentPath != System.IO.Path.GetFileName(PublicConsts.UserProfilePage) && (user == null ||
            if (_currentPath.ToLower() != "profile.aspx" && (user == null || !user.profileCompleted()))
                Page.Response.Redirect(PublicConsts.ProfilePage + "?Tab=Resume");
        }
        
        public static void redirect_if_profile_not_completed(Guid applicationId, Guid userId, System.Web.UI.Page Page)
        {
            User user = UsersController.get_user(applicationId, userId);
            redirect_if_profile_not_completed(user, Page);
        }

        public static void redirect_if_profile_not_completed(Guid applicationId, System.Web.UI.Page Page)
        {
            Guid userId = PublicMethods.get_current_user_id();
            redirect_if_profile_not_completed(applicationId, userId, Page);
        }

        public static void redirect_to_teams_page(System.Web.UI.Page Page)
        {
            List<string> validPages = new List<string>() { "applications.aspx", "help.aspx", "profile.aspx" };
            string _currentPath = Path.GetFileName(Page.Request.FilePath).ToLower();
            if (!validPages.Any(p => p == _currentPath)) Page.Response.Redirect(PublicConsts.ApplicationsPage);
        }

        public static bool new_user(Guid? applicationId, string username, string password, bool passAutoGenerated)
        {
            string thePass = password;
            while (thePass.Length <= 5) thePass += password;

            User _usr = new User()
            {
                UserName = username,
                Password = thePass,
                FirstName = string.Empty,
                LastName = string.Empty
            };

            return UsersController.create_user(applicationId, _usr, passAutoGenerated);
        }

        public static bool pre_login_check(Guid applicationId, ref string errorMessage)
        {
            if (!check_tenants())
            {
                errorMessage = Messages.TenantsCountExceededMaxAllowed.ToString();
                return false;
            }
            else if (!check_system_expiration())
            {
                errorMessage = Messages.SystemHasBeenExpired.ToString();
                return false;
            }
            else if (!check_users_count_lisence(applicationId))
            {
                errorMessage = Messages.ActiveUsersCountExceededMaxAllowed.ToString();
                return false;
            }

            return true;
        }

        public static void logged_in(Guid? applicationId, HttpContext context, 
            Guid userId, bool rememberMe, bool isSystemAdmin, ref string authCookie)
        {
            int authCookieLifeTime = isSystemAdmin ? RaaiVanSettings.AuthCookieLifeTimeForAdminInSeconds(applicationId) :
                RaaiVanSettings.AuthCookieLifeTimeInSeconds(applicationId);

            //FormsAuthentication.SetAuthCookie(username, rememberMe.HasValue && rememberMe.Value);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userId.ToString(), DateTime.Now,
                DateTime.Now.AddSeconds(authCookieLifeTime), rememberMe, string.Empty);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.Expires = ticket.Expiration;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            cookie.Secure = RaaiVanSettings.UseSecureAuthCookie(applicationId);
            context.Response.Cookies.Add(cookie);
            
            authCookie = "{\"Name\":\"" + Base64.encode(cookie.Name) + "\"" + 
                ",\"Value\":\"" + Base64.encode(cookie.Value) + "\"" +
                ",\"Expires\":\"" + cookie.Expires.ToString("r") + "\"" + //(r) equals this format: Mon, 13 Apr 2020 10:18:06 GMT
                ",\"Path\":\"" + Base64.encode(cookie.Path) + "\"" +
                ",\"Secure\":" + cookie.Secure.ToString().ToLower() +
                "}";

            int sessionTimeout = (RaaiVanSettings.MaxAllowedInactiveTimeInSeconds(applicationId) / 60);
            context.Session.Timeout = sessionTimeout == 0 ? 1440 : sessionTimeout; //1440 minutes equals a day
            
            AccessTokenList.new_token(context); //When someone logs in, at least one token must exist

            still_logged_in(applicationId, userId, context, true);

            set_session_expiration_time(context, authCookieLifeTime);
        }
        
        public static bool authenticate_user(Guid? applicationId, string username, string password, string activeDirDomain, 
            bool hasValidCaptcha, ref bool loggedInWithActiveDirectory, ref string failureText, 
            ref int remainingLockoutTime)
        {
            if (!PublicMethods.check_sys_id()) return false;
            
            loggedInWithActiveDirectory = false;

            try
            {
                bool restrictToActiveDir = username.ToLower() != "admin" &&
                    applicationId.HasValue && RaaiVanSettings.Users.RestrictPasswordsToActiveDirectory(applicationId.Value);

                failureText = Messages.UserNameAndOrPasswordIsNotValid.ToString();

                if (username.ToLower() == "system") return false;

                User theUser = UsersController.locked(applicationId, username);
                if (theUser == null) theUser = new User();
                
                if (theUser.UserID.HasValue)
                {
                    if (!theUser.IsApproved.HasValue || !theUser.IsApproved.Value)
                    {
                        failureText = "حساب کاربری شما غیر فعال شده است. برای فعال سازی آن به مدیریت سیستم مراجعه فرمایید";
                        return false;
                    }
                    else if (theUser.IsLockedOut.HasValue && theUser.IsLockedOut.Value && theUser.LastLockoutDate.HasValue)
                    {
                        TimeSpan offset = DateTime.Now - theUser.LastLockoutDate.Value;
                        int seconds = (int)offset.TotalSeconds;

                        if (hasValidCaptcha || seconds >= RaaiVanSettings.LoginLockoutDurationInSeconds(applicationId))
                            UsersController.unlock_user(theUser.UserID.Value);
                        else
                        {
                            remainingLockoutTime = RaaiVanSettings.LoginLockoutDurationInSeconds(applicationId) - seconds;
                            failureText = Messages.YourAccountIsLockedOutForNextNSeconds.ToString();
                            return false;
                        }
                    }
                }

                bool userExistsInActiveDirectory = false;
                bool validated = false;

                bool prevalidated = !restrictToActiveDir && UserUtilities.validate_user(applicationId, username, password);

                if (!prevalidated && !string.IsNullOrEmpty(activeDirDomain))
                {
                    string errorMessage = string.Empty;
                    
                    userExistsInActiveDirectory = UserUtilities.check_user_existence_in_active_directory(
                        applicationId, activeDirDomain, username, ref errorMessage);
                    
                    if (!userExistsInActiveDirectory)
                    {
                        if (restrictToActiveDir)
                        {
                            failureText = errorMessage;
                            return false;
                        }
                        else errorMessage = string.Empty;
                    }
                    
                    validated = loggedInWithActiveDirectory = userExistsInActiveDirectory &&
                        UserUtilities.is_active_directory_authenticated(applicationId, activeDirDomain, username, password, ref errorMessage);

                    /*
                    LogController.save_error_log(tenantId, null, "ActiveDirLogin",
                        "User: " + username + ", Pass: " + password + ", Result: " + validated.ToString().ToLower(), ModuleIdentifier.RV);
                    */

                    if (!validated && !string.IsNullOrEmpty(errorMessage))
                    {
                        failureText = errorMessage;
                        return false;
                    }

                    if (validated)
                    {
                        string newPass = password;
                        while (newPass.Length <= 5) newPass += password;

                        if (!theUser.UserID.HasValue)
                        {
                            if (!new_user(applicationId, username, newPass, passAutoGenerated: true))
                            {
                                failureText = "خطا در ایجاد کاربر جدید از سرویس دهنده شبکه، در اولین ورود کاربر";
                                return false;
                            }

                            theUser.UserID = UsersController.get_user_id(applicationId, username);
                        }
                        else if (theUser.UserID.HasValue && 
                            !UsersController.set_password(applicationId, theUser.UserID.Value, newPass, true, false, ref errorMessage))
                        {
                            failureText = "خطا در بروز رسانی رمز ورود از طریق سرویس دهنده شبکه";
                            return false;
                        }
                    }
                }
                
                bool? membershipValidated = null;

                if (theUser.UserID.HasValue && !(restrictToActiveDir && !userExistsInActiveDirectory) && (
                    prevalidated || validated || (
                        (membershipValidated = UserUtilities.validate_user(applicationId, username, password)).HasValue &&
                        membershipValidated.Value
                    )
                ))
                {
                    //Save Log
                    try
                    {
                        LogController.save_log(applicationId, new Log()
                        {
                            UserID = theUser.UserID.Value,
                            Date = DateTime.Now,
                            HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                            HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                            Action = Modules.Log.Action.Login,
                            SubjectID = theUser.UserID.Value,
                            ModuleIdentifier = ModuleIdentifier.USR
                        });
                    }
                    catch { }
                    //end of Save Log

                    return true;
                }
                else if (theUser.UserID.HasValue && membershipValidated.HasValue && !membershipValidated.Value)
                {
                    //Save Log
                    try
                    {
                        Guid? uId = UsersController.get_user_id(applicationId, username);

                        MembershipUser usr = !uId.HasValue ? null : PublicMethods.get_user(uId.Value);
                        bool locked = usr != null && usr.IsLockedOut;

                        if (locked)
                        {
                            LogController.save_log(applicationId, new Log()
                            {
                                UserID = theUser.UserID.Value,
                                Date = DateTime.Now,
                                HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                                HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                                Action = Modules.Log.Action.UserLockedOut,
                                SubjectID = theUser.UserID.Value,
                                ModuleIdentifier = ModuleIdentifier.USR
                            });
                        }
                    }
                    catch { }
                    //end of Save Log
                }
            }
            catch (Exception ex) { failureText = ex.ToString(); }

            return false;
        }

        public static bool login(Guid? applicationId, string username, string password, string activeDirDomain, bool? rememberMe,
            Guid? invitationId, bool hasValidCaptcha, ref string failureText, ref int remainingLockoutTime, ref bool stepTwoNeeded,
            ref Guid? userId, ref HttpContext context, ref string authCookie)
        {
            if (!RaaiVanSettings.SAASBasedMultiTenancy && !applicationId.HasValue) return false;

            /* Usernames like 'username@domain' or 'domain\username' or 'domain/username' are not allowed */
            if (!string.IsNullOrEmpty(username) && !RaaiVanSettings.SAASBasedMultiTenancy)
            {
                if (username.IndexOf("@") >= 0) username = username.Substring(0, username.IndexOf("@"));
                if (username.LastIndexOf("/") >= 0) username = username.Substring(username.LastIndexOf("/") + 1);
                if (username.LastIndexOf("\\") >= 0) username = username.Substring(username.LastIndexOf("\\") + 1);
            }

            userId = UsersController.get_user_id(applicationId, username);
            
            if (username.ToLower() == "admin") activeDirDomain = string.Empty;
            else if (applicationId.HasValue && !pre_login_check(applicationId.Value, ref failureText)) return false;

            bool loggedInWithActiveDirectory = false;

            if (!authenticate_user(applicationId, username, password, activeDirDomain, hasValidCaptcha,
                ref loggedInWithActiveDirectory, ref failureText, ref remainingLockoutTime))
            {
                return false;
            }
            else if (!userId.HasValue) userId = UsersController.get_user_id(applicationId, username);

            if (!userId.HasValue) return false;
            
            //Two step authentication
            if (RaaiVanSettings.Users.EnableTwoStepAuthenticationViaEmail(applicationId) ||
                RaaiVanSettings.Users.EnableTwoStepAuthenticationViaSMS(applicationId))
            {
                Guid? mainEmailId = !RaaiVanSettings.Users.EnableTwoStepAuthenticationViaEmail(applicationId) ? null :
                    UsersController.get_main_email(userId.Value);
                Guid? mainPhoneId = !RaaiVanSettings.Users.EnableTwoStepAuthenticationViaSMS(applicationId) ? null :
                    UsersController.get_main_phone(userId.Value);

                EmailAddress email = !mainEmailId.HasValue ? null : UsersController.get_email_address(mainEmailId.Value);
                PhoneNumber number = !mainPhoneId.HasValue ? null : UsersController.get_phone_number(mainPhoneId.Value);

                string emailAddress = email == null ? null : email.Address;
                string phoneNumber = number == null ? null : number.Number;

                if (!string.IsNullOrEmpty(emailAddress) || !string.IsNullOrEmpty(phoneNumber))
                {
                    TwoStepAuthenticationToken twoStepToken = new TwoStepAuthenticationToken(applicationId, userId.Value,
                        !loggedInWithActiveDirectory, emailAddress, phoneNumber);
                    
                    if (twoStepToken.send_code())
                    {
                        failureText = "{\"TwoStepAuthentication\":" + true.ToString().ToLower() +
                            ",\"Data\":" + twoStepToken.toJson() + "}";

                        stepTwoNeeded = true;
                    }
                    else failureText = Messages.SendingTwoStepAuthenticationCodeFailed.ToString();

                    return false;
                }
            }
            //end of Two step authentication

            if (RaaiVanSettings.PreventConcurrentSessions(applicationId) && is_logged_in(userId.Value))
            {
                failureText = Messages.YouAreAlreadyLoggedIn.ToString();
                return false;
            }

            after_login_procedures(applicationId, userId.Value, rememberMe.HasValue && rememberMe.Value,
                invitationId, loggedInWithActiveDirectory, context, ref authCookie);

            return true;
        }

        public static bool resend_authentication_code(string token)
        {
            return true;
        }

        public static bool login_step_two(Guid? applicationId, string token, long? code, bool? rememberMe, Guid? invitationId,
            ref string failureText, ref bool disposed, ref Guid? userId, ref HttpContext context, ref string authCookie)
        {
            TwoStepAuthenticationToken twoStepToken = !code.HasValue ? null :
                (TwoStepAuthenticationToken)TwoStepAuthenticationToken.validate(token, code.Value, ref disposed);

            if (twoStepToken == null) {
                failureText = Messages.AuthenticationCodeDidNotMatch.ToString();
                return false;
            }
            
            userId = twoStepToken.UserID.Value;

            after_login_procedures(applicationId, twoStepToken.UserID.Value, rememberMe.HasValue && rememberMe.Value,
                invitationId, !twoStepToken.WasNormalUserPassLogin, context, ref authCookie);

            return true;
        }

        public static void after_login_procedures(Guid? tenantId, Guid userId, bool rememberMe, Guid? invitationId,
            bool loggedInWithActiveDirectory, HttpContext context, ref string authCookie) {
            bool isSystemAdmin = PublicMethods.is_system_admin(tenantId, userId, true);

            logged_in(tenantId, context, userId, rememberMe, isSystemAdmin, ref authCookie);

            if (!isSystemAdmin && (RaaiVanSettings.Users.ForceChangeFirstPassword(tenantId) ||
                RaaiVanSettings.Users.PasswordLifeTimeInDays(tenantId) > 0))
            {
                bool passwordChangeNeeded = false;
                bool firstPassword = false, passwordExpired = false;

                if (!loggedInWithActiveDirectory && userId != Guid.Empty)
                {
                    List<Password> lasts = UsersController.get_last_passwords(userId, autoGenerated: null, count: 1);

                    if (lasts.Count == 1 && lasts[0].AutoGenerated.HasValue &&
                        lasts[0].AutoGenerated.Value && RaaiVanSettings.Users.ForceChangeFirstPassword(tenantId))
                        passwordChangeNeeded = firstPassword = true;
                    else if (RaaiVanSettings.Users.PasswordLifeTimeInDays(tenantId) > 0)
                    {
                        if (lasts.Count == 0) passwordChangeNeeded = passwordExpired = true;
                        else
                        {
                            DateTime? pd = UsersController.get_last_password_date(userId);

                            if (!pd.HasValue ||
                                (DateTime.Now - pd.Value).Days > RaaiVanSettings.Users.PasswordLifeTimeInDays(tenantId))
                                passwordChangeNeeded = passwordExpired = true;
                        }
                    }

                    if (passwordChangeNeeded)
                        RaaiVanUtil.password_change_needed(HttpContext.Current, firstPassword, passwordExpired);
                }
            }

            init_user_application(invitationId, userId);
        }

        public static void logout(Guid? applicationId)
        {
            Guid userId = PublicMethods.get_current_user_id();

            //Save Log
            try
            {
                LogController.save_log(applicationId, new Log()
                {
                    UserID = userId,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.Logout,
                    SubjectID = userId,
                    ModuleIdentifier = ModuleIdentifier.USR
                });
            }
            catch { }
            //end of Save Log

            if (HttpContext.Current.Response.Cookies["ASP.NET_SessionId"] != null)
                HttpContext.Current.Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1);

            FormsAuthentication.SignOut();

            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Clear();
                HttpContext.Current.Session.Abandon();
            }
            
            logged_out(userId);
        }

        protected static bool sso_auto_redirect(ParamsContainer paramsContainer, string loginUrl, bool skipRedirect)
        {
            if (RaaiVanSettings.SSO.AutoRedirect(paramsContainer.ApplicationID))
            {
                if (!skipRedirect) paramsContainer.redirect(loginUrl);
                return true;
            }
            else return false;
        }

        public static bool sso_login(ParamsContainer paramsContainer, bool skipRedirect,
            ref string errorMessage, ref Guid? userId, ref string authCookie, ref bool shouldRedirect)
        {
            if (paramsContainer.Tenant != null &&
                !RaaiVanUtil.pre_login_check(paramsContainer.ApplicationID.Value, ref errorMessage)) return false;

            string loginUrl = Modules.Jobs.SSO.get_login_url(paramsContainer.ApplicationID);

            if (string.IsNullOrEmpty(loginUrl)) return false;

            string ticket = Modules.Jobs.SSO.get_ticket(paramsContainer.ApplicationID, paramsContainer.Context);

            string username = string.Empty;

            if (string.IsNullOrEmpty(ticket) ||
                !Modules.Jobs.SSO.validate_ticket(paramsContainer.ApplicationID, HttpContext.Current, ticket, ref username))
                return (shouldRedirect = sso_auto_redirect(paramsContainer, loginUrl, skipRedirect));

            if (!string.IsNullOrEmpty(username))
            {
                userId = UsersController.get_user_id(paramsContainer.ApplicationID, username);

                if (!userId.HasValue && !RaaiVanUtil.new_user(paramsContainer.ApplicationID, username, username, false))
                {
                    errorMessage = Messages.UserCreationFailed.ToString();
                    return false;
                }
                else if (!userId.HasValue)
                    userId = UsersController.get_user_id(paramsContainer.ApplicationID, username);

                if (userId.HasValue)
                {
                    RaaiVanUtil.logged_in(paramsContainer.ApplicationID, HttpContext.Current, userId.Value, false, false, ref authCookie);

                    //Save Log
                    try
                    {
                        LogController.save_log(paramsContainer.ApplicationID, new Log()
                        {
                            UserID = userId,
                            Date = DateTime.Now,
                            HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                            HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                            Action = Modules.Log.Action.Login,
                            SubjectID = userId,
                            ModuleIdentifier = ModuleIdentifier.USR
                        });
                    }
                    catch { }
                    //end of Save Log

                    return true;
                }
                else
                {
                    errorMessage = Messages.RetrievingUserFailed.ToString();
                    return false;
                }
            }

            errorMessage = Messages.LoginFailed.ToString();

            return false;
        }

        public static void init_user_application(Guid? invitationId, Guid userId) {
            if (RaaiVanSettings.SAASBasedMultiTenancy)
            {
                Guid? invAppId = !invitationId.HasValue ? null :
                    UsersController.get_invitation_application_id(invitationId.Value, checkIfNotUsed: true);
                if (invAppId.HasValue) GlobalController.add_user_to_application(invAppId.Value, userId);

                List<Application> apps = GlobalController.get_user_applications(userId);
                if (apps != null && apps.Count == 1) PublicMethods.set_current_application(apps[0]);
                else if (apps != null && invAppId.HasValue && apps.Any(a => a.ApplicationID == invAppId))
                    PublicMethods.set_current_application(apps.Where(a => a.ApplicationID == invAppId).FirstOrDefault());
            }
        }

        public static bool check_users_count_lisence(Guid applicationId)
        {
            return !GlobalSettings.MaxAllowedActiveUsersCount.HasValue || 
                UsersController.get_users_count(applicationId) <= GlobalSettings.MaxAllowedActiveUsersCount.Value;
        }

        public static bool check_system_expiration()
        {
            return !GlobalSettings.SystemExpirationDate.HasValue || DateTime.Now <= GlobalSettings.SystemExpirationDate.Value;
        }

        public static bool check_tenants()
        {
            return RaaiVanSettings.Tenants.Count <= GlobalSettings.MaxTenantsCount;
            
            //return RaaiVanSettings.Tenants.Count <= GlobalSettings.MaxTenantsCount && 
            //    RaaiVanSettings.Tenants.Count <= GlobalController.get_application_ids().Count;
        }
        
        public static bool is_authenticated(Guid? applicationId, HttpContext context)
        {
            //if (string.IsNullOrEmpty(get_access_token(context))) set_access_token(context);
            bool authenticated = context.User.Identity.IsAuthenticated;

            if (session_expired(context) || (authenticated && !AccessTokenList.has_token(context)))
            {
                logout(applicationId);
                return false;
            }

            return authenticated;
        }

        private static void set_session_expiration_time(HttpContext context, int expiresAfterNSeconds) {
            try
            {
                string variableName = "AuthCookieExpirationTime";

                if (context.Session != null && context.Session[variableName] == null)
                    context.Session[variableName] = DateTime.Now.AddSeconds(expiresAfterNSeconds);
            }
            catch { }
        }

        private static void set_session_last_activity_time(Guid? applicationId, HttpContext context)
        {
            try
            {
                string variableName = "LastActivityBasedExpirationTime";

                if (context.Session != null)
                {
                    context.Session[variableName] = 
                        DateTime.Now.AddSeconds(RaaiVanSettings.MaxAllowedInactiveTimeInSeconds(applicationId));
                }
            }
            catch { }
        }

        private static bool session_expired(HttpContext context)
        {
            try
            {
                string variableName = "AuthCookieExpirationTime";
                string activityVariableName = "LastActivityBasedExpirationTime";

                bool expired =  context.Session != null && context.Session[variableName] != null &&
                    (DateTime)context.Session[variableName] <= DateTime.Now;

                bool maxAllowedInactiveTimeExceeded = expired || (context.Session != null &&
                    context.Session[activityVariableName] != null &&
                    (DateTime)context.Session[activityVariableName] <= DateTime.Now);
                
                return expired || maxAllowedInactiveTimeExceeded;
            }
            catch { return false; }
        }

        public static bool _password_change_needed(HttpContext context, bool? needed, 
            bool firstPassword, bool passwordExpired, ref string reason)
        {
            if (context.Session == null) return false;

            string paramName = "PasswordChangeNeeded";
            string reasonParamName = "PasswordChangeReason";

            if (!needed.HasValue)
            {
                reason = context.Session[reasonParamName] == null ?
                    string.Empty : (string)context.Session[reasonParamName];
                return context.Session[paramName] != null && (bool)context.Session[paramName];
            }

            context.Session[paramName] = needed.Value;

            if (!needed.Value && context.Session[reasonParamName] != null)
                context.Session[reasonParamName] = null;
            else if (firstPassword || passwordExpired)
                context.Session[reasonParamName] = firstPassword ? "FirstPassword" : "PasswordExpired";

            return true;
        }

        public static bool password_change_not_needed(HttpContext context)
        {
            string str = string.Empty;
            return _password_change_needed(context, false, false, false, ref str);
        }

        public static bool password_change_needed(HttpContext context, bool firstPassword, bool passwordExpired)
        {
            string str = string.Empty;
            return _password_change_needed(context, true, firstPassword, passwordExpired, ref str);
        }

        public static bool password_change_needed(HttpContext context, ref string reason)
        {
            return _password_change_needed(context, null, false, false, ref reason);
        }

        private static SortedList<Guid, DateTime> _LoggedInUsers = new SortedList<Guid, DateTime>();

        public static void still_logged_in(Guid? applicationId, Guid userId, HttpContext context, bool hasActivity)
        {
            if (userId != Guid.Empty)
            {
                lock (_LoggedInUsers) { _LoggedInUsers[userId] = DateTime.Now.AddMinutes(5); }

                UsersController.set_last_activity_date(userId);

                if (hasActivity) set_session_last_activity_time(applicationId, context);
            }
        }
        
        public static bool is_logged_in(Guid userId)
        {
            return _LoggedInUsers.ContainsKey(userId) && _LoggedInUsers[userId] > DateTime.Now;
        }

        public static void logged_out(Guid userId)
        {
            if (_LoggedInUsers.ContainsKey(userId))
                lock (_LoggedInUsers) { _LoggedInUsers.Remove(userId); }
        }

        private static SortedList<Guid, bool> _Initialized = new SortedList<Guid, bool>();

        public static void initialize(Guid? applicationId)
        {
            if (!applicationId.HasValue) return;

            if (_Initialized.ContainsKey(applicationId.Value)) return;
            _Initialized.Add(applicationId.Value, true);

            if (!RaaiVanSettings.SAASBasedMultiTenancy)
            {
                UserUtilities.create_admin_user(applicationId.Value);
                CNController.initialize(applicationId.Value);
                KnowledgeController.initialize(applicationId.Value);
            }

            PrivacyController.initialize_confidentiality_levels(applicationId.Value);
            PrivacyController.refine_access_roles(applicationId.Value);
        }

        public static string get_login_page_info(ParamsContainer paramsContainer, string info)
        {
            if (!paramsContainer.ApplicationID.HasValue) return string.Empty;

            if (info.ToLower().Contains("wfabstract"))
            {

                string[] strs = info.Split(':');
                if (strs.Length < 3) return string.Empty;
                Guid workflowId = Guid.Empty;
                Guid nodeTypeId = Guid.Empty;
                if (!Guid.TryParse(strs[1], out workflowId)) return string.Empty;
                if (!Guid.TryParse(strs[2], out nodeTypeId)) return string.Empty;
                return "\"" + strs[0] + "\":" + get_login_page_service_abstract(paramsContainer, workflowId, nodeTypeId);
            }
            else if (info.ToLower().Contains("modern_28_1"))
            {
                Dictionary<string, object> statistics = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id, null, null);
                Dictionary<string, object> lastMonth = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id,
                    DateTime.Now.AddDays(-30), null);

                if (lastMonth.ContainsKey("ActiveUsersCount")) statistics["ActiveUsersCount"] = lastMonth["ActiveUsersCount"];

                statistics["OnlineUsersCount"] = Membership.GetNumberOfUsersOnline();

                ArrayList lst = GlobalController.get_last_content_creators(paramsContainer.Tenant.Id, 10);

                for (int i = 0, lnt = lst.Count; i < lnt; ++i)
                {
                    Dictionary<string, object> item = (Dictionary<string, object>)lst[i];

                    if (item.ContainsKey("UserID")) item["ProfileImageURL"] =
                            DocumentUtilities.get_personal_image_address(paramsContainer.Tenant.Id, (Guid)item["UserID"]);
                    if (item.ContainsKey("Date"))
                    {
                        DateTime dt = (DateTime)item["Date"];
                        item["Date"] = PublicMethods.get_local_date(dt);
                        item["Date_Gregorian"] = dt.ToString();
                    }

                    lst[i] = item;
                }

                Dictionary<string, object> dic = new Dictionary<string, object>();

                dic["Users"] = lst;
                dic["Statistics"] = statistics;

                return "\"modern_28_1\":" + PublicMethods.toJSON(dic);
            }
            else if (info.ToLower().Contains("modern_29_1"))
            {
                Dictionary<string, object> statistics = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id, null, null);
                Dictionary<string, object> lastMonth = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id,
                    DateTime.Now.AddDays(-30), null);
                Dictionary<string, object> previousMonth = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id,
                    DateTime.Now.AddDays(-60), DateTime.Now.AddDays(-30));

                Dictionary<string, object> dic = new Dictionary<string, object>();
                Dictionary<string, object> stats = new Dictionary<string, object>();

                stats["Total"] = statistics;
                stats["LastMonth"] = lastMonth;
                stats["PreviousMonth"] = previousMonth;

                dic["Statistics"] = stats;

                return "\"modern_29_1\":" + PublicMethods.toJSON(dic);
            }

            return string.Empty;
        }

        private static string get_login_page_service_abstract(ParamsContainer paramsContainer, Guid workflowId, Guid nodeTypeId)
        {
            string result = string.Empty;

            new WFAPI() { paramsContainer = paramsContainer }
                .get_service_abstract(nodeTypeId, workflowId, null, "NoTag", true, ref result);

            return result;
        }
    }

    public class ParamsContainer
    {
        private HttpContext _Context;
        private string _Ticket;

        private static SortedList<string, bool> LastRequests = 
            new SortedList<string, bool>(RaaiVanSettings.ReplayAttackQueueLength);
        private static Queue<string> LastRequestsQueue = new Queue<string>(RaaiVanSettings.ReplayAttackQueueLength);

        private Dictionary<string, string> RequestParams;

        public ITenant Tenant { get; private set; }

        public Guid? ApplicationID
        {
            get { return Tenant == null ? null : (Guid?)Tenant.Id; }
        }

        public HttpContext Context {
            get { return _Context; }
        }

        private bool _RefreshAccessToken;

        private bool? _is_replay_attack;
        private bool is_replay_attack
        {
            get
            {
                if (RaaiVanSettings.SAASBasedMultiTenancy) return false;

                if (_is_replay_attack.HasValue) return _is_replay_attack.Value;

                string hash = PublicMethods.sha1(_Context.Request.Path + "?" + _Context.Request.Params.ToString());

                _is_replay_attack = LastRequests.ContainsKey(hash);

                if (_is_replay_attack.Value) return true;

                lock (LastRequestsQueue)
                {
                    lock (LastRequests)
                    {
                        LastRequests[hash] = true;
                        LastRequestsQueue.Enqueue(hash);

                        while (LastRequestsQueue.Count > RaaiVanSettings.ReplayAttackQueueLength)
                        {
                            string first = LastRequestsQueue.FirstOrDefault();
                            if (LastRequests.ContainsKey(first)) LastRequests.Remove(first);
                            LastRequestsQueue.Dequeue();
                        }
                    }
                }

                return _is_replay_attack.Value;
            }
        }

        public bool IsAuthenticated;
        public Guid? CurrentUserID;
        
        private bool? _GBEdit;
        public bool GBEdit
        {
            get
            {
                if (!_GBEdit.HasValue)
                {
                    _GBEdit = IsAuthenticated && CurrentUserID.HasValue && CurrentUserID.Value != Guid.Empty &&
                        AccessTokenList.check_token(_Context, _Ticket) && !is_replay_attack;
                }
                
                return _GBEdit.Value;
            }
        }

        private bool? _GBView;
        public bool GBView
        {
            get
            {
                if (!_GBView.HasValue)
                    _GBView = (RaaiVanSettings.AllowNotAuthenticatedUsers(Tenant.Id) || GBEdit) && !is_replay_attack;
                return _GBView.Value;
            }
        }

        private bool? _XMLFile;
        private bool XMLFile
        {
            get { return _XMLFile.HasValue && _XMLFile.Value; }
            set { _XMLFile = value; }
        }

        public ParamsContainer()
        {
            _Ticket = null;
            IsAuthenticated = false;
            Tenant = null;
            RequestParams = new Dictionary<string, string>();
        }

        public ParamsContainer(HttpContext context, bool nullTenantResponse = false)
        {
            //basic checks
            _Context = context;
            Tenant = context.GetCurrentTenant();

            if (Tenant == null && nullTenantResponse)
            {
                return_response(PublicConsts.NullTenantResponse);
                return;
            }
            else if (RaaiVanSettings.USBToken && !USBToken.StillConnected)
            {
                return_response(PublicConsts.USBTokenNotFoundResponse);
                return;
            }
            //end of basic checks

            //Init RequestParams
            try
            {
                RequestParams = new Dictionary<string, string>();

                MemoryStream memstream = new MemoryStream();
                context.Request.InputStream.CopyTo(memstream);
                memstream.Position = 0;
                using (StreamReader reader = new StreamReader(memstream))
                {
                    Dictionary<string, object> tmpDic = PublicMethods.fromJSON(reader.ReadToEnd());
                    if (tmpDic != null) tmpDic.Keys.Where(key => tmpDic[key] != null).ToList()
                            .ForEach(key => RequestParams[key.ToLower()] = tmpDic[key].ToString());
                }
            }
            catch { }
            //end of Init RequestParams

            string ticket = _Ticket = !Modules.RaaiVanConfig.Modules.RestAPI(ApplicationID) ? 
                string.Empty : PublicMethods.parse_string(request_param("Ticket"), false);

            bool isStateLess = !string.IsNullOrEmpty(ticket) && (CurrentUserID = RestAPI.get_user_id(ticket)).HasValue;

            if (!string.IsNullOrEmpty(ticket) && !isStateLess) {
                return_response(PublicConsts.InvalidTicketResponse);
                return;
            }
            else if (isStateLess)
            {
                IsAuthenticated = true;

                Guid? uId = PublicMethods.parse_guid(context.Request.Params["CurrentUserID"]);
                if (uId.HasValue) CurrentUserID = uId;
            }
            else
            {
                IsAuthenticated = RaaiVanUtil.is_authenticated(ApplicationID, context);
                CurrentUserID = IsAuthenticated ? PublicMethods.get_current_user_id() : Guid.Empty;
                if (CurrentUserID == Guid.Empty) CurrentUserID = null;
            }
            
            _XMLFile = !string.IsNullOrEmpty(context.Request.Params["XMLFile"]) &&
                context.Request.Params["XMLFile"].ToLower() == "true";
            
            _RefreshAccessToken = !string.IsNullOrEmpty(ticket) || 
                (!string.IsNullOrEmpty(context.Request.Params["RefreshAccessToken"]) &&
                    context.Request.Params["RefreshAccessToken"].ToLower() == "true");

            if (!isStateLess && CurrentUserID.HasValue)
            {
                bool hasActivity = string.IsNullOrEmpty(context.Request.Params["Command"]) ||
                    !PublicMethods.parse_string(context.Request.Params["Command"], false).ToLower().EndsWith("count");
                
                RaaiVanUtil.still_logged_in(ApplicationID, CurrentUserID.Value, context, hasActivity);
            }
        }

        public string request_param(string name) {
            if (string.IsNullOrEmpty(name)) return null;
            else if (RequestParams != null && RequestParams.ContainsKey(name.ToLower())) return RequestParams[name.ToLower()];
            else if (_Context != null) return _Context.Request.Params[name];
            else return null;
        }

        protected void _save_log(Modules.Log.Action action)
        {
            string postData = string.Empty, getData = string.Empty;
            string command = string.Empty;

            for (int i = 0, lnt = _Context.Request.Form.Keys.Count; i < lnt; ++i)
            {
                string key = _Context.Request.Form.Keys[i];
                if (key.ToLower() == "command" && string.IsNullOrEmpty(command))
                {
                    command = _Context.Request.Form[key];
                    if (!string.IsNullOrEmpty(command) &&
                        System.Text.RegularExpressions.Regex.IsMatch(command.ToLower(), @"^get[\w]*count$"))
                        return;
                }
                postData += (string.IsNullOrEmpty(postData) ? string.Empty : ",") +
                    "\"" + key + "\":\"" + Base64.encode(_Context.Request.Form[key]) + "\"";
            }

            for (int i = 0, lnt = _Context.Request.QueryString.Keys.Count; i < lnt; ++i)
            {
                string key = _Context.Request.QueryString.Keys[i];
                if (key.ToLower() == "command" && string.IsNullOrEmpty(command)) command = _Context.Request.QueryString[key];
                getData += (string.IsNullOrEmpty(getData) ? string.Empty : ",") +
                    "\"" + key + "\":\"" + Base64.encode(_Context.Request.QueryString[key]) + "\"";
            }

            LogController.save_log(ApplicationID, new Log()
            {
                UserID = CurrentUserID,
                HostAddress = PublicMethods.get_client_ip(_Context),
                HostName = PublicMethods.get_client_host_name(_Context),
                Action = action,
                Info = "{\"Path\":\"" + _Context.Request.Path + "\"" +
                    ",\"Command\":\"" + command + "\"" + ",\"GetData\":{" + getData + "},\"PostData\":{" + postData + "}" + "}",
                ModuleIdentifier = ModuleIdentifier.RV
            });
        }
        
        public void return_response(ref string responseText)
        {
            if (_Context == null || _Context.Response == null) return;

            if (_is_replay_attack.HasValue && _is_replay_attack.Value)
            {
                responseText = "{\"ErrorText\":\"" + Messages.PotentialReplayAttackDetected + "\"}";

                _save_log(Modules.Log.Action.PotentialReplayAttack);
            }
            else if (!IsAuthenticated && string.IsNullOrEmpty(responseText))
            {
                responseText = PublicConsts.NotAuthenticatedResponse;

                _save_log(Modules.Log.Action.NotAuthorizedAnonymousRequest);
            }
            //check_token is computed in GBEdit and CSRF Attack is important only in APIs with GBEdit permission
            else if (_GBEdit.HasValue && !AccessTokenList.check_token(_Context, _Ticket))
            {
                responseText = "{\"ErrorText\":\"" + Messages.PotentialCSRFAttackDetected + "\"}";

                _save_log(Modules.Log.Action.PotentialCSRFAttack);
            }

            if (_RefreshAccessToken)
            {
                string newToken = AccessTokenList.refresh_token(_Context, _Ticket);

                if (string.IsNullOrEmpty(responseText) || responseText[0] != '{' ||
                    responseText[responseText.Length - 1] != '}')
                {
                    responseText = "{\"AccessToken\":\"" + newToken + "\"" +
                        (ApplicationID.HasValue ? ",\"AppID\":\"" + ApplicationID.ToString() + "\"" : string.Empty) +
                        ",\"ResponseIsNotJSON\":" + true.ToString().ToLower() +
                        ",\"Response\":\"" + Base64.encode(responseText) + "\"}";
                }
                else
                {
                    responseText = responseText.Substring(0, responseText.Length - 1) +
                        (responseText.Length > 4 ? "," : string.Empty) + "\"AccessToken\":\"" + newToken + "\"" + 
                        (ApplicationID.HasValue ? ",\"AppID\":\"" + ApplicationID.ToString() + "\"" : string.Empty) + "}";
                }
            }

            if (XMLFile)
            {
                if (responseText[0] == '[' && responseText[responseText.Length - 1] == ']')
                    responseText = "{\"Items\":" + responseText + "}";
                else if (responseText[0] != '{') responseText = "{\"Value\":\"" + responseText + "\"}";

                xml_response(PublicMethods.json2xml(responseText, "Root", "{\"Error\":\"[Exception]\"}"));
            }
            else
            {
                try
                {
                    _Context.Response.Clear();

                    _Context.Response.ContentType = "text/json";
                    _Context.Response.BufferOutput = true;

                    _Context.Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                    _Context.Response.AppendHeader("Pragma", "no-cache");
                    _Context.Response.AppendHeader("Expires", "0");

                    _Context.Response.Write(responseText);

                    _Context.Response.Flush(); // Sends all currently buffered output to the client.
                    _Context.Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
                    _Context.ApplicationInstance.CompleteRequest(); // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event.
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                }

                /*
                _Context.Response.End();
                _Context.Response.Close();
                */
            }
        }

        public void return_response(string responseText)
        {
            return_response(ref responseText);
        }

        public void xml_response(XmlDocument xDoc)
        {
            if (xDoc == null) xDoc = new XmlDocument();

            _Context.Response.Clear();
            _Context.Response.ClearContent();
            _Context.Response.ClearHeaders();
            _Context.Response.Buffer = true;
            _Context.Response.ContentType = "application/xml";
            _Context.Response.ContentEncoding = Encoding.UTF8;

            _Context.Response.Expires = -1;
            _Context.Response.Cache.SetAllowResponseInBrowserHistory(true); //works around an ie bug

            xDoc.Save(_Context.Response.Output);

            try
            {
                _Context.Response.Flush();
                _Context.Response.End();
            }
            catch(Exception ex) {
                string strEx = ex.ToString();
            }
        }

        public void xml_response(string responseText)
        {
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            xDoc.LoadXml(responseText);
            xml_response(xDoc);
        }

        public void file_response(byte[] byteArray, string fileName, string contentType = null, bool isAttachment = true)
        {
            _Context.Response.Clear();
            _Context.Response.ClearContent();
            _Context.Response.ClearHeaders();
            _Context.Response.Buffer = true;
            _Context.Response.ContentType = string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType;
            if (isAttachment) _Context.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName.Replace(' ', '_'));
            _Context.Response.AddHeader("Content-Length", byteArray.Length.ToString());

            _Context.Response.BinaryWrite(byteArray);

            /*
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                memoryStream.WriteTo(_Context.Response.OutputStream);
                memoryStream.Close();
            }
            */

            try
            {
                _Context.Response.Flush();
                _Context.Response.End();
            }
            catch (Exception ex)
            {
                string strEx = ex.ToString();
            }
        }

        public void file_response(string content, string fileName, string contentType = null, bool isAttachment = true)
        {
            _Context.Response.Clear();
            _Context.Response.ClearContent();
            _Context.Response.ClearHeaders();
            _Context.Response.Buffer = true;
            _Context.Response.ContentType = string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType;
            if (isAttachment) _Context.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName.Replace(' ', '_'));
            _Context.Response.AddHeader("Content-Length", content.Length.ToString());

            _Context.Response.Write(content);

            try
            {
                _Context.Response.Flush();
                _Context.Response.End();
            }
            catch (Exception ex)
            {
                string strEx = ex.ToString();
            }
        }

        public void redirect_to_login_page() {
            FormsAuthentication.RedirectToLoginPage();

            /*
            string path = _Context == null || _Context.Request == null || _Context.Request.Url == null ? null :
                _Context.Request.Url.AbsolutePath;

            if (string.IsNullOrEmpty(path)) FormsAuthentication.RedirectToLoginPage();
            else FormsAuthentication.RedirectToLoginPage("ReturnUrl=" + HttpUtility.UrlEncode(path));
            */
        }

        public void redirect(string url)
        {
            if (_Context != null) _Context.Response.Redirect(url);
        }
    }

    public class HelpIndexEntry
    {
        public string Name;
        public string Content;
        public string Module;
        public int SequenceNumber;
        private List<HelpIndexEntry> _Sub;

        public List<HelpIndexEntry> Sub
        {
            get { return _Sub; }
        }

        public HelpIndexEntry()
        {
            Name = Content = Module = string.Empty;
            SequenceNumber = 0;
            _Sub = null;
        }

        public void add_sub(HelpIndexEntry entry)
        {
            if (entry != null)
            {
                if (_Sub == null) _Sub = new List<HelpIndexEntry>();
                _Sub.Add(entry);
            }
        }

        public void set_sub(List<HelpIndexEntry> sub)
        {
            _Sub = sub;
        }

        public string toJson(bool content, bool sub)
        {
            return "{\"Name\":\"" + Base64.encode(Name) + "\"" +
                ",\"Module\":\"" + (string.IsNullOrEmpty(Module) ? string.Empty : Module) + "\"" +
                (!content || string.IsNullOrEmpty(Content) ? string.Empty : ",\"Content\":\"" + Base64.encode(Content) + "\"") +
                (!sub || Sub == null || Sub.Count == 0 ? string.Empty :
                    ",\"Sub\":[" + string.Join(",", Sub.Select(u => u.toJson(content, sub))) + "]"
                ) +
                "}";
        }
    }

    public class RaaiVanHelp
    {
        private static Dictionary<string, List<HelpIndexEntry>> Index = new Dictionary<string, List<HelpIndexEntry>>();
        private static Dictionary<string, Dictionary<string, HelpIndexEntry>> Dic = new Dictionary<string, Dictionary<string, HelpIndexEntry>>();

        private static Dictionary<string, bool> _Initialized = new Dictionary<string, bool>();

        private static void _init(string lang)
        {
            lang = lang.ToLower();

            if (_Initialized.ContainsKey(lang)) return;
            _Initialized[lang] = true;

            Dic[lang] = new Dictionary<string, HelpIndexEntry>();

            //create index

            DirectoryInfo dir = new DirectoryInfo(PublicMethods.map_path("~/Help/" + lang));
            DirectoryInfo[] sub = dir.GetDirectories();

            List<HelpIndexEntry> lst = new List<HelpIndexEntry>();

            for (int i = 0, lnt = sub.Length; i < lnt; ++i)
            {
                if (_check_entry_folder_name(sub[i]))
                {
                    HelpIndexEntry entry = create_index_entry(lang, sub[i]);
                    if (entry != null) lst.Add(entry);
                }
            }

            lst = lst.OrderBy(u => u.SequenceNumber).ToList();

            if (lst.Count > 0) Index[lang] = lst;
            else Index = null;
            //end of create index
        }

        private static bool _check_entry_folder_name(DirectoryInfo dir)
        {
            return dir != null && dir.Name.ToLower() != "media"; 
                /* && (new Regex("^\\d+")).IsMatch(dir.Name) && !(new Regex("^0+[\\s-]")).IsMatch(dir.Name); */
        }

        private static string resolve_entry_name(string lang, string folderPath)
        {
            folderPath = folderPath.ToLower();

            string prefix = ("help\\" + lang).ToLower();

            return string.Join("_", folderPath.Substring(folderPath.IndexOf(prefix) + prefix.Length)
                .Split('\\').Where(p => !string.IsNullOrEmpty(p) && p != "sub").Select(p =>
                {
                    int ind = p.IndexOf('-');
                    return ind <= 0 ? p : p.Substring(0, ind).Trim();
                }));
        }

        private static HelpIndexEntry create_index_entry(string lang, DirectoryInfo dir)
        {
            lang = lang.ToLower();

            string[] parts = dir.Name.Split('-');

            if (parts.Length < 1) return null;

            int sequenceNumber = 0;

            //if (!int.TryParse(parts[0].Trim(), out sequenceNumber)) return null;
            bool startsWithNumber = int.TryParse(parts[0].Trim(), out sequenceNumber);

            int entryNamePos = startsWithNumber ? 1 : 0;
            int modulePos = startsWithNumber ? 2 : 1;

            string indexEntryName = parts[entryNamePos].Trim();
            string module = parts.Length > modulePos ? parts[modulePos].Trim() : null;

            if (string.IsNullOrEmpty(indexEntryName)) return null;

            HelpIndexEntry ret = new HelpIndexEntry()
            {
                Name = resolve_entry_name(lang, dir.FullName),
                Module = module,
                SequenceNumber = sequenceNumber > 0 ? sequenceNumber : 0
            };

            string entryContentPath = dir.FullName + "\\" + indexEntryName + ".rvhlp";
            string subFolderPath = dir.FullName + "\\sub";

            if (File.Exists(entryContentPath)) ret.Content = File.ReadAllText(entryContentPath);

            if (Directory.Exists(subFolderPath))
            {
                DirectoryInfo[] sub = new DirectoryInfo(subFolderPath).GetDirectories();

                for (int i = 0, lnt = sub.Length; i < lnt; ++i)
                {
                    if (_check_entry_folder_name(sub[i]))
                    {
                        HelpIndexEntry entry = create_index_entry(lang, sub[i]);
                        if (entry != null) ret.add_sub(entry);
                    }
                }

                if (ret.Sub != null) ret.set_sub(ret.Sub.OrderBy(u => u.SequenceNumber).ToList());
            }

            Dic[lang][ret.Name.ToLower()] = ret;

            return ret;
        }

        public static List<HelpIndexEntry> help_index(string lang)
        {
            lang = lang.ToLower();

            _init(lang);

            if (Index.ContainsKey(lang)) return Index[lang];
            else return null;
        }

        public static HelpIndexEntry index_entry(string lang, string name)
        {
            lang = lang.ToLower();

            _init(lang);

            if (!string.IsNullOrEmpty(name) && Dic.ContainsKey(lang) &&
                Dic[lang].ContainsKey(name = name.ToLower())) return Dic[lang][name];
            else return null;
        }
    }
}