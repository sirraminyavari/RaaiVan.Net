using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities
{
    public static class DBConnector
    {
        //to be removed
        public static DBResultSet read_postgre(string procedureName, params object[] parameters)
        {
            return PostgreSQLConnector.read(procedureName, parameters);
        }
        //end of to be removed

        public static DBResultSet read(string procedureName, params object[] parameters)
        {
            return RaaiVanSettings.UsePostgreSQL ?
                PostgreSQLConnector.read(procedureName, parameters) :
                MSSQLConnector.read(procedureName, parameters);
        }

        public static bool succeed(ref string errorMessage, List<Dashboard> retDashboards, string procedureName, params object[] parameters)
        {
            DBResultSet result = read(procedureName, parameters);

            RVDataTable table = result.get_table();

            try
            {
                object value = table.GetValue(row: 0, column: 0);
                object error = table.GetValue(row: 0, column: 1);

                if (error != null && error.GetType() == typeof(string))
                    errorMessage += error;
                
                if (result.TablesCount > 0) parse_dashboards(ref retDashboards, result.get_table(1));

                if (value == null) return false;

                return value != null && ((value.GetType() == typeof(bool) && ((bool)value)) ||
                       (value.GetType() == typeof(int) && ((int)value) > 0));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool succeed(ref string errorMessage, string procedureName, params object[] parameters)
        {
            return succeed(ref errorMessage, new List<Dashboard>(), procedureName, parameters);
        }

        public static bool succeed(List<Dashboard> retDashboards, string procedureName, params object[] parameters)
        {
            string errorMessage = string.Empty;
            return succeed(ref errorMessage, retDashboards, procedureName, parameters);
        }

        public static bool succeed(string procedureName, params object[] parameters)
        {
            string errorMessage = string.Empty;
            return succeed(ref errorMessage, new List<Dashboard>(), procedureName, parameters);
        }

        public static int get_int(string procedureName, params object[] parameters)
        {
            return read(procedureName, parameters).get_table().GetInt(row: 0, column: 0, defaultValue: 0).Value;
        }

        public static long get_long(string procedureName, params object[] parameters)
        {
            return read(procedureName, parameters).get_table().GetLong(row: 0, column: 0, defaultValue: 0).Value;
        }

        public static DateTime? get_date(string procedureName, params object[] parameters)
        {
            return read(procedureName, parameters).get_table().GetDate(row: 0, column: 0);
        }

        public static Guid? get_guid(string procedureName, params object[] parameters)
        {
            return read(procedureName, parameters).get_table().GetGuid(row: 0, column: 0);
        }

        public static string get_string(string procedureName, params object[] parameters)
        {
            return read(procedureName, parameters).get_table().GetString(row: 0, column: 0);
        }

        public static List<string> get_string_list(string procedureName, params object[] parameters)
        {
            RVDataTable table = read(procedureName, parameters).get_table();
            return Enumerable.Range(0, table.Rows.Count).Select(r => table.GetString(row: r, column: 0)).ToList();
        }

        public static List<Guid> get_guid_list(ref long totalCount, ref string errorMessage,
            string procedureName, params object[] parameters)
        {
            DBResultSet result = read(procedureName, parameters);

            RVDataTable table = result.get_table();

            List<Guid> lst = new List<Guid>();

            for (int i = 0, lnt = table.Rows.Count; i < lnt; ++i)
            {
                Guid? val = table.GetGuid(row: i, column: 0);
                if (val.HasValue) lst.Add(val.Value);
            }

            if (result.TablesCount > 1)
            {
                table = result.get_table(1);

                totalCount = table.GetLong(row: 0, column: 0, defaultValue: 0).Value;
                errorMessage += table.GetString(row: 0, column: 1);
            }

            return lst;
        }

        public static List<Guid> get_guid_list(string procedureName, params object[] parameters)
        {
            string errorMessage = string.Empty;
            long totalCount = 0;
            return get_guid_list(ref totalCount, ref errorMessage, procedureName, parameters);
        }

        public static List<Guid> get_guid_list(ref long totalCount, string procedureName, params object[] parameters)
        {
            string errorMessage = string.Empty;
            return get_guid_list(ref totalCount, ref errorMessage, procedureName, parameters);
        }

        public static List<Guid> get_guid_list(ref string errorMessage, string procedureName, params object[] parameters)
        {
            long totalCount = 0;
            return get_guid_list(ref totalCount, errorMessage, procedureName, parameters);
        }

        public static List<Hierarchy> get_hierarchy(string procedureName, params object[] parameters)
        {
            RVDataTable table = read(procedureName, parameters).get_table();

            List<Hierarchy> ret = new List<Hierarchy>();

            for (int i = 0, lnt = table.Rows.Count; i < lnt; ++i)
            {
                Hierarchy e = new Hierarchy();

                e.ID = table.GetGuid(i, "ID");
                e.ParentID = table.GetGuid(i, "ParentID");
                e.Level = table.GetInt(i, "Level");
                e.Name = table.GetString(i, "Name");

                ret.Add(e);
            }

            return ret;
        }

        public static void parse_dashboards(ref List<Dashboard> retDashboards, RVDataTable table, ref long totalCount)
        {
            if (retDashboards == null) retDashboards = new List<Dashboard>();
            if (table == null) return;

            for(int i = 0; i < table.Rows.Count; i++)
            {
                Dashboard e = new Dashboard();

                totalCount = table.GetLong(i, "TotalCount", defaultValue: 0).Value;

                e.DashboardID = table.GetLong(i, "ID");
                e.UserID = table.GetGuid(i, "UserID");
                e.NodeID = table.GetGuid(i, "NodeID");
                e.NodeAdditionalID = table.GetString(i, "NodeAdditionalID");
                e.NodeName = table.GetString(i, "NodeName");
                e.NodeType = table.GetString(i, "NodeType");
                e.Type = table.GetEnum<DashboardType>(i, "Type", DashboardType.NotSet);
                e.SubType = table.GetEnum<DashboardSubType>(i, "SubType", DashboardSubType.NotSet);
                e.Info = table.GetString(i, "Info");
                e.Removable = table.GetBool(i, "Removable");
                e.SenderUserID = table.GetGuid(i, "SenderUserID");
                e.SendDate = table.GetDate(i, "SendDate");
                e.ExpirationDate = table.GetDate(i, "ExpirationDate");
                e.Seen = table.GetBool(i, "Seen");
                e.ViewDate = table.GetDate(i, "ViewDate");
                e.Done = table.GetBool(i, "Done");
                e.ActionDate = table.GetDate(i, "ActionDate");

                if (e.DashboardID.HasValue) retDashboards.Add(e);
            };
        }

        public static void parse_dashboards(ref List<Dashboard> retDashboards, RVDataTable table) {
            long totalCount = 0;
            parse_dashboards(ref retDashboards, table, ref totalCount);
        }

        public static int get_dashboards(ref string errorMessage, ref List<Dashboard> retDashboards,
            string procedureName, params object[] parameters)
        {
            DBResultSet result = read(procedureName, parameters);
            RVDataTable table = result.get_table();

            if (table == null) return 0;

            if (table.Columns.Count <= 2)
            {
                try
                {
                    string value = table.GetString(row: 1, column: 0);

                    try
                    {
                        if (table.Columns.Count > 1) errorMessage += value;
                    }
                    catch (Exception ex)
                    {
                    }

                    return int.Parse(value);
                }
                catch (Exception ex2)
                {
                    return 0;
                }
            }

            parse_dashboards(ref retDashboards, table);

            return 1;
        }

        public static int get_dashboards(List<Dashboard> retDashboards, string procedureName, params object[] parameters)
        {
            string msg = string.Empty;
            return get_dashboards(ref msg, ref retDashboards, procedureName, parameters);
        }

        public static List<KeyValuePair<Guid, int>> get_items_count_list(string procedureName, params object[] parameters)
        {
            RVDataTable table = read(procedureName, parameters).get_table();

            List<KeyValuePair<Guid, int>> retDic = new List<KeyValuePair<Guid, int>>();

            for (int i = 0, lnt = table.Rows.Count; i < lnt; ++i)
            {
                Guid? id = table.GetGuid(i, "ID");
                int? count = table.GetInt(i, "Count");

                if (id.HasValue && count.HasValue) retDic.Add(new KeyValuePair<Guid, int>(id.Value, count.Value));
            }

            return retDic;
        }

        public static Dictionary<Guid, int> get_items_count(string procedureName, params object[] parameters)
        {
            List<KeyValuePair<Guid, int>> lst = get_items_count_list(procedureName, parameters);

            Dictionary<Guid, int> retDic = new Dictionary<Guid, int>();

            foreach (KeyValuePair<Guid, int> e in lst)
                retDic.Add(e.Key, e.Value);

            return retDic;
        }

        public static Dictionary<Guid, bool> get_items_status_bool(string procedureName,
            ref long totalCount, params object[] parameters)
        {
            RVDataTable table = read(procedureName, parameters).get_table();

            Dictionary<Guid, bool> retDic = new Dictionary<Guid, bool>();

            for (int i = 0, lnt = table.Rows.Count; i < lnt; ++i)
            {
                totalCount = table.GetLong(row: i, column: "TotalCount", defaultValue: 0).Value;

                Guid? id = table.GetGuid(i, "ID");
                bool? val = table.GetBool(i, "Value");

                if (id.HasValue && val.HasValue) retDic.Add(id.Value, val.Value);
            }

            return retDic;
        }
    }
}
