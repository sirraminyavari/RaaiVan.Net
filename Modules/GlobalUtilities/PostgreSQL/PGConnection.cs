using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace RaaiVan.Modules.GlobalUtilities
{
    public class RVConnection
    {
        private static string HOST = "localhost";
        private static string DBNAME = "ekm_app";
        private static string USERNAME = "postgres";
        private static string PASSWORD = "hrdovjshjpvjo";

        private static string CONNECTION_STRING
        {
            get { return "Host=" + HOST + "; Username=" + USERNAME + ";Password=" + PASSWORD + ";Database=" + DBNAME; }
        }

        public static void config(string host, string dbName, string username, string password)
        {
            if (!string.IsNullOrEmpty(host)) HOST = host;
            if (!string.IsNullOrEmpty(dbName)) DBNAME = dbName;
            if (!string.IsNullOrEmpty(username)) USERNAME = username;
            if (!string.IsNullOrEmpty(password)) PASSWORD = password;
        }

        private Type resolve_db_data_type(Type dbType)
        {
            if (dbType == typeof(Int32)) return typeof(int);
            else if (dbType == typeof(Int64)) return typeof(long);
            else if (dbType == typeof(float)) return typeof(double);
            else if (dbType == typeof(Double)) return typeof(double);
            else if (dbType == typeof(Boolean)) return typeof(bool);
            else return dbType;
        }

        private bool add_parameter(NpgsqlCommand cmd, object value)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(Int32)) value = (int)value;
                else if (value.GetType() == typeof(Int64)) value = (long)value;
                else if (value.GetType() == typeof(float)) value = (double)value;
                else if (value.GetType() == typeof(Double)) value = (double)value;
                else if (value.GetType() == typeof(Boolean)) value = (bool)value;
            }

            List<Type> validTypes = new List<Type>() {
                typeof(int),
                typeof(long),
                typeof(double),
                typeof(bool),
                typeof(char),
                typeof(string),
                typeof(DateTime),
                typeof(Guid),
                typeof(byte[])
            };

            if (value != null && !validTypes.Any(t => t == value.GetType())) return false;

            cmd.Parameters.AddWithValue(value);

            return true;
        }

        private bool add_parameter_structured(NpgsqlCommand cmd, object value)
        {
            return true;
            /*
            ++index;

            try
            {
                if (value == null) st.setNull(index, Types.NULL);
                else if (value instanceof RVStructuredParam) {
                    if (!((RVStructuredParam)value).setParameter(index, st)) return false;
                }
            else if (value instanceof Integer) st.setInt(index, (Integer)value);
            else if (value instanceof DateTime) st.setDate(index, new Date(((DateTime)value).toDateTime(DateTimeZone.UTC).getMillis()));
            else if (value instanceof Long) st.setLong(index, (Long)value);
            else if (value instanceof Float) st.setFloat(index, (Float)value);
            else if (value instanceof Double) st.setDouble(index, (Double)value);
            else if (value instanceof String) st.setString(index, (String)value);
            else st.setString(index, value.toString());

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            */
        }

        public RVResultSet read(string procedureName, params object[] parameters)
        {
            if (parameters.Any(p => p.GetType() == typeof(RVStructuredParam)))
                return read_structured(procedureName, parameters);

            RVResultSet ret = new RVResultSet();
            NpgsqlConnection con = null;

            try
            {
                using (con = new NpgsqlConnection(CONNECTION_STRING))
                using (NpgsqlCommand cmd = new NpgsqlCommand(procedureName, con))
                {
                    con.Open();

                    using (NpgsqlTransaction tran = con.BeginTransaction())
                    {
                        try
                        {
                            cmd.Transaction = tran;
                            cmd.CommandType = CommandType.StoredProcedure;

                            for (int i = 0; i < parameters.Length; i++)
                                if (!add_parameter(cmd, parameters[i])) return new RVResultSet();

                            List<string> cursors = new List<string>();

                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    cursors.Add((string)reader[0]);
                                if (!reader.IsClosed) reader.Close();
                            }

                            cursors.ForEach(cr =>
                            {
                                cmd.CommandText = "FETCH ALL IN \"" + cr + "\";";
                                cmd.CommandType = CommandType.Text;

                                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                                {
                                    ret.add_table(reader);
                                    reader.Close();
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            string strEx = ex.ToString();
                        }

                        tran.Commit();
                        con.Close();
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                return new RVResultSet();
            }
            finally
            {
                try
                {
                    if (con != null) con.Close();
                }
                catch (Exception e)
                {
                }
            }
        }

        private RVResultSet read_structured(string procedureName, params object[] parameters)
        {
            return new RVResultSet();
            /*
            SQLServerConnection connection = null;

            RVResultSet ret = new RVResultSet();

            try
            {
                connection = dataSource.getConnection().unwrap(SQLServerConnection.class);

            String[] questionMarks = new String[parameters.length];
            for (int i = 0; i<parameters.length; ++i) questionMarks[i] = "?";

            String proc = "exec " + procedureName +
                    (questionMarks.length > 0 ? " " : "") + String.join(",", questionMarks);
        SQLServerPreparedStatement st = (SQLServerPreparedStatement)connection.prepareStatement(proc);

            for (int i = 0; i<parameters.length; ++i)
                if(!addParameter(st, i, parameters[i])) return new RVResultSet();

        boolean result = st.execute();

            while (result) {
                ResultSet resultSet = st.getResultSet();
        ResultSetMetaData rsmd = resultSet.getMetaData();

        List<String> colNames = new ArrayList<>();
                for (Integer i = 1; i <= rsmd.getColumnCount(); ++i)
                    colNames.add(StringUtils.isBlank(rsmd.getColumnName(i)) ? i.toString() : rsmd.getColumnName(i));

                ret.addTable(colNames);

                while (resultSet.next()) {
                    ret.addRow();

                    for (int i = 1; i <= rsmd.getColumnCount(); ++i)
                        ret.addValue(colNames.get(i - 1), resultSet.getObject(i));
                }

    resultSet.close();

                result = st.getMoreResults();
            }

            return ret;
        }
        catch (Exception ex) {
            return new RVResultSet();
        } finally {
            try {
                if (connection != null && !connection.isClosed()) connection.close();
            } catch (Exception e) {
            }
        }
    */
        }
        public bool succeed(ref string errorMessage, List<Dashboard> retDashboards, string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            try
            {
                object value = result.get_value(rowIndex: 0, columnIndex: 0);
                object error = result.get_value(rowIndex: 1, columnIndex: 0);

                if (error != null && error.GetType() == typeof(string))
                    errorMessage += error;

                if (result.get_tables_count() > 0) parse_dashboards(result, retDashboards, 1);

                if (value == null) return false;

                return value != null && ((value.GetType() == typeof(bool) && ((bool)value)) ||
                       (value.GetType() == typeof(int) && ((int)value) > 0));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool succeed(ref string errorMessage, string procedureName, params object[] parameters)
        {
            return succeed(ref errorMessage, new List<Dashboard>(), procedureName, parameters);
        }

        public bool succeed(List<Dashboard> retDashboards, string procedureName, params object[] parameters)
        {
            string errorMessage = string.Empty;
            return succeed(ref errorMessage, retDashboards, procedureName, parameters);
        }

        public bool succeed(string procedureName, params object[] parameters)
        {
            string errorMessage = string.Empty;
            return succeed(ref errorMessage, new List<Dashboard>(), procedureName, parameters);
        }

        public int get_int(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            try
            {
                object value = result.get_value(rowIndex: 0, columnIndex: 0);
                return value == null ? 0 : (int)value;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public long get_long(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            try
            {
                object value = result.get_value(rowIndex: 0, columnIndex: 0);
                return value == null ? 0 : (long)value;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public DateTime? get_date(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            try
            {
                return (DateTime)result.get_value(rowIndex: 0, columnIndex: 0);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Guid? get_guid(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            try
            {
                return (Guid)result.get_value(rowIndex: 0, columnIndex: 0);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string get_string(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            try
            {
                return (string)result.get_value(rowIndex: 0, columnIndex: 0);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> getStringList(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            List<string> lst = new List<string>();

            try
            {
                for (int i = 0, lnt = result.get_rows_count(); i < lnt; ++i)
                    lst.Add((string)result.get_value(rowIndex: 0, columnIndex: 0));
                return lst;
            }
            catch (Exception ex)
            {
                return lst;
            }
        }

        public List<Guid> get_guid_list(ref long totalCount, ref string errorMessage, string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            List<Guid> lst = new List<Guid>();

            try
            {
                for (int i = 0, lnt = result.get_rows_count(); i < lnt; ++i)
                    lst.Add((Guid)result.get_value(rowIndex: 0, columnIndex: 0));

                if (result.get_tables_count() > 1)
                {
                    try
                    {
                        totalCount = long.Parse(result.get_value(rowIndex: 0, columnIndex: 0, tableIndex: 1).ToString());
                    }
                    catch (Exception e)
                    {
                    }

                    try
                    {
                        errorMessage += (string)result.get_value(tableIndex: 1, rowIndex: 0, columnIndex: 1);
                    }
                    catch (Exception e)
                    {
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                return lst;
            }
        }

        public List<Guid> get_guid_list(string procedureName, params object[] parameters)
        {
            string errorMessage = string.Empty;
            long totalCount = 0;
            return get_guid_list(ref totalCount, ref errorMessage, procedureName, parameters);
        }

        public List<Guid> get_guid_list(ref long totalCount, string procedureName, params object[] parameters)
        {
            string errorMessage = string.Empty;
            return get_guid_list(ref totalCount, ref errorMessage, procedureName, parameters);
        }

        public List<Guid> get_guid_list(ref string errorMessage, string procedureName, params object[] parameters)
        {
            long totalCount = 0;
            return get_guid_list(ref totalCount, errorMessage, procedureName, parameters);
        }

        public List<Hierarchy> get_hierarchy(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            List<Hierarchy> ret = new List<Hierarchy>();

            for (int i = 0, lnt = result.get_rows_count(); i < lnt; ++i)
            {
                try
                {
                    Hierarchy e = new Hierarchy();

                    e.ID = (Guid)result.get_value(rowIndex: i, columnName: "ID");
                    e.ParentID = (Guid)result.get_value(rowIndex: i, columnName: "ParentID");
                    e.Level = (int)result.get_value(rowIndex: i, columnName: "Level");
                    e.Name = (string)result.get_value(rowIndex: i, columnName: "Name");

                    ret.Add(e);
                }
                catch (Exception e)
                {
                }
            }

            return ret;
        }

        public void parse_dashboards(RVResultSet result, List<Dashboard> retDashboards, int tableIndex)
        {
            for (int i = 0, lnt = result.get_rows_count(tableIndex); i < lnt; ++i)
            {
                try
                {
                    Dashboard e = new Dashboard();

                    e.DashboardID = (long)result.get_value(rowIndex: i, columnName: "ID", tableIndex: tableIndex);
                    e.UserID = (Guid)result.get_value(rowIndex: i, columnName: "UserID", tableIndex: tableIndex);
                    e.NodeID = (Guid)result.get_value(i, columnName: "NodeID", tableIndex: tableIndex);
                    e.NodeAdditionalID = (string)result.get_value(rowIndex: i, columnName: "NodeAdditionalID", tableIndex: tableIndex);
                    e.NodeName = (string)result.get_value(rowIndex: i, columnName: "NodeName", tableIndex: tableIndex);
                    e.NodeType = (string)result.get_value(rowIndex: i, columnName: "NodeType", tableIndex: tableIndex);
                    e.Type = PublicMethods.parse_enum<DashboardType>(
                        (string)result.get_value(rowIndex: i, columnName: "Type", tableIndex: tableIndex), DashboardType.NotSet);
                    e.SubType = PublicMethods.parse_enum<DashboardSubType>(
                        (string)result.get_value(rowIndex: i, columnName: "SubType", tableIndex: tableIndex), DashboardSubType.NotSet);
                    e.Info = (string)result.get_value(rowIndex: i, columnName: "Info", tableIndex: tableIndex);
                    e.Removable = (bool)result.get_value(rowIndex: i, columnName: "Removable", tableIndex: tableIndex);
                    e.SenderUserID = (Guid)result.get_value(rowIndex: i, columnName: "SenderUserID", tableIndex: tableIndex);
                    e.SendDate = (DateTime)result.get_value(rowIndex: i, columnName: "SendDate", tableIndex: tableIndex);
                    e.ExpirationDate = (DateTime)result.get_value(rowIndex: i, columnName: "ExpirationDate", tableIndex: tableIndex);
                    e.Seen = (bool)result.get_value(rowIndex: i, columnName: "Seen", tableIndex: tableIndex);
                    e.ViewDate = (DateTime)result.get_value(rowIndex: i, columnName: "ViewDate", tableIndex: tableIndex);
                    e.Done = (bool)result.get_value(rowIndex: i, columnName: "Done", tableIndex: tableIndex);
                    e.ActionDate = (DateTime)result.get_value(rowIndex: i, columnName: "ActionDate", tableIndex: tableIndex);

                    if (!e.DashboardID.HasValue) return;

                    retDashboards.Add(e);
                }
                catch (Exception e)
                {
                }
            }
        }

        public int get_dashboards(ref string errorMessage, List<Dashboard> retDashboards,
            string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            if (result.get_columns_count() <= 2)
            {
                try
                {
                    string value = result.get_value(rowIndex: 1, columnIndex: 0).ToString();

                    try
                    {
                        if (result.get_columns_count() > 1) errorMessage += value;
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

            parse_dashboards(result, retDashboards, 0);

            return 1;
        }

        public int get_dashboards(List<Dashboard> retDashboards, string procedureName, params object[] parameters)
        {
            string msg = string.Empty;
            return get_dashboards(ref msg, retDashboards, procedureName, parameters);
        }

        public List<KeyValuePair<Guid, int>> get_items_count_list(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            List<KeyValuePair<Guid, int>> retDic = new List<KeyValuePair<Guid, int>>();

            for (int i = 0, lnt = result.get_rows_count(); i < lnt; ++i)
            {
                try
                {
                    Guid? id = (Guid?)result.get_value(rowIndex: i, columnName: "ID");
                    int? count = (int?)result.get_value(rowIndex: i, columnName: "Count");

                    if (id.HasValue && count.HasValue) retDic.Add(new KeyValuePair<Guid, int>(id.Value, count.Value));
                }
                catch (Exception e)
                {
                }
            }

            return retDic;
        }

        public Dictionary<Guid, int> getItemsCount(string procedureName, params object[] parameters)
        {
            List<KeyValuePair<Guid, int>> lst = get_items_count_list(procedureName, parameters);

            Dictionary<Guid, int> retDic = new Dictionary<Guid, int>();

            foreach (KeyValuePair<Guid, int> e in lst)
                retDic.Add(e.Key, e.Value);

            return retDic;
        }

        public Dictionary<Guid, bool> get_items_status_bool(string procedureName, params object[] parameters)
        {
            RVResultSet result = read(procedureName, parameters);

            Dictionary<Guid, bool> retDic = new Dictionary<Guid, bool>();

            for (int i = 0, lnt = result.get_rows_count(); i < lnt; ++i)
            {
                try
                {
                    Guid? id = (Guid)result.get_value(rowIndex: i, columnName: "ID");
                    bool? val = (bool)result.get_value(rowIndex: i, columnName: "Value");

                    if (id.HasValue && val.HasValue) retDic.Add(id.Value, val.Value);
                }
                catch (Exception e)
                {
                }
            }

            return retDic;
        }
    }
}
