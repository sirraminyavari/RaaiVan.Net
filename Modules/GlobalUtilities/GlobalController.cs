using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace RaaiVan.Modules.GlobalUtilities
{
    public class GlobalController
    {
        public static string get_system_version()
        {
            return DataProvider.GetSystemVersion();
        }

        public static bool set_applications(List<KeyValuePair<Guid, string>> applications)
        {
            return DataProvider.SetApplications(applications);
        }

        public static List<Application> get_applications(List<Guid> applicationIds)
        {
            return DataProvider.GetApplications(applicationIds);
        }

        public static List<Application> get_applications(int? count, int? lowerBoundary, ref int totalCount)
        {
            return DataProvider.GetApplications(count, lowerBoundary, ref totalCount);
        }

        public static List<Application> get_applications()
        {
            int totalCount = 0;
            return DataProvider.GetApplications(count: 1000000, lowerBoundary: null, totalCount: ref totalCount);
        }

        public static List<Application> get_user_applications(Guid userId, bool isCreator = false, bool? archive = false)
        {
            return DataProvider.GetUserApplications(userId, isCreator, archive);
        }

        public static Application get_user_application(Guid userId, Guid applicationId)
        {
            return get_user_applications(userId).Where(a => a.ApplicationID == applicationId).FirstOrDefault();
        }

        public static bool add_or_modify_application(Guid applicationId, 
            string name, string title, string description, Guid? currentUserId)
        {
            return DataProvider.AddOrModifyApplication(applicationId, name, title, description, currentUserId);
        }

        public static bool remove_application(Guid applicationId)
        {
            return DataProvider.RemoveApplication(applicationId);
        }

        public static bool recycle_application(Guid applicationId)
        {
            return DataProvider.RecycleApplication(applicationId);
        }

        public static bool add_user_to_application(Guid applicationId, Guid userId)
        {
            return DataProvider.AddUserToApplication(applicationId, userId);
        }

        public static bool remove_user_from_application(Guid applicationId, Guid userId)
        {
            return DataProvider.RemoveUserFromApplication(applicationId, userId);
        }

        public static bool set_variable(Guid? applicationId, string name, string value, Guid currentUserId)
        {
            return DataProvider.SetVariable(applicationId, name, value, currentUserId);
        }

        public static string get_variable(Guid? applicationId, string name)
        {
            return DataProvider.GetVariable(applicationId, name);
        }

        public static long? set_owner_variable(Guid applicationId, long id, string name, string value, Guid currentUserId)
        {
            return DataProvider.SetOwnerVariable(applicationId, id, null, name, value, currentUserId);
        }

        public static long? set_owner_variable(Guid applicationId, Guid ownerId, string name, string value, Guid currentUserId)
        {
            return DataProvider.SetOwnerVariable(applicationId, null, ownerId, name, value, currentUserId);
        }

        public static List<Variable> get_owner_variables(Guid applicationId, Guid ownerId, string name, Guid? creatorUserId)
        {
            List<Variable> lst = new List<GlobalUtilities.Variable>();
            DataProvider.GetOwnerVariables(ref lst, applicationId, null, ownerId, name, creatorUserId);
            return lst;
        }

        public static Variable get_owner_variable(Guid applicationId, long id)
        {
            List<Variable> lst = new List<GlobalUtilities.Variable>();
            DataProvider.GetOwnerVariables(ref lst, applicationId, id, null, null, null);
            return lst.FirstOrDefault();
        }

        public static bool remove_owner_variable(Guid applicationId, long id, Guid currentUserId)
        {
            return DataProvider.RemoveOwnerVariable(applicationId, id, currentUserId);
        }

        public static bool add_emails_to_queue(Guid applicationId, List<EmailQueueItem> items)
        {
            return DataProvider.AddEmailsToQueue(applicationId, ref items);
        }

        public static List<EmailQueueItem> get_email_queue_items(Guid applicationId, int? count = 100)
        {
            List<EmailQueueItem> retList = new List<EmailQueueItem>();
            DataProvider.GetEmailQueueItems(applicationId, ref retList, count);
            return retList;
        }

        public static bool archive_email_queue_items(Guid applicationId, List<long> itemIds)
        {
            if (itemIds.Count == 0) return true;
            return DataProvider.ArchiveEmailQueueItems(applicationId, ref itemIds);
        }

        public static List<KeyValuePair<string, Guid>> get_guids(Guid applicationId,
            List<string> ids, string type, bool? exist, bool? createIfNotExist)
        {
            List<KeyValuePair<string, Guid>> retList = new List<KeyValuePair<string, Guid>>();
            DataProvider.GetGuids(applicationId, ref retList, ref ids, type, exist, createIfNotExist);
            return retList;
        }

        public static List<DeletedState> get_deleted_states(Guid applicationId, int? count, long? lowerBoundary)
        {
            return DataProvider.GetDeletedStates(applicationId, count, lowerBoundary);
        }

        public static bool save_tagged_items(Guid applicationId, List<TaggedItem> items,
            bool? removeOldTags, Guid currentUserId)
        {
            return DataProvider.SaveTaggedItems(applicationId, ref items, removeOldTags, currentUserId);
        }

        protected static void _save_tagged_items(object info)
        {
            SortedList<string, object> obj = (SortedList<string, object>)info;
            save_tagged_items((Guid)obj["ApplicationID"], (List<TaggedItem>)obj["Items"],
                (bool)obj["RemoveOldTags"], (Guid)obj["CurrentUserID"]);
        }

        public static void save_tagged_items_offline(Guid applicationId, List<TaggedItem> items,
            bool? removeOldTags, Guid currentUserId)
        {
            if (items.Count == 0 || currentUserId == Guid.Empty) return;

            SortedList<string, object> obj = new SortedList<string, object>();
            obj["Items"] = items;
            obj["RemoveOldTags"] = removeOldTags.HasValue && removeOldTags.Value;
            obj["CurrentUserID"] = currentUserId;
            obj["ApplicationID"] = applicationId;

            ThreadPool.QueueUserWorkItem(new WaitCallback(_save_tagged_items), obj);
        }

        public static List<TaggedItem> get_tagged_items(Guid applicationId, Guid contextId, List<TaggedType> taggedTypes)
        {
            List<TaggedItem> lst = new List<TaggedItem>();
            DataProvider.GetTaggedItems(applicationId, ref lst, contextId, taggedTypes);
            return lst;
        }

        public static List<TaggedItem> get_tagged_items(Guid applicationId, Guid contextId, TaggedType taggedType)
        {
            return get_tagged_items(applicationId, contextId, new List<TaggedType>() { taggedType });
        }

        public static bool add_system_admin(Guid applicationId, Guid userId)
        {
            return DataProvider.AddSystemAdmin(applicationId, userId);
        }

        public static bool is_system_admin(Guid? applicationId, Guid userId)
        {
            return DataProvider.IsSystemAdmin(applicationId, userId);
        }

        public static string get_file_extension(Guid applicationId, Guid fileId)
        {
            return DataProvider.GetFileExtension(applicationId, fileId);
        }

        public static bool like(Guid applicationId, Guid userId, Guid likedId)
        {
            return DataProvider.LikeDislikeUnlike(applicationId, userId, likedId, true);
        }

        public static bool dislike(Guid applicationId, Guid userId, Guid likedId)
        {
            return DataProvider.LikeDislikeUnlike(applicationId, userId, likedId, false);
        }

        public static bool unlike(Guid applicationId, Guid userId, Guid likedId)
        {
            return DataProvider.LikeDislikeUnlike(applicationId, userId, likedId, null);
        }

        public static List<Guid> get_fan_ids(Guid applicationId, Guid likedId)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetFanIDs(applicationId, ref retList, likedId);
            return retList;
        }

        public static bool follow(Guid applicationId, Guid userId, Guid followedId)
        {
            return DataProvider.FollowUnFollow(applicationId, userId, followedId, true);
        }

        public static bool unfollow(Guid applicationId, Guid userId, Guid followedId)
        {
            return DataProvider.FollowUnFollow(applicationId, userId, followedId, null);
        }

        public static bool set_system_settings(Guid applicationId,
            Dictionary<RVSettingsItem, string> items, Guid currentUserId)
        {
            return DataProvider.SetSystemSettings(applicationId, items, currentUserId);
        }

        public static Dictionary<RVSettingsItem, string> get_system_settings(Guid applicationId, List<RVSettingsItem> names)
        {
            return DataProvider.GetSystemSettings(applicationId, names);
        }

        public static ArrayList get_last_content_creators(Guid applicationId, int? count)
        {
            ArrayList ret = new ArrayList();
            DataProvider.GetLastContentCreators(applicationId, ref ret, count);
            return ret;
        }

        public static Dictionary<string, object> raaivan_statistics(Guid applicationId, DateTime? dateFrom, DateTime? dateTo)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            DataProvider.RaaiVanStatistics(applicationId, ref dic, dateFrom, dateTo);
            return dic;
        }

        public static List<SchemaInfo> get_schema_info()
        {
            return DataProvider.GetSchemaInfo();
        }

        public static List<ForeignKey> get_foreign_keys()
        {
            return DataProvider.GetForeignKeys();
        }

        public static List<DBIndex> get_indexes()
        {
            return DataProvider.GetIndexes();
        }
    }
}
