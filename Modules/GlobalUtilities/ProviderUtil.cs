using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.ApplicationBlocks.Data;

namespace RaaiVan.Modules.GlobalUtilities
{
    public static class ProviderUtil
    {
        private static string _connectionString;
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    string name = "EKMConnectionString";
                    string env = RaaiVanSettings.UseLocalVariables ? string.Empty : PublicMethods.get_environment_variable("rv_" + name);
                    _connectionString = !string.IsNullOrEmpty(env) ? env : 
                        System.Configuration.ConfigurationManager.ConnectionStrings[name].ConnectionString;
                }
                return _connectionString;
            }
        }


        public static string list_to_string<T>(ref List<T> lst, char? delimiter = ',')
        {
            string strGuids = string.Empty;
            bool isFirst = true;
            foreach (T _item in lst)
            {
                strGuids += (isFirst || !delimiter.HasValue ? string.Empty : delimiter.ToString()) + _item.ToString();
                isFirst = false;
            }

            return strGuids;
        }

        public static string list_to_string<T>(List<T> lst, char? delimiter = ',')
        {
            return list_to_string<T>(ref lst, delimiter);
        }

        public static string pair_list_to_string<F, S>(ref List<KeyValuePair<F, S>> lst, char innerDelimiter, char outerDelimiter)
        {
            string strGuids = string.Empty;
            bool isFirst = true;
            foreach (KeyValuePair<F, S> _item in lst)
            {
                strGuids += (isFirst ? string.Empty : outerDelimiter.ToString()) +
                    (_item.Key.ToString() + innerDelimiter.ToString() + _item.Value.ToString());
                isFirst = false;
            }

            return strGuids;
        }

        public static string triple_list_to_string<F, S, T>(ref List<KeyValuePair<KeyValuePair<F, S>, T>> lst,
            char innerDelimiter, char outerDelimiter)
        {
            string strGuids = string.Empty;
            bool isFirst = true;
            foreach (KeyValuePair<KeyValuePair<F, S>, T> _item in lst)
            {
                strGuids += (isFirst ? string.Empty : outerDelimiter.ToString()) +
                    (_item.Key.Key.ToString() + innerDelimiter.ToString() + _item.Key.Value.ToString() + innerDelimiter.ToString() +
                    _item.Value.ToString());
                isFirst = false;
            }

            return strGuids;
        }

        public static string get_search_text(string searchText, bool startWith = true)
        {
            if (string.IsNullOrEmpty(searchText)) return searchText;

            return "ISABOUT(" + string.Join(",", 
                PublicMethods.convert_numbers_from_local(searchText.Replace("\"", " ").Replace("'", " "))
                .Split(' ').Select(u => u.Trim()).Where(x => !string.IsNullOrEmpty(x))
                .Select(v => "\"" + v + (startWith ? "*" : "") + "\"")) + ")";

            /*
            searchText = PublicMethods.convert_numbers_from_persian(searchText.Replace("\"", " ").Replace("'", " ").Replace("(", " ").Replace(")", " "));

            string[] words = searchText.Split(' ');
            List<string> lstWords = new List<string>();

            for (int i = 0; i < words.Count(); ++i)
                if (!string.IsNullOrEmpty(words[i].Trim())) lstWords.Add(words[i].Trim());

            return "ISABOUT(" + string.Join(",", lstWords.Select(u => "\"" + u + (startWith ? "*" : "") + "\"")) + ")";

            searchText = "ISABOUT(";
            for (int i = 0, _count = lstWords.Count; i < _count; ++i)
            {
                searchText += (i == 0 ? string.Empty : ",") + "\"" + lstWords[i] + (startWith ? "*" : "") +
                    "\" WEIGHT(" + (i > 4 ? 0.1 : (i == 0 ? 0.99 : 1.0) - (i * 0.2)).ToString() + ")";
            }
            searchText += ")";
            
            return searchText;
            */
        }

        public static string get_tags_text(List<string> tags)
        {
            if (tags == null) return string.Empty;

            string strTags = string.Empty;

            bool isFirst = true;
            foreach (string _t in tags)
            {
                strTags += (isFirst ? string.Empty : " ~ ") + _t;
                isFirst = false;
            }

            return strTags;
        }

        public static List<string> get_tags_list(string strTags, char delimiter = '~')
        {
            if (string.IsNullOrEmpty(strTags)) return new List<string>();
            List<string> tags = strTags.Split(delimiter).ToList();

            List<string> retList = new List<string>();

            foreach (string _t in tags)
            {
                if (string.IsNullOrEmpty(_t.Trim())) continue;
                retList.Add(_t.Trim());
            }

            return retList;
        }

        public static bool succeed(IDataReader reader, ref string errorMessage, ref List<Dashboard> retDashboards)
        {
            try
            {
                reader.Read();

                try { return (bool)reader[0]; }
                catch { }

                try { if (reader.FieldCount > 1) errorMessage = reader[1].ToString(); }
                catch { }

                bool result = long.Parse(reader[0].ToString()) > 0 ? true : false;

                if (result && reader.NextResult())
                {
                    long totalCount = 0;
                    parse_dashboards(ref reader, ref retDashboards, ref totalCount);
                }

                return result;
            }
            catch { return false; }
            finally { if (!reader.IsClosed) reader.Close(); }
        }

        public static bool succeed(IDataReader reader, ref string errorMessage)
        {
            List<Dashboard> d = new List<Dashboard>();
            return succeed(reader, ref errorMessage, ref d);
        }

        public static bool succeed(IDataReader reader, ref List<Dashboard> retDashboards)
        {
            string msg = string.Empty;
            return succeed(reader, ref msg, ref retDashboards);
        }

        public static bool succeed(IDataReader reader)
        {
            string msg = string.Empty;
            return succeed(reader, ref msg);
        }

        public static int succeed_int(IDataReader reader, ref string errorMessage)
        {
            try
            {
                reader.Read();

                try { if (reader.FieldCount > 1) errorMessage = reader[1].ToString(); }
                catch { }

                return int.Parse(reader[0].ToString());
            }
            catch { return 0; }
            finally { if (!reader.IsClosed) reader.Close(); }
        }

        public static int succeed_int(IDataReader reader)
        {
            string msg = string.Empty;
            return succeed_int(reader, ref msg);
        }

        public static long succeed_long(IDataReader reader, ref string errorMessage, bool dontClose = false)
        {
            try
            {
                reader.Read();

                try { if (reader.FieldCount > 1) errorMessage = reader[1].ToString(); }
                catch { }

                return long.Parse(reader[0].ToString());
            }
            catch { return 0; }
            finally { if (!dontClose && !reader.IsClosed) reader.Close(); }
        }

        public static long succeed_long(IDataReader reader, bool dontClose = false)
        {
            string msg = string.Empty;
            return succeed_long(reader, ref msg, dontClose);
        }

        public static string succeed_string(IDataReader reader, ref string errorMessage)
        {
            try
            {
                reader.Read();

                try { if (reader.FieldCount > 1) errorMessage = reader[1].ToString(); }
                catch { }

                return reader[0].ToString();
            }
            catch { return null; }
            finally { if (!reader.IsClosed) reader.Close(); }
        }

        public static string succeed_string(IDataReader reader)
        {
            string msg = string.Empty;
            return succeed_string(reader, ref msg);
        }

        public static Guid? succeed_guid(IDataReader reader, ref string errorMessage)
        {
            try
            {
                reader.Read();

                try { if (reader.FieldCount > 1) errorMessage = reader[1].ToString(); }
                catch { }

                return Guid.Parse(reader[0].ToString());
            }
            catch { return null; }
            finally { if (!reader.IsClosed) reader.Close(); }
        }

        public static Guid? succeed_guid(IDataReader reader)
        {
            string msg = string.Empty;
            return succeed_guid(reader, ref msg);
        }

        public static DateTime? succeed_datetime(IDataReader reader, ref string errorMessage)
        {
            try
            {
                reader.Read();

                try { if (reader.FieldCount > 1) errorMessage = reader[1].ToString(); }
                catch { }

                return DateTime.Parse(reader[0].ToString());
            }
            catch { return null; }
            finally { if (!reader.IsClosed) reader.Close(); }
        }

        public static DateTime? succeed_datetime(IDataReader reader)
        {
            string msg = string.Empty;
            return succeed_datetime(reader, ref msg);
        }
        
        public static void parse_hierarchy(ref IDataReader reader, ref List<Hierarchy> lstNodes)
        {
            while (reader.Read())
            {
                try
                {
                    Hierarchy item = new Hierarchy();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) item.ID = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["ParentID"].ToString())) item.ParentID = (Guid)reader["ParentID"];
                    if (!string.IsNullOrEmpty(reader["Level"].ToString())) item.Level = (int)reader["Level"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) item.Name = (string)reader["Name"];

                    lstNodes.Add(item);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static void parse_guids(ref IDataReader reader, ref List<Guid> lstGuids, 
            ref long totalCount, ref string errorMessage)
        {
            while (reader.Read())
            {
                try { if (!string.IsNullOrEmpty(reader["ID"].ToString())) lstGuids.Add((Guid)reader["ID"]); }
                catch { }

                if (totalCount <= 0)
                {
                    try { if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = long.Parse(reader["TotalCount"].ToString()); }
                    catch { }
                }
            }

            if (reader.NextResult()) totalCount = ProviderUtil.succeed_long(reader, ref errorMessage);
            else if (!reader.IsClosed) reader.Close();
        }

        public static void parse_guids(ref IDataReader reader, ref List<Guid> lstGuids, ref long totalCount)
        {
            string errorMessage = string.Empty;
            parse_guids(ref reader, ref lstGuids, ref totalCount, ref errorMessage);
        }

        public static void parse_guids(ref IDataReader reader, ref List<Guid> lstGuids, ref string errorMessage)
        {
            long totalCount = 0;
            parse_guids(ref reader, ref lstGuids, ref totalCount, ref errorMessage);
        }

        public static void parse_guids(ref IDataReader reader, ref List<Guid> lstGuids)
        {
            long totalCount = 0;
            parse_guids(ref reader, ref lstGuids, ref totalCount);
        }

        public static void parse_guids(IDataReader reader, ref List<Guid> lstGuids)
        {
            parse_guids(ref reader, ref lstGuids);
        }

        public static void parse_guids(IDataReader reader, ref List<Guid> lstGuids, ref long totalCount)
        {
            parse_guids(ref reader, ref lstGuids, ref totalCount);
        }

        public static void parse_strings(ref IDataReader reader, ref List<string> lstString)
        {
            while (reader.Read())
            {
                try { if (!string.IsNullOrEmpty(reader["Value"].ToString())) lstString.Add((string)reader["Value"]); }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static Dictionary<Guid, bool> parse_items_status_bool(ref IDataReader reader, ref long totalCount)
        {
            Dictionary<Guid, bool> retDic = new Dictionary<Guid, bool>();

            while (reader.Read())
            {
                try
                {
                    Guid? id = null;
                    bool? val = null;

                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = long.Parse(reader["TotalCount"].ToString());
                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) id = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["Value"].ToString())) val = (bool)reader["Value"];

                    if (id.HasValue && val.HasValue) retDic[id.Value] = val.Value;
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return retDic;
        }

        public static Dictionary<Guid, int> parse_items_count(ref IDataReader reader)
        {
            Dictionary<Guid, int> retDic = new Dictionary<Guid, int>();

            while (reader.Read())
            {
                try
                {
                    Guid? id = null;
                    int? count = null;

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) id = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["Count"].ToString())) count = (int)reader["Count"];

                    if (id.HasValue && count.HasValue) retDic[id.Value] = count.Value;
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return retDic;
        }

        public static List<KeyValuePair<Guid, int>> parse_items_count_list(ref IDataReader reader)
        {
            List<KeyValuePair<Guid, int>> ret = new List<KeyValuePair<Guid, int>>();

            while (reader.Read())
            {
                try
                {
                    Guid? id = null;
                    int? count = null;

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) id = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["Count"].ToString())) count = (int)reader["Count"];

                    if (id.HasValue && count.HasValue)
                        ret.Add(new KeyValuePair<Guid, int>(id.Value, count.Value));
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return ret;
        }

        public static void parse_dashboards_count(ref IDataReader reader, ref List<DashboardCount> lstCounts)
        {
            while (reader.Read())
            {
                try
                {
                    DashboardType type = DashboardType.NotSet;
                    if (string.IsNullOrEmpty(reader["Type"].ToString()) ||
                        !Enum.TryParse<DashboardType>((string)reader["Type"], out type)) type = DashboardType.NotSet;

                    DashboardSubType subType = DashboardSubType.NotSet;
                    string subTypeTitle = string.IsNullOrEmpty(reader["SubType"].ToString()) ? string.Empty : (string)reader["SubType"];
                    if (!Enum.TryParse<DashboardSubType>(subTypeTitle, out subType)) subType = DashboardSubType.NotSet;
                    else if (subType != DashboardSubType.NotSet) subTypeTitle = string.Empty;

                    Guid? nodeTypeId = null;
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) nodeTypeId = (Guid)reader["NodeTypeID"];

                    string nodeType = string.IsNullOrEmpty(reader["NodeType"].ToString()) ? string.Empty : (string)reader["NodeType"];

                    if (type == DashboardType.NotSet || (subType == DashboardSubType.NotSet && !nodeTypeId.HasValue)) continue;

                    DashboardCount item = lstCounts.Where(u => u.Type == type).FirstOrDefault();

                    if (item == null)
                    {
                        item = new DashboardCount() { Type = type };
                        lstCounts.Add(item);
                    }

                    DashboardCount subItem = new DashboardCount() {
                        Type = type,
                        SubType = subType,
                        SubTypeTitle = subTypeTitle,
                        NodeTypeID = nodeTypeId,
                        NodeType = nodeType
                    };

                    if (nodeTypeId.HasValue)
                    {
                        DashboardCount ntCount = item.Sub.Where(u => u.NodeTypeID == nodeTypeId.Value).FirstOrDefault();

                        if (ntCount == null) {
                            ntCount = new DashboardCount()
                            {
                                Type = type,
                                NodeTypeID = nodeTypeId,
                                NodeType = nodeType
                            };

                            item.Sub.Add(ntCount);
                        }

                        ntCount.Sub.Add(subItem);
                    }
                    else item.Sub.Add(subItem);

                    if (!string.IsNullOrEmpty(reader["DateOfEffect"].ToString())) subItem.DateOfEffect = (DateTime)reader["DateOfEffect"];
                    if (!string.IsNullOrEmpty(reader["ToBeDone"].ToString())) subItem.ToBeDone = (int)reader["ToBeDone"];
                    if (!string.IsNullOrEmpty(reader["NotSeen"].ToString())) subItem.NotSeen = (int)reader["NotSeen"];
                    if (!string.IsNullOrEmpty(reader["Done"].ToString())) subItem.Done = (int)reader["Done"];
                    if (!string.IsNullOrEmpty(reader["DoneAndInWorkFlow"].ToString()))
                        subItem.DoneAndInWorkFlow = (int)reader["DoneAndInWorkFlow"];
                    if (!string.IsNullOrEmpty(reader["DoneAndNotInWorkFlow"].ToString()))
                        subItem.DoneAndNotInWorkFlow = (int)reader["DoneAndNotInWorkFlow"];
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static int _parse_dashboards(ref IDataReader reader, ref List<Dashboard> lstDashboards,
            ref string message, ref long totalCount, bool closeReader = true)
        {
            List<string> fieldNames = new List<string>();

            while (reader.Read())
            {
                try
                {
                    if (fieldNames.Count == 0)
                    {
                        for (int i = 0, lnt = reader.FieldCount; i < lnt; ++i)
                            fieldNames.Add(reader.GetName(i));
                        fieldNames.Sort();

                        if (fieldNames.Count <= 2)
                        {
                            try
                            {
                                try { if (reader.FieldCount > 1) message = reader[1].ToString(); }
                                catch { }

                                return int.Parse(reader[0].ToString());
                            }
                            catch { closeReader = true; return 0; }
                            finally { if (closeReader && !reader.IsClosed) reader.Close(); }
                        }
                    }

                    Dashboard dash = new Dashboard();

                    if (fieldNames.Any(u => u == "TotalCount") && !string.IsNullOrEmpty(reader["TotalCount"].ToString()))
                        totalCount = (long)reader["TotalCount"];

                    if (fieldNames.Any(u => u == "ID") && !string.IsNullOrEmpty(reader["ID"].ToString()))
                        dash.DashboardID = (long)reader["ID"];
                    if (fieldNames.Any(u => u == "UserID") && !string.IsNullOrEmpty(reader["UserID"].ToString()))
                        dash.UserID = (Guid)reader["UserID"];
                    if (fieldNames.Any(u => u == "NodeID") && !string.IsNullOrEmpty(reader["NodeID"].ToString()))
                        dash.NodeID = (Guid)reader["NodeID"];
                    if (fieldNames.Any(u => u == "NodeAdditionalID") && !string.IsNullOrEmpty(reader["NodeAdditionalID"].ToString()))
                        dash.NodeAdditionalID = (string)reader["NodeAdditionalID"];
                    if (fieldNames.Any(u => u == "NodeName") && !string.IsNullOrEmpty(reader["NodeName"].ToString()))
                        dash.NodeName = (string)reader["NodeName"];
                    if (fieldNames.Any(u => u == "NodeType") && !string.IsNullOrEmpty(reader["NodeType"].ToString()))
                        dash.NodeType = (string)reader["NodeType"];
                    if (fieldNames.Any(u => u == "Type") && !string.IsNullOrEmpty(reader["Type"].ToString()))
                    {
                        try { dash.Type = (DashboardType)Enum.Parse(typeof(DashboardType), (string)reader["Type"]); }
                        catch { dash.Type = DashboardType.NotSet; }
                    }
                    if (fieldNames.Any(u => u == "SubType") && !string.IsNullOrEmpty(reader["SubType"].ToString()))
                    {
                        try { dash.SubType = (DashboardSubType)Enum.Parse(typeof(DashboardSubType), (string)reader["SubType"]); }
                        catch { dash.SubType = DashboardSubType.NotSet; }
                    }
                    if (fieldNames.Any(u => u == "Info") && !string.IsNullOrEmpty(reader["Info"].ToString()))
                        dash.Info = (string)reader["Info"];
                    if (fieldNames.Any(u => u == "Removable") && !string.IsNullOrEmpty(reader["Removable"].ToString()))
                        dash.Removable = (bool)reader["Removable"];
                    if (fieldNames.Any(u => u == "SenderUserID") && !string.IsNullOrEmpty(reader["SenderUserID"].ToString()))
                        dash.SenderUserID = (Guid)reader["SenderUserID"];
                    if (fieldNames.Any(u => u == "SendDate") && !string.IsNullOrEmpty(reader["SendDate"].ToString()))
                        dash.SendDate = (DateTime)reader["SendDate"];
                    if (fieldNames.Any(u => u == "ExpirationDate") && !string.IsNullOrEmpty(reader["ExpirationDate"].ToString()))
                        dash.ExpirationDate = (DateTime)reader["ExpirationDate"];
                    if (fieldNames.Any(u => u == "Seen") && !string.IsNullOrEmpty(reader["Seen"].ToString()))
                        dash.Seen = (bool)reader["Seen"];
                    if (fieldNames.Any(u => u == "ViewDate") && !string.IsNullOrEmpty(reader["ViewDate"].ToString()))
                        dash.ViewDate = (DateTime)reader["ViewDate"];
                    if (fieldNames.Any(u => u == "Done") && !string.IsNullOrEmpty(reader["Done"].ToString()))
                        dash.Done = (bool)reader["Done"];
                    if (fieldNames.Any(u => u == "ActionDate") && !string.IsNullOrEmpty(reader["ActionDate"].ToString()))
                        dash.ActionDate = (DateTime)reader["ActionDate"];

                    lstDashboards.Add(dash);
                }
                catch { closeReader = true; }
            }

            if (closeReader && !reader.IsClosed) reader.Close();
            return 1;
        }

        public static int parse_dashboards(ref IDataReader reader, 
            ref List<Dashboard> lstDashboards, ref string message, ref long totalCount)
        {
            return _parse_dashboards(ref reader, ref lstDashboards, ref message, ref totalCount);
        }

        public static int parse_dashboards(ref IDataReader reader, ref List<Dashboard> lstDashboards, ref string message)
        {
            long totalCount = 0;
            return _parse_dashboards(ref reader, ref lstDashboards, ref message, ref totalCount);
        }

        public static int parse_dashboards(ref IDataReader reader, ref List<Dashboard> lstDashboards, ref long totalCount)
        {
            string msg = string.Empty;
            return parse_dashboards(ref reader, ref lstDashboards, ref msg, ref totalCount);
        }

        public static int parse_dashboards(ref IDataReader reader, ref List<Dashboard> lstDashboards)
        {
            string msg = string.Empty;
            long totalCount = 0;
            return parse_dashboards(ref reader, ref lstDashboards, ref msg, ref totalCount);
        }

        public static bool parse_dashboards(ref IDataReader reader, ref List<Dashboard> lstDashboards, ref RVDataTable retTable)
        {
            string msg = string.Empty;
            long totalCount = 0;
            if (_parse_dashboards(ref reader, ref lstDashboards, ref msg, ref totalCount, false) <= 0 ||
                reader.IsClosed || !reader.NextResult())
            {
                if (!reader.IsClosed) reader.Close();
                return false;
            }

            return reader2table(ref reader, ref retTable, true);
        }

        public static IDataReader execute_reader(string spName, params object[] parameterValues)
        {
            return (IDataReader)SqlHelper.ExecuteReader(ProviderUtil.ConnectionString, spName, parameterValues);
        }

        public static object execute_scalar(string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteScalar(ProviderUtil.ConnectionString, spName, parameterValues);
        }

        public static bool reader2table(ref IDataReader reader, ref RVDataTable retTable, bool closeReader = true)
        {
            try
            {
                int fieldCount = reader.FieldCount;

                for (int i = 0; i < fieldCount; ++i)
                {
                    string colName = reader.GetName(i);
                    if (string.IsNullOrEmpty(colName)) colName = PublicMethods.random_string(5);
                    retTable.Columns.Add(colName, reader.GetFieldType(i));
                }

                while (reader.Read())
                {
                    object[] values = new object[fieldCount];

                    for (int i = 0; i < fieldCount; ++i)
                    {
                        values[i] = reader[i].GetType() == typeof(string) && !string.IsNullOrEmpty((string)reader[i]) ?
                            ((string)reader[i]).Substring(0, Math.Min(1000, ((string)reader[i]).Length)) : reader[i];
                    }

                    retTable.Rows.Add(values);
                }
            }
            catch (Exception e) { closeReader = true; return false; }
            finally
            {
                if (closeReader && !reader.IsClosed)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }

            return true;
        }
    }
}
