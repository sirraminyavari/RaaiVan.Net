using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Users
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[USR_" + name + "]"; //'[dbo].' is database owner and 'CN_' is module qualifier
        }

        private static void _parse_users(ref IDataReader reader, ref List<User> lstUsers, 
            ref long totalCount, bool systemAlso = false)
        {
            totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    User user = new User();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) user.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) user.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) user.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) user.LastName = (string)reader["LastName"];

                    if (!systemAlso && user.UserName.ToLower() == "system") continue;

                    if (!string.IsNullOrEmpty(reader["JobTitle"].ToString())) user.JobTitle = (string)reader["JobTitle"];
                    if (!string.IsNullOrEmpty(reader["BirthDay"].ToString())) user.Birthday = (DateTime)reader["BirthDay"];
                    if (!string.IsNullOrEmpty(reader["MainPhoneID"].ToString())) user.MainPhoneID = (Guid)reader["MainPhoneID"];
                    if (!string.IsNullOrEmpty(reader["MainEmailID"].ToString())) user.MainEmailID = (Guid)reader["MainEmailID"];
                    if (!string.IsNullOrEmpty(reader["IsApproved"].ToString())) user.IsApproved = (bool)reader["IsApproved"];
                    if (!string.IsNullOrEmpty(reader["IsLockedOut"].ToString())) user.IsLockedOut = (bool)reader["IsLockedOut"];

                    string strEmploymentType = string.IsNullOrEmpty(reader["EmploymentType"].ToString()) ? string.Empty :
                        reader["EmploymentType"].ToString();
                    try { user.EmploymentType = (EmploymentType)Enum.Parse(typeof(EmploymentType), strEmploymentType); }
                    catch { user.EmploymentType = EmploymentType.NotSet; }

                    lstUsers.Add(user);
                }
                catch { }
            }

            if (reader.NextResult()) totalCount = ProviderUtil.succeed_long(reader);
            else if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_users(ref IDataReader reader, ref List<User> lstUsers, bool systemAlso = false)
        {
            long totalCount = 0;
            _parse_users(ref reader, ref lstUsers, ref totalCount, systemAlso);
        }

        private static List<ApplicationUsers> _parse_application_users(ref IDataReader reader)
        {
            List<ApplicationUsers> lst = new List<ApplicationUsers>();

            while (reader.Read())
            {
                try
                {
                    Guid? applicationId = !string.IsNullOrEmpty(reader["UserID"].ToString()) ? (Guid?)reader["ApplicationID"] : null;

                    if (!applicationId.HasValue) continue;

                    ApplicationUsers item = lst.Where(l => l.ApplicationID == applicationId).FirstOrDefault();

                    if (item == null)
                    {
                        item = new ApplicationUsers() { ApplicationID = applicationId };
                        lst.Add(item);
                    }

                    User user = new User();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) user.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) user.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) user.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) user.LastName = (string)reader["LastName"];

                    item.Count = !string.IsNullOrEmpty(reader["TotalCount"].ToString()) ? 
                        int.Parse(reader["TotalCount"].ToString()) : 0;

                    item.Users.Add(user);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return lst;
        }

        private static void _parse_item_visits_count(ref IDataReader reader, ref List<ItemVisitsCount> lstItemVisits)
        {
            while (reader.Read())
            {
                try
                {
                    ItemVisitsCount visit = new ItemVisitsCount();

                    if (!string.IsNullOrEmpty(reader["ItemID"].ToString())) visit.ItemID = (Guid)reader["ItemID"];
                    if (!string.IsNullOrEmpty(reader["VisitsCount"].ToString())) visit.Count = (int)reader["VisitsCount"];

                    lstItemVisits.Add(visit);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static long _parse_friends(ref IDataReader reader, ref List<Friend> lstFriends)
        {
            long totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    Friend friend = new Friend();

                    if (!string.IsNullOrEmpty(reader["FriendID"].ToString())) friend.User.UserID = (Guid)reader["FriendID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) friend.User.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) friend.User.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) friend.User.LastName = (string)reader["LastName"];
                    if (!string.IsNullOrEmpty(reader["RequestDate"].ToString())) friend.RequestDate = (DateTime)reader["RequestDate"];
                    if (!string.IsNullOrEmpty(reader["AcceptionDate"].ToString())) friend.AcceptionDate = (DateTime)reader["AcceptionDate"];
                    if (!string.IsNullOrEmpty(reader["AreFriends"].ToString())) friend.AreFriends = (bool)reader["AreFriends"];
                    if (!string.IsNullOrEmpty(reader["IsSender"].ToString())) friend.IsSender = (bool)reader["IsSender"];
                    if (!string.IsNullOrEmpty(reader["MutualFriendsCount"].ToString())) 
                        friend.MutualFriendsCount = (int)reader["MutualFriendsCount"];

                    totalCount = !string.IsNullOrEmpty(reader["TotalCount"].ToString()) ? (long)reader["TotalCount"] : 0;

                    lstFriends.Add(friend);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static void _parse_friendship_statuses(ref IDataReader reader, ref List<Friend> lstFriends)
        {
            while (reader.Read())
            {
                try
                {
                    Friend friend = new Friend();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) friend.User.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["IsFriend"].ToString())) friend.AreFriends = (bool)reader["IsFriend"];
                    if (!string.IsNullOrEmpty(reader["IsSender"].ToString())) friend.IsSender = (bool)reader["IsSender"];
                    if (!string.IsNullOrEmpty(reader["MutualFriendsCount"].ToString()))
                        friend.MutualFriendsCount = (int)reader["MutualFriendsCount"];
                    
                    lstFriends.Add(friend);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_phone_numbers(ref IDataReader reader, ref List<PhoneNumber> lstPhoneNumbers, bool mainFlag = false)
        {
            while (reader.Read())
            {
                try
                {
                    PhoneNumber phoneNumber = new PhoneNumber();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) phoneNumber.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["NumberID"].ToString())) phoneNumber.NumberID = (Guid)reader["NumberID"];
                    if (!string.IsNullOrEmpty(reader["PhoneNumber"].ToString())) phoneNumber.Number = (string)reader["PhoneNumber"];
                    if (mainFlag && !string.IsNullOrEmpty(reader["IsMain"].ToString())) phoneNumber.IsMain = (bool)reader["IsMain"];

                    string phoneTypeStr = string.IsNullOrEmpty(reader["PhoneType"].ToString()) ? string.Empty : (string)reader["PhoneType"];

                    try { phoneNumber.PhoneType = (PhoneType)Enum.Parse(typeof(PhoneType), phoneTypeStr); }
                    catch { }

                    lstPhoneNumbers.Add(phoneNumber);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_email_addresses(ref IDataReader reader, ref List<EmailAddress> lstEmailAddress, bool mainFlag = false)
        {
            while (reader.Read())
            {
                try
                {
                    EmailAddress emailAddress = new EmailAddress();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) emailAddress.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["EmailID"].ToString())) emailAddress.EmailID = (Guid)reader["EmailID"];
                    if (!string.IsNullOrEmpty(reader["EmailAddress"].ToString())) emailAddress.Address = (string)reader["EmailAddress"];
                    if (mainFlag && !string.IsNullOrEmpty(reader["IsMain"].ToString())) emailAddress.IsMain = (bool)reader["IsMain"];

                    lstEmailAddress.Add(emailAddress);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_email_contacts_status(ref IDataReader reader, ref List<EmailContactStatus> lstcontacts)
        {
            while (reader.Read())
            {
                try
                {
                    EmailContactStatus contact = new EmailContactStatus();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) contact.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Email"].ToString())) contact.Email = (string)reader["Email"];
                    if (!string.IsNullOrEmpty(reader["FriendRequestReceived"].ToString()))
                        contact.FriendRequestReceived = (bool)reader["FriendRequestReceived"];

                    lstcontacts.Add(contact);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_job_experiences(ref IDataReader reader, ref List<JobExperience> lstJobs)
        {
            while (reader.Read())
            {
                try
                {
                    JobExperience jobExp = new JobExperience();

                    if (!string.IsNullOrEmpty(reader["JobID"].ToString())) jobExp.JobID = (Guid)reader["JobID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) jobExp.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) jobExp.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Employer"].ToString())) jobExp.Employer = (string)reader["Employer"];
                    if (!string.IsNullOrEmpty(reader["StartDate"].ToString())) jobExp.StartDate = (DateTime)reader["StartDate"];
                    if (!string.IsNullOrEmpty(reader["EndDate"].ToString())) jobExp.EndDate = (DateTime)reader["EndDate"];

                    lstJobs.Add(jobExp);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_educational_experiences(ref IDataReader reader, ref List<EducationalExperience> lstEducations)
        {
            while (reader.Read())
            {
                try
                {
                    EducationalExperience educationExp = new EducationalExperience();

                    if (!string.IsNullOrEmpty(reader["EducationID"].ToString())) educationExp.EducationID = (Guid)reader["EducationID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) educationExp.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["School"].ToString())) educationExp.School = (string)reader["School"];
                    if (!string.IsNullOrEmpty(reader["StudyField"].ToString())) educationExp.StudyField = (string)reader["StudyField"];
                    if (!string.IsNullOrEmpty(reader["StartDate"].ToString())) educationExp.StartDate = (DateTime)reader["StartDate"];
                    if (!string.IsNullOrEmpty(reader["EndDate"].ToString())) educationExp.EndDate = (DateTime)reader["EndDate"];
                    if (!string.IsNullOrEmpty(reader["IsSchool"].ToString())) educationExp.IsSchool = (bool)reader["IsSchool"];

                    string DegreeStr = string.IsNullOrEmpty(reader["Level"].ToString()) ? string.Empty : (string)reader["Level"];
                    try { educationExp.Level = (EducationalLevel)Enum.Parse(typeof(EducationalLevel), DegreeStr); }
                    catch { educationExp.Level = EducationalLevel.None; }

                    string FinalLevelStr = string.IsNullOrEmpty(reader["GraduateDegree"].ToString()) ? string.Empty : (string)reader["GraduateDegree"];
                    try { educationExp.GraduateDegree = (GraduateDegree)Enum.Parse(typeof(GraduateDegree), FinalLevelStr); }
                    catch { educationExp.GraduateDegree = GraduateDegree.None; }

                    lstEducations.Add(educationExp);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_honors_and_awards_experiences(ref IDataReader reader, ref List<HonorsAndAwards> retHonors)
        {
            while (reader.Read())
            {
                try
                {
                    HonorsAndAwards honors = new HonorsAndAwards();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) honors.ID = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) honors.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) honors.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Issuer"].ToString())) honors.Issuer = (string)reader["Issuer"];
                    if (!string.IsNullOrEmpty(reader["Occupation"].ToString())) honors.Occupation = (string)reader["Occupation"];
                    if (!string.IsNullOrEmpty(reader["IssueDate"].ToString())) honors.IssueDate = (DateTime)reader["IssueDate"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) honors.Description = (string)reader["Description"];

                    retHonors.Add(honors);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_languages(bool isUserLanguage, ref IDataReader reader, ref List<Language> retLanguages)
        {
            while (reader.Read())
            {
                try
                {
                    Language lang = new Language();

                    if (!string.IsNullOrEmpty(reader["LanguageName"].ToString())) lang.LanguageName = (string)reader["LanguageName"];

                    if (isUserLanguage)
                    {
                        if (!string.IsNullOrEmpty(reader["ID"].ToString())) lang.ID = (Guid)reader["ID"];
                        if (!string.IsNullOrEmpty(reader["UserID"].ToString())) lang.UserID = (Guid)reader["UserID"];

                        string LevelStr = string.IsNullOrEmpty(reader["Level"].ToString()) ? string.Empty : (string)reader["Level"];
                        try { lang.Level = (LanguageLevel)Enum.Parse(typeof(LanguageLevel), LevelStr); }
                        catch { lang.Level = LanguageLevel.None; }
                    }
                    else if (!string.IsNullOrEmpty(reader["LanguageID"].ToString())) lang.LanguageID = (Guid)reader["LanguageID"];
                    retLanguages.Add(lang);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }
        
        private static long _parse_friend_suggestions(ref IDataReader reader, ref List<FriendSuggestion> frndSuggestions)
        {
            long totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    FriendSuggestion fSuggestion = new FriendSuggestion();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) fSuggestion.User.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) fSuggestion.User.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) fSuggestion.User.LastName = (string)reader["LastName"];
                    if (!string.IsNullOrEmpty(reader["MutualFriendsCount"].ToString())) fSuggestion.MutualFriends = (int)reader["MutualFriendsCount"];

                    totalCount = !string.IsNullOrEmpty(reader["TotalCount"].ToString()) ? (long)reader["TotalCount"] : 0;

                    frndSuggestions.Add(fSuggestion);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static long _parse_invitations(ref IDataReader reader, ref List<Invitation> retInvitations)
        {
            long totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    Invitation inv = new Invitation();

                    if (!string.IsNullOrEmpty(reader["ReceiverUserID"].ToString())) inv.ReceiverUser.UserID = (Guid)reader["ReceiverUserID"];
                    if (!string.IsNullOrEmpty(reader["ReceiverFirstName"].ToString())) inv.ReceiverUser.FirstName = (string)reader["ReceiverFirstName"];
                    if (!string.IsNullOrEmpty(reader["ReceiverLastName"].ToString())) inv.ReceiverUser.LastName = (string)reader["ReceiverLastName"];
                    if (!string.IsNullOrEmpty(reader["Email"].ToString())) inv.Email = (string)reader["Email"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) inv.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["Activated"].ToString())) inv.Activated = (bool)reader["Activated"];

                    totalCount = !string.IsNullOrEmpty(reader["TotalCount"].ToString()) ? (long)reader["TotalCount"] : 0;

                    retInvitations.Add(inv);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static void _parse_password(ref IDataReader reader, ref string password, ref string passwordSalt)
        {
            while (reader.Read())
            {
                try
                {
                    if (!string.IsNullOrEmpty(reader["Password"].ToString())) password = (string)reader["Password"];
                    if (!string.IsNullOrEmpty(reader["PasswordSalt"].ToString()))
                        passwordSalt = (string)reader["PasswordSalt"];
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_last_passwords(ref IDataReader reader, ref List<Password> ret)
        {
            while (reader.Read())
            {
                try
                {
                    Password pass = new Password();

                    if (!string.IsNullOrEmpty(reader["Password"].ToString())) pass.Value = (string)reader["Password"];
                    if (!string.IsNullOrEmpty(reader["AutoGenerated"].ToString()))
                        pass.AutoGenerated = (bool)reader["AutoGenerated"];

                    ret.Add(pass);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_user_groups(ref IDataReader reader, ref List<UserGroup> groups)
        {
            while (reader.Read())
            {
                try
                {
                    UserGroup grp = new UserGroup();

                    if (!string.IsNullOrEmpty(reader["GroupID"].ToString())) grp.GroupID = (Guid)reader["GroupID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) grp.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString()))
                        grp.Description = (string)reader["Description"];
                    
                    groups.Add(grp);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_access_roles(ref IDataReader reader, ref List<AccessRole> roles)
        {
            while (reader.Read())
            {
                try
                {
                    AccessRole rl = new AccessRole();

                    if (!string.IsNullOrEmpty(reader["RoleID"].ToString())) rl.RoleID = (Guid)reader["RoleID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) rl.Title = (string)reader["Title"];

                    AccessRoleName name = AccessRoleName.None;
                    if (!Enum.TryParse<AccessRoleName>(reader["Name"].ToString(), out name)) name = AccessRoleName.None;

                    rl.Name = name;

                    roles.Add(rl);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }
        
        private static List<AccessRoleName> _parse_permissions(ref IDataReader reader)
        {
            List<string> strPers = new List<string>();
            ProviderUtil.parse_strings(ref reader, ref strPers);

            List<AccessRoleName> roles = new List<AccessRoleName>();
            foreach (string p in strPers)
            {
                AccessRoleName rn = AccessRoleName.None;
                if (Enum.TryParse<AccessRoleName>(p, out rn) && rn != AccessRoleName.None) roles.Add(rn);
            }

            return roles;
        }

        private static void _parse_advanced_user_search(ref IDataReader reader,
            ref List<AdvancedUserSearch> lst, ref long totalCount)
        {
            while (reader.Read())
            {
                try
                {
                    AdvancedUserSearch m = new AdvancedUserSearch();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) m.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];
                    if (!string.IsNullOrEmpty(reader["Rank"].ToString())) m.Rank = (double)reader["Rank"];
                    if (!string.IsNullOrEmpty(reader["IsMemberCount"].ToString()))
                        m.IsMemberCount = (int)reader["IsMemberCount"];
                    if (!string.IsNullOrEmpty(reader["IsExpertCount"].ToString()))
                        m.IsExpertCount = (int)reader["IsExpertCount"];
                    if (!string.IsNullOrEmpty(reader["IsContributorCount"].ToString()))
                        m.IsContributorCount = (int)reader["IsContributorCount"];
                    if (!string.IsNullOrEmpty(reader["HasPropertyCount"].ToString()))
                        m.HasPropertyCount = (int)reader["HasPropertyCount"];
                    if (!string.IsNullOrEmpty(reader["Resume"].ToString())) m.Resume = (int)reader["Resume"];

                    lst.Add(m);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_advanced_user_search_meta(ref IDataReader reader, ref List<AdvancedUserSearchMeta> lst)
        {
            while (reader.Read())
            {
                try
                {
                    AdvancedUserSearchMeta m = new AdvancedUserSearchMeta();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) m.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["Rank"].ToString())) m.Rank = (double)reader["Rank"];
                    if (!string.IsNullOrEmpty(reader["IsMember"].ToString())) m.IsMember = (bool)reader["IsMember"];
                    if (!string.IsNullOrEmpty(reader["IsExpert"].ToString())) m.IsExpert = (bool)reader["IsExpert"];
                    if (!string.IsNullOrEmpty(reader["IsContributor"].ToString())) m.IsContributor = (bool)reader["IsContributor"];
                    if (!string.IsNullOrEmpty(reader["HasProperty"].ToString())) m.HasProperty = (bool)reader["HasProperty"];

                    lst.Add(m);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static bool _parse_lockout_date(ref IDataReader reader, ref User theUser)
        {
            if (reader.Read())
            {

                try
                {
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) theUser.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["IsLockedOut"].ToString()))
                        theUser.IsLockedOut = (bool)reader["IsLockedOut"];
                    if (!string.IsNullOrEmpty(reader["LastLockoutDate"].ToString()))
                        theUser.LastLockoutDate = (DateTime)reader["LastLockoutDate"];
                    if (!string.IsNullOrEmpty(reader["IsApproved"].ToString()))
                        theUser.IsApproved = (bool)reader["IsApproved"];
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();

            return theUser.IsLockedOut.HasValue && theUser.IsLockedOut.Value;
        }

        private static void _parse_remote_servers(ref IDataReader reader, ref List<RemoteServer> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    RemoteServer item = new RemoteServer();

                    if (!string.IsNullOrEmpty(reader["ServerID"].ToString())) item.ID = (Guid)reader["ServerID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) item.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) item.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["URL"].ToString())) item.URL = (string)reader["URL"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) item.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["Password"].ToString()))
                        item.set_password_encrypted((byte[])reader["Password"]);

                    retItems.Add(item);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }


        public static List<Password> GetLastPasswords(Guid userId, bool? autoGenerated, int? count)
        {
            string spName = GetFullyQualifiedName("GetLastPasswords");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, userId, autoGenerated, count);
                List<Password> lst = new List<Password>();
                _parse_last_passwords(ref reader, ref lst);
                return lst;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return new List<Password>();
            }
        }

        public static DateTime? GetLastPasswordDate(Guid userId)
        {
            string spName = GetFullyQualifiedName("GetLastPasswordDate");

            try
            {
                return ProviderUtil.succeed_datetime(ProviderUtil.execute_reader(spName, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static void GetCurrentPassword(Guid userId, ref string password, ref string passwordSalt)
        {
            string spName = GetFullyQualifiedName("GetCurrentPassword");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, userId);
                _parse_password(ref reader, ref password, ref passwordSalt);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static bool CreateUser(Guid? applicationId, User info, bool passAutoGenerated)
        {
            if (string.IsNullOrEmpty(info.UserName)) return false;

            string spName = GetFullyQualifiedName("CreateUser");
            
            try
            {
                if (!info.UserID.HasValue || info.UserID == Guid.Empty) info.UserID = Guid.NewGuid();

                if (string.IsNullOrEmpty(info.Password))
                {
                    info.Password = info.UserName;
                    while (info.Password.Length < 5) info.Password += info.UserName;

                    passAutoGenerated = true;
                }
                
                info.PasswordSalt = UserUtilities.generate_password_salt();

                string saltedPassword = UserUtilities.encode_password(info.Password, info.PasswordSalt);

                string email = info.Emails == null || info.Emails.Count != 1 || 
                    string.IsNullOrEmpty(info.Emails.First().Address) ? null : info.Emails.First().Address;

                string number = info.PhoneNumbers == null || info.PhoneNumbers.Count != 1 ||
                    string.IsNullOrEmpty(info.PhoneNumbers.First().Number) ? null : info.PhoneNumbers.First().Number;

                bool succeed = ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, info.UserID, 
                    info.UserName, info.FirstName, info.LastName, saltedPassword, info.PasswordSalt,
                    PublicMethods.sha1(info.Password), passAutoGenerated, email, number, DateTime.Now));
                
                return succeed;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool CreateTemporaryUser(Guid? applicationId, User info, bool passwordAutoGenerated,
            string emailSubject, string confirmationEmailBody, DateTime? expirationDate, string activationCode, 
            Guid? invitationId, ref string errorMessage)
        {
            if (invitationId == Guid.Empty) invitationId = null;

            bool result = false;

            if (string.IsNullOrEmpty(info.UserName) || string.IsNullOrEmpty(confirmationEmailBody)) return result;

            string email = info.Emails.Count == 0 ? string.Empty : info.Emails.First().Address;
            string phoneNumber = info.PhoneNumbers.Count == 0 ? string.Empty : info.PhoneNumbers.First().Number;

            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber)) return result;

            if (string.IsNullOrEmpty(info.PasswordSalt) || string.IsNullOrEmpty(info.Password))
            {
                info.PasswordSalt = UserUtilities.generate_password_salt();
                info.Password = UserUtilities.generate_password();
                info.SaltedPassword = UserUtilities.encode_password(info.Password, info.PasswordSalt);

                passwordAutoGenerated = true;
            }

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = con.CreateCommand();

            cmd.Parameters.AddWithValue("@UserID", info.UserID);
            cmd.Parameters.AddWithValue("@UserName", info.UserName);
            cmd.Parameters.AddWithValue("@FirstName", info.FirstName);
            cmd.Parameters.AddWithValue("@LastName", info.LastName);
            cmd.Parameters.AddWithValue("@Password", info.SaltedPassword);
            cmd.Parameters.AddWithValue("@PasswordSalt", info.PasswordSalt);
            cmd.Parameters.AddWithValue("@EncryptedPassword", PublicMethods.sha1(info.Password));
            cmd.Parameters.AddWithValue("@PassAutoGenerated", passwordAutoGenerated);
            if (!string.IsNullOrEmpty(email)) cmd.Parameters.AddWithValue("@Email", email);
            if (!string.IsNullOrEmpty(phoneNumber)) cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);
            if(expirationDate.HasValue) cmd.Parameters.AddWithValue("@ExpirationDate", expirationDate);
            if(!string.IsNullOrEmpty(activationCode)) cmd.Parameters.AddWithValue("@ActivationCode", activationCode);
            if (invitationId.HasValue) cmd.Parameters.AddWithValue("@InvitationID", invitationId);

            string spName = GetFullyQualifiedName("CreateTemporaryUser");

            string sep = ",";
            string arguments = "@UserID" + sep + "@UserName" + sep + "@FirstName" + sep + 
                "@LastName" + sep + "@Password" + sep + "@PasswordSalt" + sep + "@EncryptedPassword" + sep +
                "@PassAutoGenerated" + sep + 
                (string.IsNullOrEmpty(email) ? "null" : "@Email") + sep +
                (string.IsNullOrEmpty(phoneNumber) ? "null" : "@PhoneNumber") + sep +
                "@Now" + sep + 
                (expirationDate.HasValue ? "@ExpirationDate" : "null") + sep + 
                (string.IsNullOrEmpty(activationCode) ? "null" : "@ActivationCode") + sep + 
                (invitationId.HasValue ? "@InvitationID" : "null");
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            SqlTransaction tran = con.BeginTransaction();
            cmd.Transaction = tran;

            try
            {
                result = ProviderUtil.succeed((IDataReader)cmd.ExecuteReader(), ref errorMessage);

                if (!result || !PublicMethods.send_email(applicationId, email, emailSubject, confirmationEmailBody))
                {
                    tran.Rollback();  //if !result the transaction is already rolled back
                    result = false;
                }
                else
                {
                    tran.Commit();
                    result = true;
                }

                tran.Dispose();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
            finally { con.Close(); }

            return result;
        }

        public static bool ActivateTemporaryUser(Guid? applicationId, string activationCode, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("ActivateTemporaryUser");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, activationCode, DateTime.Now), ref errorMessage);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static Guid? GetInvitationID(Guid applicationId, string email, bool checkIfNotUsed)
        {
            string spName = GetFullyQualifiedName("GetInvitationID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, email, checkIfNotUsed));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static bool InviteUser(Guid applicationId, Guid invitationId, string email, 
            bool isExistingInvitation, Guid currentUserId, string emailSubject, string emailBody)
        {
            bool result = false;

            if (string.IsNullOrEmpty(email)) return result;

            if (isExistingInvitation) return PublicMethods.send_email(applicationId, email, emailSubject, emailBody);

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = con.CreateCommand();

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@InvitationID", invitationId);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("InviteUser");

            string sep = ",";
            string arguments = "@ApplicationID" + sep + "@InvitationID" + sep + "@Email" + sep + 
                "@CurrentUserID" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            SqlTransaction tran = con.BeginTransaction();
            cmd.Transaction = tran;

            try
            {
                result = ProviderUtil.succeed((IDataReader)cmd.ExecuteReader());

                if (!result || !PublicMethods.send_email(applicationId, email, emailSubject, emailBody))
                {
                    tran.Rollback();  //if !result the transaction is already rolled back
                    result = false;
                }
                else
                {
                    tran.Commit();
                    result = true;
                }

                tran.Dispose();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
            finally { con.Close(); }

            return result;
        }

        public static int GetInvitedUsersCount(Guid applicationId, Guid? invitorUserId)
        {
            string spName = GetFullyQualifiedName("GetInvitedUsersCount");

            try
            {
                if (invitorUserId == Guid.Empty) invitorUserId = null;

                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId, invitorUserId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return 0;
            }
        }

        public static void GetUserInvitations(Guid applicationId, ref List<Invitation> retInvitation, 
            Guid? senderUserId, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetUserInvitations");

            try
            {
                if (senderUserId == Guid.Empty) senderUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    senderUserId, count, lowerBoundary);
                totalCount = _parse_invitations(ref reader, ref retInvitation);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static List<Application> GetCurrentInvitations(Guid userId, string email)
        {
            string spName = GetFullyQualifiedName("GetCurrentInvitations");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, userId, email);
                List<Guid> appIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref appIds);
                return GlobalController.get_applications(appIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return new List<Application>();
            }
        }

        public static Guid? GetInvitationApplicationID(Guid invitationId, bool checkIfNotUsed)
        {
            string spName = GetFullyQualifiedName("GetInvitationApplicationID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, invitationId, checkIfNotUsed));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static bool SetPassResetTicket(Guid? applicationId, 
            Guid userId, Guid ticket, string email, string emailSubject, string emailBody, bool sendEmail)
        {
            bool result = false;

            if (sendEmail && string.IsNullOrEmpty(email)) return result;

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = con.CreateCommand();

            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Ticket", ticket);

            string spName = GetFullyQualifiedName("SetPassResetTicket");

            string sep = ",";
            string arguments = "@UserID" + sep + "@Ticket";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            SqlTransaction tran = con.BeginTransaction();
            cmd.Transaction = tran;

            try
            {
                result = ProviderUtil.succeed((IDataReader)cmd.ExecuteReader());

                if (sendEmail && (!result || !PublicMethods.send_email(applicationId, email, emailSubject, emailBody)))
                {
                    tran.Rollback();  //if !result the transaction is already rolled back
                    result = false;
                }
                else
                {
                    tran.Commit();
                    result = true;
                }

                tran.Dispose();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
            finally { con.Close(); }

            return result;
        }

        public static Guid? GetPassResetTicket(Guid userId)
        {
            string spName = GetFullyQualifiedName("GetPassResetTicket");

            try
            {
                Guid? ticket = ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, userId));

                if (ticket == Guid.Empty) ticket = null;

                return ticket;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static int GetUsersCount(Guid applicationId, 
            DateTime? creationDateLowerThreshold, DateTime? creationDateUpperThreshold)
        {
            string spName = GetFullyQualifiedName("GetUsersCount");

            try
            {
                return (int)ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId,
                    creationDateLowerThreshold, creationDateUpperThreshold));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return 0;
            }
        }

        public static void GetUserIDs(Guid? applicationId, ref List<Guid> retUserIds, ref List<string> usernames)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add AdditionalIDs
            DataTable usernamesTable = new DataTable();
            usernamesTable.Columns.Add("Value", typeof(string));
            foreach (string _adId in usernames) usernamesTable.Rows.Add(_adId);

            SqlParameter usernamesParam = new SqlParameter("@UserNames", SqlDbType.Structured);
            usernamesParam.TypeName = "[dbo].[StringTableType]";
            usernamesParam.Value = usernamesTable;
            //end of Add AdditionalIDs

            if (applicationId.HasValue) cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(usernamesParam);

            string spName = GetFullyQualifiedName("GetUserIDs");

            string sep = ", ";
            string arguments = (applicationId.HasValue ? "@ApplicationID" : "null") + sep + "@UserNames";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                ProviderUtil.parse_guids(ref reader, ref retUserIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
            finally { con.Close(); }
        }

        public static void GetUsers(Guid applicationId, ref List<User> retUsers, string searchText, 
            long? lowerBoundary, int? count, bool? isOnline, ref long totalCount, bool? isApproved)
        {
            string spName = GetFullyQualifiedName("GetUsers");

            try
            {
                if (lowerBoundary < 0) lowerBoundary = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.get_search_text(searchText), lowerBoundary, count, isOnline, isApproved, DateTime.Now);
                _parse_users(ref reader, ref retUsers, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetUsers(Guid? applicationId, ref List<User> retUsers, List<Guid> userIds)
        {
            string spName = GetFullyQualifiedName("GetUsersByIDs");

            try
            {
                if (userIds == null || userIds.Count == 0) return;

                IDataReader reader = ProviderUtil.execute_reader(spName, ProviderUtil.list_to_string<Guid>(ref userIds), ',');
                _parse_users(ref reader, ref retUsers);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetUsers(Guid applicationId, ref List<User> retUsers, List<string> usernames)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add AdditionalIDs
            DataTable usernamesTable = new DataTable();
            usernamesTable.Columns.Add("Value", typeof(string));
            foreach (string _adId in usernames) usernamesTable.Rows.Add(_adId);

            SqlParameter usernamesParam = new SqlParameter("@UserNames", SqlDbType.Structured);
            usernamesParam.TypeName = "[dbo].[StringTableType]";
            usernamesParam.Value = usernamesTable;
            //end of Add AdditionalIDs

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(usernamesParam);

            string spName = GetFullyQualifiedName("GetUsersByUserName");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@UserNames";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                _parse_users(ref reader, ref retUsers);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
            finally { con.Close(); }
        }

        public static List<ApplicationUsers> GetApplicationUsersPartitioned(List<Guid> applicationIds, int? count)
        {
            string spName = GetFullyQualifiedName("GetApplicationUsersPartitioned");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, string.Join(",", applicationIds), ',', count);
                return _parse_application_users(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return new List<ApplicationUsers>();
            }
        }

        public static void AdvancedUserSearch(Guid applicationId, ref List<AdvancedUserSearch> retList, 
            string searchText, List<Guid> nodeTypeIds, List<Guid> nodeIds, bool? members, bool? experts, 
            bool? contributors, bool? propertyOwners, bool? resume, long? lowerBoundary, int? count, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("AdvancedUserSearch");

            try
            {
                if (lowerBoundary < 0) lowerBoundary = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    searchText, ProviderUtil.get_search_text(searchText, false),
                    ProviderUtil.list_to_string<Guid>(nodeTypeIds), ProviderUtil.list_to_string<Guid>(nodeIds),
                    ',', members, experts, contributors,
                    propertyOwners, resume, lowerBoundary, count);
                
                _parse_advanced_user_search(ref reader, ref retList, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void AdvancedUserSearchMeta(Guid applicationId, ref List<AdvancedUserSearchMeta> retList, 
            Guid userId, string searchText, List<Guid> nodeTypeIds, List<Guid> nodeIds, bool? members, bool? experts, 
            bool? contributors, bool? propertyOwners)
        {
            string spName = GetFullyQualifiedName("AdvancedUserSearchMeta");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId,
                    ProviderUtil.get_search_text(searchText, false),
                    ProviderUtil.list_to_string<Guid>(nodeTypeIds), ProviderUtil.list_to_string<Guid>(nodeIds),
                    ',', members, experts, contributors, propertyOwners);
                _parse_advanced_user_search_meta(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static User GetSystemUser(Guid? applicationId)
        {
            if (RaaiVanSettings.SAASBasedMultiTenancy) applicationId = null;

            string spName = GetFullyQualifiedName("GetSystemUser");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId);
                List<User> users = new List<User>();
                _parse_users(ref reader, ref users, true);
                return users.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static bool CreateAdminUser(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("CreateAdminUser");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static void GetNotExistingUsers(Guid applicationId, 
            ref List<string > retUserNames, ref List<string> userNames)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Users
            DataTable usernamesTable = new DataTable();
            usernamesTable.Columns.Add("Value", typeof(string));

            foreach (string _usr in userNames) usernamesTable.Rows.Add(_usr);

            SqlParameter usernamesParam = new SqlParameter("@UserNames", SqlDbType.Structured);
            usernamesParam.TypeName = "[dbo].[StringTableType]";
            usernamesParam.Value = usernamesTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(usernamesParam);

            string spName = GetFullyQualifiedName("GetNotExistingUsers");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@UserNames";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                ProviderUtil.parse_strings(ref reader, ref retUserNames);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
            finally { con.Close(); }
        }

        public static void GetDepartmentsUserIDs(Guid applicationId, 
            ref List<Guid> retUserIds, ref List<Guid> departmentIds)
        {
            string spName = GetFullyQualifiedName("GetDepartmentsUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref departmentIds), ',');
                ProviderUtil.parse_guids(ref reader, ref retUserIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetDepartmentGroupsUserIDs(Guid applicationId, 
            ref List<Guid> retUserIds, ref List<Guid> departmentGroupIds)
        {
            string spName = GetFullyQualifiedName("GetDepartmentGroupsUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref departmentGroupIds), ',');
                ProviderUtil.parse_guids(ref reader, ref retUserIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static bool RegisterItemVisit(Guid applicationId, 
            Guid itemId, Guid userId, DateTime visitDate, VisitItemTypes itemType)
        {
            string spName = GetFullyQualifiedName("RegisterItemVisit");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    itemId, userId, visitDate, itemType.ToString()));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static void GetItemsVisitsCount(Guid applicationId, ref List<ItemVisitsCount> retCounts, 
            ref List<Guid> itemIds, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            string spName = GetFullyQualifiedName("GetItemsVisitsCount");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref itemIds), lowerDateLimit, upperDateLimit);
                _parse_item_visits_count(ref reader, ref retCounts);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetMostVisitedItems(Guid applicationId, ref List<ItemVisitsCount> retCounts, 
            VisitItemTypes itemType, int? count, DateTime? lowerDateLimit, DateTime? upperDateLimit, 
            Guid? nodeTypeId, int? knowledgeTypeId)
        {
            string spName = GetFullyQualifiedName("GetMostVisitedItems");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    itemType.ToString(), count, lowerDateLimit, upperDateLimit, nodeTypeId, knowledgeTypeId);
                _parse_item_visits_count(ref reader, ref retCounts);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static bool SendFriendshipRequest(Guid applicationId, Guid userId, Guid receiverUserId)
        {
            string spName = GetFullyQualifiedName("SendFriendshipRequest");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, receiverUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool AcceptFriendship(Guid applicationId, Guid userId, Guid senderUserId)
        {
            string spName = GetFullyQualifiedName("AcceptFriendship");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    userId, senderUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RejectFriendship(Guid applicationId, Guid userId, Guid friendUserId)
        {
            string spName = GetFullyQualifiedName("RejectFriendship");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId, friendUserId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static void GetFriendshipStatus(Guid applicationId, 
            ref List<Friend> retUsers, Guid userId, List<Guid> otherUserIds, bool? mutualsCount)
        {
            string spName = GetFullyQualifiedName("GetFriendshipStatus");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, ProviderUtil.list_to_string<Guid>(otherUserIds), ',', mutualsCount);
                _parse_friendship_statuses(ref reader, ref retUsers);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetFriendIDs(Guid applicationId, 
            ref List<Guid> retIds, Guid userId, bool? areFriends, bool? sent, bool? received)
        {
            string spName = GetFullyQualifiedName("GetFriendIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    userId, areFriends, sent, received);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetFriends(Guid applicationId, ref List<Friend> friends, Guid userId, 
            List<Guid> friendIds, bool? mutualsCount, bool? areFriends, bool? isSender, string searchText, 
            int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetFriends");

            try
            {
                searchText = ProviderUtil.get_search_text(searchText);

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, ProviderUtil.list_to_string<Guid>(friendIds), ',', mutualsCount,
                    areFriends, isSender, searchText, count, lowerBoundary);
                totalCount = _parse_friends(ref reader, ref friends);
                if (totalCount < 0) totalCount = 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static bool UpdateFriendSuggestions(Guid applicationId, Guid? userId)
        {
            string spName = GetFullyQualifiedName("UpdateFriendSuggestions");

            try
            {
                if (userId == Guid.Empty) userId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }
        
        public static void GetFriendSuggestions(Guid applicationId, ref List<FriendSuggestion> frndSuggestions, 
            Guid userId, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetFriendSuggestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, count, lowerBoundary);
                totalCount = _parse_friend_suggestions(ref reader, ref frndSuggestions);
                if (totalCount < 0) totalCount = 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }
        
        public static int GetFriendsCount(Guid applicationId, 
            Guid userId, bool? areFriends, bool? sent, bool? received)
        {
            string spName = GetFullyQualifiedName("GetFriendsCount");

            try
            {
                return (int)ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId,
                    userId, areFriends, sent, received));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return 0;
            }
        }

        public static void GetEmailContactsStatus(Guid applicationId, ref List<EmailContactStatus> retContacts, 
            Guid userId, List<string> emails, bool? saveEmails)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add AdditionalIDs
            DataTable emailsTable = new DataTable();
            emailsTable.Columns.Add("Value", typeof(string));
            foreach (string _adId in emails) emailsTable.Rows.Add(_adId);

            SqlParameter emailsParam = new SqlParameter("@Emails", SqlDbType.Structured);
            emailsParam.TypeName = "[dbo].[StringTableType]";
            emailsParam.Value = emailsTable;
            //end of Add AdditionalIDs

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.Add(emailsParam);
            if(saveEmails.HasValue) cmd.Parameters.AddWithValue("@SaveEmails", saveEmails);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("GetEmailContactsStatus");

            string sep = ", " ;
            string arguments = "@ApplicationID" + sep + "@UserID" + sep + "@Emails" + sep + 
                (saveEmails.HasValue ? "@SaveEmails" : "null") + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                _parse_email_contacts_status(ref reader, ref retContacts);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
            finally { con.Close(); }
        }

        public static bool SetTheme(Guid? applicationId, Guid userId, string theme)
        {
            string spName = GetFullyQualifiedName("SetTheme");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, userId, theme));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static string GetTheme(Guid? applicationId, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetTheme");

            try
            {
                return ProviderUtil.succeed_string(ProviderUtil.execute_reader(spName, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return string.Empty;
            }
        }

        public static void GetApprovedUserIDs(Guid applicationId, ref List<Guid> retIds, ref List<Guid> userIds)
        {
            string spName = GetFullyQualifiedName("GetApprovedUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string(ref userIds), ',');
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static bool SetLastActivityDate(Guid userId)
        {
            string spName = GetFullyQualifiedName("SetLastActivityDate");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, userId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool AddOrModifyRemoteServer(Guid applicationId, Guid serverId, Guid userId,
            string name, string url, string username, byte[] password)
        {
            string spName = GetFullyQualifiedName("AddOrModifyRemoteServer");

            try
            {
                return ProviderUtil.succeed((IDataReader)ProviderUtil.execute_reader(spName, applicationId,
                    serverId, userId, name, url, username, password, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemoveRemoteServer(Guid applicationId, Guid serverId)
        {
            string spName = GetFullyQualifiedName("RemoveRemoteServer");

            try
            {
                return ProviderUtil.succeed((IDataReader)ProviderUtil.execute_reader(spName, applicationId, serverId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static List<RemoteServer> GetRemoteServers(Guid applicationId, Guid? serverId)
        {
            string spName = GetFullyQualifiedName("GetRemoteServers");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, serverId);
                List<RemoteServer> retItems = new List<RemoteServer>();
                _parse_remote_servers(ref reader, ref retItems);
                return retItems;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return new List<RemoteServer>();
            }
        }

        /* Profile */

        public static bool SetFirstAndLastName(Guid? applicationId, Guid userId, string firstName, string lastName)
        {
            string spName = GetFullyQualifiedName("SetFirstAndLastName");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, userId, firstName, lastName));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetUserName(Guid applicationId, Guid userId, string userName)
        {
            string spName = GetFullyQualifiedName("SetUserName");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId, userName));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetPassword(Guid userId, string password, bool autoGenerated)
        {
            string spName = GetFullyQualifiedName("SetPassword");

            try
            {
                Password pass = new Password(password);
                
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, userId,
                    pass.Salted, pass.Salt, pass.Encrypted, autoGenerated, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool Locked(Guid userId, bool? locked, ref User theUser)
        {
            string spName = GetFullyQualifiedName("Locked");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, userId, locked, DateTime.Now);

                if (locked.HasValue) return ProviderUtil.succeed(reader);
                else return _parse_lockout_date(ref reader, ref theUser);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static int LoginAttempt(Guid userId, bool succeed)
        {
            string spName = GetFullyQualifiedName("LoginAttempt");

            try
            {
                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, userId, succeed));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return 0;
            }
        }

        public static bool IsApproved(Guid applicationId, Guid userId, bool? isApproved)
        {
            string spName = GetFullyQualifiedName("IsApproved");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId, isApproved));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetJobTitle(Guid applicationId, Guid userId, string jobTitle)
        {
            string spName = GetFullyQualifiedName("SetJobTitle");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId, jobTitle));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetEmploymentType(Guid applicationId, Guid userId, EmploymentType employmentType)
        {
            string spName = GetFullyQualifiedName("SetEmploymentType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, employmentType.ToString()));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetBirthday(Guid applicationId, Guid userId, DateTime birthday)
        {
            string spName = GetFullyQualifiedName("SetBirthday");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId, birthday));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetPhoneNumber(Guid numberId, Guid userId, string phoneNumber, PhoneType phoneNumberType, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetPhoneNumber");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName,
                    numberId, userId, phoneNumber, phoneNumberType.ToString(), currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool EditPhoneNumber(Guid numberID, string phoneNumber, PhoneType phoneType, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("EditPhoneNumber");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, 
                    numberID, phoneNumber, phoneType.ToString(), currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemovePhoneNumber(Guid numberId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemovePhoneNumber");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, numberId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetMainPhone(Guid numberId, Guid userId)
        {
            string spName = GetFullyQualifiedName("SetMainPhone");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, numberId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetEmailAddress(Guid emailId, Guid userId, string address, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetEmailAddress");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, emailId, userId, address, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool EditEmailAddress(Guid emailId, string address, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("EditEmailAddress");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, emailId, address, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemoveEmailAddress(Guid emailId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveEmailAddress");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, emailId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetMainEmail(Guid emailId, Guid userId)
        {
            string spName = GetFullyQualifiedName("SetMainEmail");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, emailId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static void GetPhoneNumbers(ref List<PhoneNumber> retPhoneNumber, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetPhoneNumbers");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, userId);
                _parse_phone_numbers(ref reader, ref retPhoneNumber);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static PhoneNumber GetPhoneNumber(Guid numberId)
        {
            string spName = GetFullyQualifiedName("GetPhoneNumber");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, numberId);
                List<PhoneNumber> numbers = new List<PhoneNumber>();
                _parse_phone_numbers(ref reader, ref numbers);
                return numbers.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static Guid? GetMainPhone(Guid userId)
        {
            string spName = GetFullyQualifiedName("GetMainPhone");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static void GetEmailAddresses(ref List<EmailAddress> retEmailAddress, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetEmailAddresses");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, userId);
                _parse_email_addresses(ref reader, ref retEmailAddress);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static EmailAddress GetEmailAddress(Guid emailId)
        {
            string spName = GetFullyQualifiedName("GetEmailAddress");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, emailId);
                List<EmailAddress> addresses = new List<EmailAddress>();
                _parse_email_addresses(ref reader, ref addresses);
                return addresses.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static void GetNotExistingEmails(Guid applicationId, 
            ref List<string> retEmails, ref List<string> emails)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Users
            DataTable emailsTable = new DataTable();
            emailsTable.Columns.Add("Value", typeof(string));

            foreach (string _usr in emails) emailsTable.Rows.Add(_usr);

            SqlParameter emailsParam = new SqlParameter("@Emails", SqlDbType.Structured);
            emailsParam.TypeName = "[dbo].[StringTableType]";
            emailsParam.Value = emailsTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(emailsParam);

            string spName = GetFullyQualifiedName("GetNotExistingEmails");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Emails";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                ProviderUtil.parse_strings(ref reader, ref retEmails);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
            finally { con.Close(); }
        }

        public static void GetEmailOwners(Guid? applicationId, ref List<EmailAddress> retEmailAddresses, List<string> emails)
        {
            string spName = GetFullyQualifiedName("GetEmailOwners");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, string.Join(",", emails), ',');
                _parse_email_addresses(ref reader, ref retEmailAddresses, mainFlag: true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetPhoneOwners(Guid? applicationId, ref List<PhoneNumber> retNumbers, List<string> numbers)
        {
            string spName = GetFullyQualifiedName("GetPhoneOwners");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, string.Join(",", numbers), ',');
                _parse_phone_numbers(ref reader, ref retNumbers, mainFlag: true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static Guid? GetMainEmail(Guid userId)
        {
            string spName = GetFullyQualifiedName("GetMainEmail");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static void GetUsersMainPhone(ref List<PhoneNumber> retUsersMainPhone, List<Guid> userIds)
        {
            string spName = GetFullyQualifiedName("GetUsersMainPhone");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, ProviderUtil.list_to_string<Guid>(userIds), ',');
                _parse_phone_numbers(ref reader, ref retUsersMainPhone);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetUsersMainEmail(ref List<EmailAddress> retUsersMainEmail, List<Guid> userIds)
        {
            string spName = GetFullyQualifiedName("GetUsersMainEmail");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName,
                    ProviderUtil.list_to_string<Guid>(userIds), ',');
                _parse_email_addresses(ref reader, ref retUsersMainEmail);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(null, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetJobExperiences(Guid applicationId, Guid userId, ref List<JobExperience> retJobs)
        {
            string spName = GetFullyQualifiedName("GetJobExperiences");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId);
                _parse_job_experiences(ref reader, ref retJobs);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static JobExperience GetJobExperience(Guid applicationId, Guid id)
        {
            string spName = GetFullyQualifiedName("GetJobExperience");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, id);
                List<JobExperience> items = new List<JobExperience>();
                _parse_job_experiences(ref reader, ref items);
                return items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static void GetEducationalExperiences(Guid applicationId, 
            Guid userId, ref List<EducationalExperience> retEducations)
        {
            string spName = GetFullyQualifiedName("GetEducationalExperiences");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId);
                _parse_educational_experiences(ref reader, ref retEducations);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static EducationalExperience GetEducationalExperience(Guid applicationId, Guid id)
        {
            string spName = GetFullyQualifiedName("GetEducationalExperience");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, id);
                List<EducationalExperience> items = new List<EducationalExperience>();
                _parse_educational_experiences(ref reader, ref items);
                return items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static void GetHonorsAndAwards(Guid applicationId, Guid userId, ref List<HonorsAndAwards> retHonors)
        {
            string spName = GetFullyQualifiedName("GetHonorsAndAwards");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId);
                _parse_honors_and_awards_experiences(ref reader, ref retHonors);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static HonorsAndAwards GetHonorOrAward(Guid applicationId, Guid id)
        {
            string spName = GetFullyQualifiedName("GetHonorOrAward");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, id);
                List<HonorsAndAwards> items = new List<HonorsAndAwards>();
                _parse_honors_and_awards_experiences(ref reader, ref items);
                return items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static void GetUserLanguages(Guid applicationId, Guid userId, ref List<Language> retLanguages)
        {
            string spName = GetFullyQualifiedName("GetUserLanguages");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId);
                _parse_languages(true, ref reader, ref retLanguages);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static Language GetUserLanguage(Guid applicationId, Guid id)
        {
            string spName = GetFullyQualifiedName("GetUserLanguage");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, id);
                List<Language> items = new List<Language>();
                _parse_languages(true, ref reader, ref items);
                return items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return null;
            }
        }

        public static void GetLanguages(Guid applicationId, 
            ref List<Language> lst, List<Guid> langIDs = null, char? delimiter = null)
        {
            string spName = GetFullyQualifiedName("GetLanguages");

            try
            {
                string s = (langIDs == null ? null : ProviderUtil.list_to_string(langIDs, delimiter));

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, s, delimiter);
                _parse_languages(false, ref reader, ref lst);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static bool SetJobExperience(Guid applicationId, Guid jobId, Guid userId, Guid currentUserId, 
            string title, string employer, DateTime? startDate, DateTime? endDate)
        {
            string spName = GetFullyQualifiedName("SetJobExperience");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    jobId, userId, currentUserId, DateTime.Now, title, employer, startDate, endDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetEducationalExperience(Guid applicationId, Guid educationId, Guid userId, 
            Guid currentUserId, string school, string studyField, EducationalLevel? degree, 
            GraduateDegree? level, DateTime? startDate, DateTime? endDate, bool isSchool)
        {
            string spName = GetFullyQualifiedName("SetEducationalExperience");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, educationId, 
                    userId, currentUserId, DateTime.Now, school, studyField, degree.ToString(), 
                    level.ToString(), startDate, endDate, isSchool));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetHonorAndAward(Guid applicationId, Guid honorId, Guid userId, Guid currentUserId, 
            string title, string occupation, string issuer, DateTime? issueDate, string description)
        {
            string spName = GetFullyQualifiedName("SetHonorAndAward");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    honorId, userId, currentUserId, DateTime.Now, title, occupation, issuer, issueDate, description));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetLanguage(Guid applicationId, Guid Id, string languageName, Guid userId, 
            Guid currentUserId, LanguageLevel langLevel)
        {
            string spName = GetFullyQualifiedName("SetLanguage");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Id, languageName, userId, currentUserId, DateTime.Now, langLevel.ToString()));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemoveJobExperience(Guid applicationId, Guid jobId)
        {
            string spName = GetFullyQualifiedName("RemoveJobExperience");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, jobId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemoveEducationalExperience(Guid applicationId, Guid educationId)
        {
            string spName = GetFullyQualifiedName("RemoveEducationalExperience");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, educationId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemoveHonorAndAward(Guid applicationId, Guid honorId)
        {
            string spName = GetFullyQualifiedName("RemoveHonorAndAward");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, honorId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemoveLanguage(Guid applicationId, Guid languageId)
        {
            string spName = GetFullyQualifiedName("RemoveLanguage");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, languageId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        /* end of Profile */

        /* User Groups */

        public static bool CreateUserGroup(Guid applicationId, Guid groupId, 
            string title, string description, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("CreateUserGroup");

            title = title.Trim();
            description = description.Trim();

            if (string.IsNullOrEmpty(title)) return false;

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    groupId, title, description, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool ModifyUserGroup(Guid applicationId, Guid groupId,
            string title, string description, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ModifyUserGroup");

            title = title.Trim();
            description = description.Trim();

            if (string.IsNullOrEmpty(title)) return false;

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    groupId, title, description, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemoveUserGroup(Guid applicationId, Guid groupId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveUserGroup");
            
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    groupId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }
        
        public static bool AddUserGroupMembers(Guid applicationId, Guid groupId, List<Guid> userIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddUserGroupMembers");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    groupId, ProviderUtil.list_to_string<Guid>(userIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool RemoveUserGroupMembers(Guid applicationId, Guid groupId, List<Guid> userIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveUserGroupMembers");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    groupId, ProviderUtil.list_to_string<Guid>(userIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool SetUserGroupPermission(Guid applicationId, Guid groupId, Guid roleId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetUserGroupPermission");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    groupId, roleId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static bool UnsetUserGroupPermission(Guid applicationId, Guid groupId, Guid roleId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("UnsetUserGroupPermission");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    groupId, roleId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return false;
            }
        }

        public static void GetUserGroups(Guid applicationId, ref List<UserGroup> retGroups, List<Guid> groupIds)
        {
            string spName = GetFullyQualifiedName("GetUserGroups");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(groupIds), ',');
                _parse_user_groups(ref reader, ref retGroups);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetUserGroupMembers(Guid applicationId, ref List<User> retUsers, Guid groupId)
        {
            string spName = GetFullyQualifiedName("GetUserGroupMembers");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, groupId);
                _parse_users(ref reader, ref retUsers);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static void GetUserGroupAccessRoles(Guid applicationId, ref List<AccessRole> retRoles, Guid groupId)
        {
            string spName = GetFullyQualifiedName("GetUserGroupAccessRoles");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, groupId);
                _parse_access_roles(ref reader, ref retRoles);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        public static List<AccessRoleName> CheckUserGroupPermissions(Guid applicationId, 
            Guid userId, List<AccessRoleName> permissions)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            
            //Add Permissions
            DataTable permissionsTable = new DataTable();
            permissionsTable.Columns.Add("Value", typeof(string));

            foreach (AccessRoleName nm in permissions)
                if (nm != AccessRoleName.None) permissionsTable.Rows.Add(nm.ToString());

            SqlParameter permissionsParam = new SqlParameter("@Permissions", SqlDbType.Structured);
            permissionsParam.TypeName = "[dbo].[StringTableType]";
            permissionsParam.Value = permissionsTable;
            //end of Add Permissions
            
            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.Add(permissionsParam);

            string spName = GetFullyQualifiedName("CheckUserGroupPermissions");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@UserID" + sep + "@Permissions";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);
            
            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                return _parse_permissions(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
                return new List<AccessRoleName>();
            }
            finally { con.Close(); }
        }

        public static void GetAccessRoles(Guid applicationId, ref List<AccessRole> roles)
        {
            string spName = GetFullyQualifiedName("GetAccessRoles");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId);
                _parse_access_roles(ref reader, ref roles);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.USR);
            }
        }

        /* end of User Groups */
    }
}
