using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace RaaiVan.Modules.GlobalUtilities
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[RV_" + name + "]"; //'[dbo].' is database owner and 'CN_' is module qualifier
        }

        private static void _parse_applications(ref IDataReader reader, ref List<Application> retItems, ref int totalCount)
        {
            while (reader.Read())
            {
                try
                {
                    Application app = new Application();

                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = int.Parse(reader["TotalCount"].ToString());
                    if (!string.IsNullOrEmpty(reader["ApplicationID"].ToString())) app.ApplicationID = (Guid)reader["ApplicationID"];
                    if (!string.IsNullOrEmpty(reader["ApplicationName"].ToString())) app.Name = (string)reader["ApplicationName"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) app.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) app.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) app.CreatorUserID = (Guid)reader["CreatorUserID"];

                    retItems.Add(app);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_variables(ref IDataReader reader, ref List<Variable> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    Variable var = new Variable();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) var.ID = (long)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) var.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) var.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Value"].ToString())) var.Value = (string)reader["Value"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString()))
                        var.CreatorUserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString()))
                        var.CreationDate = (DateTime)reader["CreationDate"];

                    retItems.Add(var);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_email_queue_items(ref IDataReader reader, ref List<EmailQueueItem> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    EmailQueueItem item = new EmailQueueItem();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) item.ID = (long)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) item.SenderUserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["Email"].ToString())) item.Email = (string)reader["Email"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) item.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["EmailBody"].ToString())) item.EmailBody = (string)reader["EmailBody"];

                    EmailAction ac = EmailAction.None;
                    if (Enum.TryParse<EmailAction>(reader["Action"].ToString(), out ac)) item.Action = ac;

                    retItems.Add(item);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_guids(ref IDataReader reader, ref List<KeyValuePair<string, Guid>> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    string key = string.Empty;
                    Guid value = Guid.Empty;

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) key = (string)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["Guid"].ToString())) value = (Guid)reader["Guid"];

                    if (!string.IsNullOrEmpty(key) && value != Guid.Empty)
                        retItems.Add(new KeyValuePair<string, Guid>(key, value));
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_deleted_states(ref IDataReader reader, ref List<DeletedState> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    DeletedState ds = new DeletedState();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) ds.ID = (long)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["ObjectID"].ToString())) ds.ObjectID = (Guid)reader["ObjectID"];
                    if (!string.IsNullOrEmpty(reader["ObjectType"].ToString())) ds.ObjectType = (string)reader["ObjectType"];
                    if (!string.IsNullOrEmpty(reader["Date"].ToString())) ds.Date = (DateTime)reader["Date"];
                    if (!string.IsNullOrEmpty(reader["Deleted"].ToString())) ds.Deleted = (bool)reader["Deleted"];
                    if (!string.IsNullOrEmpty(reader["Bidirectional"].ToString()))
                        ds.Bidirectional = (bool)reader["Bidirectional"];
                    if (!string.IsNullOrEmpty(reader["HasReverse"].ToString()))
                        ds.HasReverse = (bool)reader["HasReverse"];
                    if (!string.IsNullOrEmpty(reader["RelSourceID"].ToString()))
                        ds.RelSourceID = (Guid)reader["RelSourceID"];
                    if (!string.IsNullOrEmpty(reader["RelDestinationID"].ToString()))
                        ds.RelDestinationID = (Guid)reader["RelDestinationID"];
                    if (!string.IsNullOrEmpty(reader["RelSourceType"].ToString()))
                        ds.RelSourceType = (string)reader["RelSourceType"];
                    if (!string.IsNullOrEmpty(reader["RelDestinationType"].ToString()))
                        ds.RelDestinationType = (string)reader["RelDestinationType"];
                    if (!string.IsNullOrEmpty(reader["RelCreatorID"].ToString()))
                        ds.RelCreatorID = (Guid)reader["RelCreatorID"];

                    retItems.Add(ds);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_tagged_items(ref IDataReader reader, ref List<TaggedItem> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    TaggedItem item = new TaggedItem();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) item.TaggedID = (Guid)reader["ID"];

                    TaggedType tt = TaggedType.None;
                    if (Enum.TryParse<TaggedType>(reader["Type"].ToString(), out tt)) item.TaggedType = tt;
                    else continue;

                    if (tt == TaggedType.Node_Form || tt == TaggedType.Node_Wiki) tt = TaggedType.Node;

                    retItems.Add(item);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_setting_items(ref IDataReader reader, ref Dictionary<RVSettingsItem, string> dic)
        {
            while (reader.Read())
            {
                try
                {
                    RVSettingsItem item;

                    if (!string.IsNullOrEmpty(reader["Value"].ToString()) &&
                        Enum.TryParse<RVSettingsItem>(reader["Name"].ToString(), out item)) dic[item] = (string)reader["Value"];
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_last_active_users(ref IDataReader reader, ref ArrayList retItems)
        {
            while (reader.Read())
            {
                try
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) item["UserID"] = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString()))
                        item["UserName"] = Base64.encode((string)reader["UserName"]);
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString()))
                        item["FirstName"] = Base64.encode((string)reader["FirstName"]);
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString()))
                        item["LastName"] = Base64.encode((string)reader["LastName"]);
                    if (!string.IsNullOrEmpty(reader["Date"].ToString())) item["Date"] = (DateTime)reader["Date"];
                    if (!string.IsNullOrEmpty(reader["Types"].ToString())) item["Types"] = (string)reader["Types"];

                    retItems.Add(item);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_raaivan_statistics(ref IDataReader reader, ref Dictionary<string, object> dic)
        {
            if (reader.Read())
            {
                try
                {
                    List<string> names = new List<string>() {
                        "NodesCount",
                        "QuestionsCount",
                        "AnswersCount",
                        "WikiChangesCount",
                        "PostsCount",
                        "CommentsCount",
                        "ActiveUsersCount",
                        "NodePageVisitsCount",
                        "SearchesCount"
                    };

                    foreach (string n in names)
                        if (!string.IsNullOrEmpty(reader[n].ToString())) dic[n] = (int)reader[n];
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_schema_info(ref IDataReader reader, ref List<SchemaInfo> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    SchemaInfo si = new SchemaInfo();

                    if (!string.IsNullOrEmpty(reader["Table"].ToString())) si.Table = (string)reader["Table"];
                    if (!string.IsNullOrEmpty(reader["Column"].ToString())) si.Column = (string)reader["Column"];
                    if (!string.IsNullOrEmpty(reader["IsPrimaryKey"].ToString())) si.IsPrimaryKey = (bool)reader["IsPrimaryKey"];
                    if (!string.IsNullOrEmpty(reader["IsIdentity"].ToString())) si.IsIdentity = (bool)reader["IsIdentity"];
                    if (!string.IsNullOrEmpty(reader["IsNullable"].ToString())) si.IsNullable = (bool)reader["IsNullable"];
                    if (!string.IsNullOrEmpty(reader["MaxLength"].ToString())) si.MaxLength = (int)reader["MaxLength"];
                    if (!string.IsNullOrEmpty(reader["Order"].ToString())) si.Order = (int)reader["Order"];
                    if (!string.IsNullOrEmpty(reader["DefaultValue"].ToString())) si.DefaultValue = (string)reader["DefaultValue"];

                    MSSQLDataType dt = MSSQLDataType.None;
                    if (!string.IsNullOrEmpty(reader["DataType"].ToString()) &&
                        Enum.TryParse<MSSQLDataType>((string)reader["DataType"], true, out dt)) si.DataType = dt;

                    retItems.Add(si);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_foreign_key(ref IDataReader reader, ref List<ForeignKey> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    ForeignKey fk = new ForeignKey();

                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) fk.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Table"].ToString())) fk.Table = (string)reader["Table"];
                    if (!string.IsNullOrEmpty(reader["Column"].ToString())) fk.Column = (string)reader["Column"];
                    if (!string.IsNullOrEmpty(reader["RefTable"].ToString())) fk.RefTable = (string)reader["RefTable"];
                    if (!string.IsNullOrEmpty(reader["RefColumn"].ToString())) fk.RefColumn = (string)reader["RefColumn"];

                    retItems.Add(fk);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_indexes(ref IDataReader reader, ref List<DBIndex> retItems)
        {
            while (reader.Read())
            {
                try
                {
                    DBIndex ind = new DBIndex();

                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) ind.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Table"].ToString())) ind.Table = (string)reader["Table"];
                    if (!string.IsNullOrEmpty(reader["Column"].ToString())) ind.Column = (string)reader["Column"];
                    if (!string.IsNullOrEmpty(reader["Order"].ToString())) ind.Order = (int)reader["Order"];
                    if (!string.IsNullOrEmpty(reader["IsDescending"].ToString())) ind.IsDescending = (bool)reader["IsDescending"];
                    if (!string.IsNullOrEmpty(reader["IsUnique"].ToString())) ind.IsUnique = (bool)reader["IsUnique"];
                    if (!string.IsNullOrEmpty(reader["IsIncludedColumn"].ToString())) ind.IsIncludedColumn = (bool)reader["IsIncludedColumn"];

                    retItems.Add(ind);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static string GetSystemVersion()
        {
            try
            {
                return ProviderUtil.succeed_string(
                    ProviderUtil.execute_reader(GetFullyQualifiedName("GetSystemVersion")));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return string.Empty; }
        }

        public static bool SetApplications(List<KeyValuePair<Guid, string>> applications)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Items
            DataTable appsTable = new DataTable();
            appsTable.Columns.Add("FirstValue", typeof(Guid));
            appsTable.Columns.Add("SecondValue", typeof(string));

            foreach (KeyValuePair<Guid, string> app in applications)
                appsTable.Rows.Add(app.Key, app.Value);

            SqlParameter appParam = new SqlParameter("@Applications", SqlDbType.Structured);
            appParam.TypeName = "[dbo].[GuidStringTableType]";
            appParam.Value = appsTable;
            //end of Add Items

            cmd.Parameters.Add(appParam);

            string arguments = "@Applications";
            cmd.CommandText = ("EXEC" + " " + GetFullyQualifiedName("SetApplications") + " " + arguments);

            con.Open();

            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
            finally { con.Close(); }
        }

        public static List<Application> GetApplications(List<Guid> applicationIds)
        {
            try
            {
                if (applicationIds == null || applicationIds.Count == 0) return new List<Application>();

                int totalCount = 0;

                List<Application> ret = new List<Application>();
                IDataReader reader = ProviderUtil.execute_reader(GetFullyQualifiedName("GetApplicationsByIDs"),
                    string.Join(",", applicationIds.Select(id => id.ToString())), ',');
                _parse_applications(ref reader, ref ret, ref totalCount);
                return ret;
            }
            catch (Exception ex) { return new List<Application>(); }
        }

        public static List<Application> GetApplications(int? count, int? lowerBoundary, ref int totalCount)
        {
            try
            {
                List<Application> ret = new List<Application>();
                IDataReader reader = ProviderUtil.execute_reader(GetFullyQualifiedName("GetApplications"), count, lowerBoundary);
                _parse_applications(ref reader, ref ret, ref totalCount);
                return ret;
            }
            catch (Exception ex) { return new List<Application>(); }
        }

        public static List<Application> GetUserApplications(Guid userId, bool isCreator, bool? archive)
        {
            try
            {
                List<Application> ret = new List<Application>();
                IDataReader reader = ProviderUtil.execute_reader(GetFullyQualifiedName("GetUserApplications"), userId, isCreator, archive);
                int totalCount = 0;
                _parse_applications(ref reader, ref ret, ref totalCount);
                return ret;
            }
            catch (Exception ex) { return new List<Application>(); }
        }

        public static bool AddOrModifyApplication(Guid applicationId, string name, string title, string description, Guid? currentUserId)
        {
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("AddOrModifyApplication"),
                    applicationId, name, title, description, currentUserId));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
        }

        public static bool RemoveApplication(Guid applicationId)
        {
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("RemoveApplication"), applicationId));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
        }

        public static bool RecycleApplication(Guid applicationId)
        {
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("RecycleApplication"), applicationId));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
        }

        public static bool AddUserToApplication(Guid applicationId, Guid userId)
        {
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("AddUserToApplication"),
                    applicationId, userId));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
        }

        public static bool RemoveUserFromApplication(Guid applicationId, Guid userId)
        {
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("RemoveUserFromApplication"),
                    applicationId, userId));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
        }

        public static bool SetVariable(Guid? applicationId, string name, string value, Guid currentUserId)
        {
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("SetVariable"), applicationId,
                    name, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
        }

        public static string GetVariable(Guid? applicationId, string name)
        {
            try
            {
                return ProviderUtil.succeed_string(
                    ProviderUtil.execute_reader(GetFullyQualifiedName("GetVariable"), applicationId, name));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return string.Empty; }
        }

        public static long? SetOwnerVariable(Guid applicationId, long? id, Guid? ownerId,
            string name, string value, Guid currentUserId)
        {
            try
            {
                long result = ProviderUtil.succeed_long(ProviderUtil.execute_reader(GetFullyQualifiedName("SetOwnerVariable"),
                    applicationId, id, ownerId, name, value, currentUserId, DateTime.Now));

                if (result <= 0) return null;
                else return result;
            }
            catch (Exception ex) { string strEx = ex.ToString(); return null; }
        }

        public static void GetOwnerVariables(ref List<Variable> ret, Guid applicationId,
            long? id, Guid? ownerId, string name, Guid? creatorUserId)
        {
            try
            {
                if (ownerId == Guid.Empty) ownerId = null;
                if (creatorUserId == Guid.Empty) creatorUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(GetFullyQualifiedName("GetOwnerVariables"), applicationId,
                    id, ownerId, name, creatorUserId);
                _parse_variables(ref reader, ref ret);
            }
            catch (Exception ex) { }
        }

        public static bool RemoveOwnerVariable(Guid applicationId, long id, Guid currentUserId)
        {
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("RemoveOwnerVariable"),
                    applicationId, id, currentUserId, DateTime.Now));
            }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
        }

        public static bool AddEmailsToQueue(Guid applicationId, ref List<EmailQueueItem> items)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Items
            DataTable queueTable = new DataTable();
            queueTable.Columns.Add("ID", typeof(long));
            queueTable.Columns.Add("SenderUserID", typeof(Guid));
            queueTable.Columns.Add("Action", typeof(string));
            queueTable.Columns.Add("Email", typeof(string));
            queueTable.Columns.Add("Title", typeof(string));
            queueTable.Columns.Add("EmailBody", typeof(string));

            foreach (EmailQueueItem itm in items)
                queueTable.Rows.Add(itm.ID, itm.SenderUserID, itm.Action, itm.Email, itm.Title, itm.EmailBody);

            SqlParameter queueParam = new SqlParameter("@EmailQueueItems", SqlDbType.Structured);
            queueParam.TypeName = "[dbo].[EmailQueueItemTableType]";
            queueParam.Value = queueTable;
            //end of Add Items

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(queueParam);

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@EmailQueueItems";
            cmd.CommandText = ("EXEC" + " " + GetFullyQualifiedName("AddEmailsToQueue") + " " + arguments);

            con.Open();

            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
            finally { con.Close(); }
        }

        public static void GetEmailQueueItems(Guid applicationId, ref List<EmailQueueItem> retItems, int? count)
        {
            try
            {
                IDataReader reader = ProviderUtil.execute_reader(
                    GetFullyQualifiedName("GetEmailQueueItems"), applicationId, count);
                _parse_email_queue_items(ref reader, ref retItems);
            }
            catch (Exception ex) { string strEx = ex.ToString(); }
        }

        public static bool ArchiveEmailQueueItems(Guid applicationId, ref List<long> itemIds)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add IDs
            DataTable idsTable = new DataTable();
            idsTable.Columns.Add("Value", typeof(long));

            foreach (long id in itemIds) idsTable.Rows.Add(id);

            SqlParameter idsParam = new SqlParameter("@ItemIDs", SqlDbType.Structured);
            idsParam.TypeName = "[dbo].[BigIntTableType]";
            idsParam.Value = idsTable;
            //end of Add IDs

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(idsParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@ItemIDs" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + GetFullyQualifiedName("ArchiveEmailQueueItems") + " " + arguments);

            con.Open();

            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
            finally { con.Close(); }
        }

        public static List<DeletedState> GetDeletedStates(Guid applicationId, int? count, long? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetDeletedStates");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, count, lowerBoundary);
                List<DeletedState> retItems = new List<DeletedState>();
                _parse_deleted_states(ref reader, ref retItems);
                return retItems;
            }
            catch (Exception ex) { return new List<DeletedState>(); }
        }

        public static void GetGuids(Guid applicationId, ref List<KeyValuePair<string, Guid>> retItems,
            ref List<string> ids, string type, bool? exist, bool? createIfNotExist)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add IDs
            DataTable idsTable = new DataTable();
            idsTable.Columns.Add("Value", typeof(string));

            foreach (string id in ids) idsTable.Rows.Add(id);

            SqlParameter idsParam = new SqlParameter("@IDs", SqlDbType.Structured);
            idsParam.TypeName = "[dbo].[StringTableType]";
            idsParam.Value = idsTable;
            //end of Add IDs

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(idsParam);
            cmd.Parameters.AddWithValue("@Type", type);
            if (exist.HasValue) cmd.Parameters.AddWithValue("@Exist", exist);
            if (createIfNotExist.HasValue) cmd.Parameters.AddWithValue("@CreateIfNotExist", createIfNotExist);

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@IDs" + sep + "@Type" +
                (exist.HasValue ? sep + "@Exist" : string.Empty) +
                (createIfNotExist.HasValue ? sep + "@CreateIfNotExist" : string.Empty);
            cmd.CommandText = ("EXEC" + " " + GetFullyQualifiedName("GetGuids") + " " + arguments);

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                _parse_guids(ref reader, ref retItems);
            }
            catch (Exception ex) { string strEx = ex.ToString(); }
            finally { con.Close(); }
        }

        public static bool SaveTaggedItems(Guid applicationId, ref List<TaggedItem> items,
            bool? removeOldTags, Guid currentUserId)
        {
            if (items == null || items.Count == 0 || currentUserId == Guid.Empty) return false;

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Items
            DataTable taggedTable = new DataTable();
            taggedTable.Columns.Add("ContextID", typeof(Guid));
            taggedTable.Columns.Add("TaggedID", typeof(Guid));
            taggedTable.Columns.Add("ContextType", typeof(string));
            taggedTable.Columns.Add("TaggedType", typeof(string));

            foreach (TaggedItem itm in items)
                taggedTable.Rows.Add(itm.ContextID, itm.TaggedID, itm.ContextType, itm.TaggedType);

            SqlParameter taggedParam = new SqlParameter("@TaggedItems", SqlDbType.Structured);
            taggedParam.TypeName = "[dbo].[TaggedItemTableType]";
            taggedParam.Value = taggedTable;
            //end of Add Items

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(taggedParam);
            cmd.Parameters.AddWithValue("@RemoveOldTags", removeOldTags.HasValue && removeOldTags.Value);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@TaggedItems" + sep + "@RemoveOldTags" + sep + "@CurrentUserID";
            cmd.CommandText = ("EXEC" + " " + GetFullyQualifiedName("SaveTaggedItems") + " " + arguments);

            con.Open();

            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
            finally { con.Close(); }
        }

        public static void GetTaggedItems(Guid applicationId, ref List<TaggedItem> retItems,
            Guid contextId, List<TaggedType> taggedTypes)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add TaggedTypes
            DataTable taggedTypesTable = new DataTable();
            taggedTypesTable.Columns.Add("Value", typeof(string));

            foreach (TaggedType tt in taggedTypes)
                if (tt != TaggedType.None) taggedTypesTable.Rows.Add(tt);

            SqlParameter taggedTypesParam = new SqlParameter("@TaggedTypes", SqlDbType.Structured);
            taggedTypesParam.TypeName = "[dbo].[StringTableType]";
            taggedTypesParam.Value = taggedTypesTable;
            //end of Add TaggedTyeps

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@ContextID", contextId);
            cmd.Parameters.Add(taggedTypesParam);

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@ContextID" + sep + "@TaggedTypes";
            cmd.CommandText = ("EXEC" + " " + GetFullyQualifiedName("GetTaggedItems") + " " + arguments);

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                _parse_tagged_items(ref reader, ref retItems);
            }
            catch (Exception ex) { string strEx = ex.ToString(); }
            finally { con.Close(); }
        }

        public static bool AddSystemAdmin(Guid applicationId, Guid userId)
        {
            string spName = GetFullyQualifiedName("AddSystemAdmin");

            try
            {
                return ProviderUtil.succeed((IDataReader)ProviderUtil.execute_reader(spName, applicationId, userId));
            }
            catch (Exception ex) { return false; }
        }

        public static bool IsSystemAdmin(Guid? applicationId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsSystemAdmin");

            try
            {
                return ProviderUtil.succeed((IDataReader)ProviderUtil.execute_reader(spName, applicationId, userId));
            }
            catch (Exception ex) { return false; }
        }

        public static string GetFileExtension(Guid applicationId, Guid fileId)
        {
            string spName = GetFullyQualifiedName("GetFileExtension");

            try
            {
                return ProviderUtil.succeed_string((IDataReader)ProviderUtil.execute_reader(spName, applicationId, fileId));
            }
            catch (Exception ex) { return string.Empty; }
        }

        public static bool LikeDislikeUnlike(Guid applicationId, Guid userId, Guid likedId, bool? like)
        {
            string spName = GetFullyQualifiedName("LikeDislikeUnlike");

            try
            {
                return ProviderUtil.succeed((IDataReader)ProviderUtil.execute_reader(spName, applicationId,
                    userId, likedId, like, DateTime.Now));
            }
            catch (Exception ex) { return false; }
        }

        public static void GetFanIDs(Guid applicationId, ref List<Guid> retList, Guid likedId)
        {
            string spName = GetFullyQualifiedName("GetFanIDs");

            try
            {
                IDataReader reader = (IDataReader)ProviderUtil.execute_reader(spName, applicationId, likedId);
                ProviderUtil.parse_guids(ref reader, ref retList);
            }
            catch (Exception ex) { }
        }

        public static bool FollowUnFollow(Guid applicationId, Guid userId, Guid followedId, bool? follow)
        {
            string spName = GetFullyQualifiedName("FollowUnfollow");

            try
            {
                return ProviderUtil.succeed((IDataReader)ProviderUtil.execute_reader(spName, applicationId,
                    userId, followedId, follow, DateTime.Now));
            }
            catch (Exception ex) { return false; }
        }

        public static bool SetSystemSettings(Guid applicationId, Dictionary<RVSettingsItem, string> items, Guid currentUserId)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Items
            DataTable appsTable = new DataTable();
            appsTable.Columns.Add("FirstValue", typeof(string));
            appsTable.Columns.Add("SecondValue", typeof(string));

            foreach (RVSettingsItem name in items.Keys)
                appsTable.Rows.Add(name.ToString(), items[name]);

            SqlParameter appParam = new SqlParameter("@Items", SqlDbType.Structured);
            appParam.TypeName = "[dbo].[StringPairTableType]";
            appParam.Value = appsTable;
            //end of Add Items

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(appParam);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Items" + sep + "@CurrentUserID" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + GetFullyQualifiedName("SetSystemSettings") + " " + arguments);

            con.Open();

            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex) { string strEx = ex.ToString(); return false; }
            finally { con.Close(); }
        }

        public static Dictionary<RVSettingsItem, string> GetSystemSettings(Guid applicationId, List<RVSettingsItem> names)
        {
            names = names.Where(n => n != RVSettingsItem.UseLocalVariables).ToList();

            if (applicationId == Guid.Empty && names.Count == 0) return new Dictionary<RVSettingsItem, string>();

            string spName = GetFullyQualifiedName("GetSystemSettings");

            try
            {
                IDataReader reader = (IDataReader)ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<RVSettingsItem>(names), ',');
                Dictionary<RVSettingsItem, string> items = new Dictionary<RVSettingsItem, string>();
                _parse_setting_items(ref reader, ref items);
                return items;
            }
            catch (Exception ex) { return new Dictionary<RVSettingsItem, string>(); }
        }

        public static void GetLastContentCreators(Guid applicationId, ref ArrayList retList, int? count)
        {
            string spName = GetFullyQualifiedName("GetLastContentCreators");

            try
            {
                IDataReader reader = (IDataReader)ProviderUtil.execute_reader(spName, applicationId, count);
                _parse_last_active_users(ref reader, ref retList);
            }
            catch (Exception ex) { }
        }

        public static void RaaiVanStatistics(Guid applicationId, ref Dictionary<string, object> dic, DateTime? dateFrom, DateTime? dateTo)
        {
            string spName = GetFullyQualifiedName("RaaiVanStatistics");

            try
            {
                IDataReader reader = (IDataReader)ProviderUtil.execute_reader(spName, applicationId, dateFrom, dateTo);
                _parse_raaivan_statistics(ref reader, ref dic);
            }
            catch (Exception ex) { }
        }

        public static List<SchemaInfo> GetSchemaInfo()
        {
            string spName = GetFullyQualifiedName("SchemaInfo");

            try
            {
                IDataReader reader = (IDataReader)ProviderUtil.execute_reader(spName);
                List<SchemaInfo> items = new List<SchemaInfo>();
                _parse_schema_info(ref reader, ref items);
                return items;
            }
            catch (Exception ex) { return new List<SchemaInfo>(); }
        }

        public static List<ForeignKey> GetForeignKeys()
        {
            string spName = GetFullyQualifiedName("ForeignKeys");

            try
            {
                IDataReader reader = (IDataReader)ProviderUtil.execute_reader(spName);
                List<ForeignKey> items = new List<ForeignKey>();
                _parse_foreign_key(ref reader, ref items);
                return items;
            }
            catch (Exception ex) { return new List<ForeignKey>(); }
        }

        public static List<DBIndex> GetIndexes()
        {
            string spName = GetFullyQualifiedName("Indexes");

            try
            {
                IDataReader reader = (IDataReader)ProviderUtil.execute_reader(spName);
                List<DBIndex> items = new List<DBIndex>();
                _parse_indexes(ref reader, ref items);
                return items;
            }
            catch (Exception ex) { return new List<DBIndex>(); }
        }
    }
}
