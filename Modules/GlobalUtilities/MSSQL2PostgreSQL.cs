using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace RaaiVan.Modules.GlobalUtilities
{
    public enum MSSQLDataType
    {
        None,

        BIGINT,
        BIT,
        CHAR,
        DATETIME,
        DECIMAL,
        FLOAT,
        IMAGE,
        INT,
        NTEXT,
        NVARCHAR,
        UNIQUEIDENTIFIER,
        VARBINARY,
        VARCHAR
    }

    public class SchemaInfo
    {
        public string Table;
        public string Column;
        public bool? IsPrimaryKey;
        public bool? IsIdentity;
        public bool? IsNullable;
        public MSSQLDataType DataType;
        public int? MaxLength;
        public int? Order;
        public string DefaultValue;

        public SchemaInfo()
        {
            DataType = MSSQLDataType.None;
        }

        private static string toScript(string tableName, List<SchemaInfo> columns)
        {
            List<string> strColumns = columns.Select(c =>
            {
                string dt = MSSQL2PostgreSQL.resolve_data_type(c.DataType, c.MaxLength, c.IsIdentity);
                string nullable = c.IsNullable.HasValue && c.IsNullable.Value ? string.Empty : " NOT NULL";
                string defaultValue = c.DataType == MSSQLDataType.UNIQUEIDENTIFIER &&
                    !string.IsNullOrEmpty(c.DefaultValue) && c.DefaultValue.ToLower().Contains("newid()") ?
                    " DEFAULT gen_random_uuid()" : string.Empty;

                return MSSQL2PostgreSQL.resolve_name(c.Column) + " " + dt + nullable + defaultValue;
            }).Where(x => !string.IsNullOrEmpty(x)).ToList();

            List<string> primary = columns
                .Where(c => c.IsPrimaryKey.HasValue && c.IsPrimaryKey.Value)
                .Select(c => MSSQL2PostgreSQL.resolve_name(c.Column)).ToList();

            if (primary.Count > 0) strColumns.Add("PRIMARY KEY (" + string.Join(", ", primary) + ")");

            return "CREATE TABLE IF NOT EXISTS " + MSSQL2PostgreSQL.resolve_table_name(tableName) + " (" +
                "\r\n\t" +
                string.Join(",\r\n\t", strColumns) +
                "\r\n" +
                ");";
        }

        public static string toScript(List<SchemaInfo> info)
        {
            List<string> tables = info.Select(i => i.Table).Distinct().ToList();

            return string.Join("\r\n\r\n", tables.Select(t => toScript(t, MSSQL2PostgreSQL.get_columns(t, info))));
        }
    }
    
    public class ForeignKey
    {
        public string Name;
        public string Table;
        public string Column;
        public string RefTable;
        public string RefColumn;

        public string toScript()
        {
            string constraintName = "fk_" + MSSQL2PostgreSQL.resolve_table_name(Table) + "_" + 
                MSSQL2PostgreSQL.resolve_name(Column) + "_" +
                MSSQL2PostgreSQL.resolve_table_name(RefTable) + "_" + 
                MSSQL2PostgreSQL.resolve_name(RefColumn);

            return
                "ALTER TABLE " + MSSQL2PostgreSQL.resolve_table_name(Table) + "\r\n" +
                "ADD CONSTRAINT " + MSSQL2PostgreSQL.resolve_constraint_name(constraintName) + "\r\n" +
                "FOREIGN KEY(" + MSSQL2PostgreSQL.resolve_name(Column) + ")\r\n" +
                "REFERENCES " + MSSQL2PostgreSQL.resolve_table_name(RefTable) + "(" + MSSQL2PostgreSQL.resolve_name(RefColumn) + ")" +
                ";"; // "\r\n" + "ON DELETE CASCADE;";
        }

        public static string toScript(List<ForeignKey> foreignKeys)
        {
            return string.Join("\r\n\r\n", foreignKeys.Select(t => t.toScript()));
        }
    }

    public class DBIndex
    {
        public string Name;
        public string Table;
        public string Column;
        public int? Order;
        public bool? IsDescending;
        public bool? IsUnique;
        public bool? IsIncludedColumn;
        
        private static string toScript(string indexName, List<DBIndex> data)
        {
            if (data != null) data = data
                    .Where(d => d.Name == indexName)
                    .OrderBy(d => !d.Order.HasValue ? 0 : d.Order.Value)
                    .ToList();

            if (data == null || data.Count == 0) return string.Empty;

            string table = data[0].Table;
            bool isUnique = data[0].IsUnique.HasValue && data[0].IsUnique.Value;
            
            string constraintName = (isUnique ? "ux_" : "ix_") + 
                MSSQL2PostgreSQL.resolve_table_name(table) + "_" +
                MSSQL2PostgreSQL.resolve_name(data[0].Column) + "_" + 
                PublicMethods.random_string(8);

            List<string> columnNames = data.Where(d => !d.IsIncludedColumn.HasValue || !d.IsIncludedColumn.Value)
                .Select(d =>
                {
                    bool isDesc = d.IsDescending.HasValue && d.IsDescending.Value;

                    return MSSQL2PostgreSQL.resolve_name(d.Column) + (isDesc ? " DESC" : "") + " NULLS LAST";
                }).ToList();

            List<string> included = data.Where(d => d.IsIncludedColumn.HasValue && d.IsIncludedColumn.Value)
                .Select(d => MSSQL2PostgreSQL.resolve_name(d.Column)).ToList();

            return columnNames.Count == 0 ? string.Empty :
                "CREATE " + (isUnique ? "UNIQUE " : "") + "INDEX " +
                    MSSQL2PostgreSQL.resolve_constraint_name(constraintName) + "\r\n\t" +
                "ON " + MSSQL2PostgreSQL.resolve_table_name(table) + " USING btree" + "\r\n\t" + 
                "(" + string.Join(", ", columnNames) + ")" +
                (included.Count > 0 ? "\r\n\t" + "INCLUDE(" + string.Join(", ", included) + ")" : "") + ";";
        }

        public static string toScript(List<DBIndex> data)
        {
            List<string> names = data.Select(i => i.Name).Distinct().ToList();

            return string.Join("\r\n\r\n", names.Select(t => toScript(t, data)));
        }
    }

    public static class MSSQL2PostgreSQL
    {
        public static string resolve_name(string name)
        {
            string postgreName = string.Empty;

            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            name.ToList().ForEach(chr => postgreName += (upper.IndexOf(chr) >= 0 ? "_" : "") + chr);

            string retStr = postgreName.ToLower();

            if (retStr.StartsWith("_") && retStr.Length > 1) retStr = retStr.Substring(1);

            if (retStr == "i_d") retStr = "id";
            else if (retStr == "e_mail") retStr = "email";
            else if (retStr == "user_name") retStr = "username";
            else if (retStr == "u_r_l") retStr = "url";
            else if (retStr == "birth_day") retStr = "birthdate";
            else if (retStr == "like") retStr = "like_status";

            return retStr
                .Replace("__", "_")
                .Replace("___", "_")
                .Replace("_i_d", "_id")
                .Replace("w_f_", "wf_")
                .Replace("_p_i_n", "_pin")
                .Replace("work_space", "workspace")
                .Replace("sub_type", "subtype")
                .Replace("m_i_m_e", "mime")
                .Replace("work_flow", "workflow")
                .Replace("feed_back", "feedback")
                .Replace("_f_a_q", "_faq")
                .Replace("f_a_q_", "faq_")
                .Replace("_e_mail", "_email")
                .Replace("_user_name", "_username")
                .Replace("_u_r_l", "_url")
                .Replace("aspnet_", "rv_");
        }

        public static string resolve_table_name(string mssqlName)
        {
            int ind = mssqlName.IndexOf("_");

            string moduleIdentifier = ind > 0 ? mssqlName.Substring(0, ind + 1).ToLower() : string.Empty;
            string tableName = string.IsNullOrEmpty(moduleIdentifier) ? mssqlName : mssqlName.Substring(ind + 1);

            return resolve_name(moduleIdentifier + tableName);
        }

        public static string resolve_data_type(MSSQLDataType type, int? maxLength, bool? identity)
        {
            string strMaxLength = !maxLength.HasValue || maxLength.Value <= 0 ?
                string.Empty : "(" + maxLength.Value.ToString() + ")";

            string retStr = string.Empty;

            switch (type)
            {
                case MSSQLDataType.INT:
                    retStr = identity.HasValue && identity.Value ? "SERIAL" : "INTEGER";
                    break;
                case MSSQLDataType.BIGINT:
                    retStr = identity.HasValue && identity.Value ? "BIGSERIAL" : "BIGINT";
                    break;
                case MSSQLDataType.FLOAT:
                    retStr = "FLOAT";
                    break;
                case MSSQLDataType.BIT:
                    retStr = "BOOLEAN";
                    break;
                case MSSQLDataType.CHAR:
                    retStr = "CHAR";
                    break;
                case MSSQLDataType.DATETIME:
                    retStr = "TIMESTAMP";
                    break;
                case MSSQLDataType.VARCHAR:
                    retStr = "VARCHAR" + strMaxLength;
                    break;
                case MSSQLDataType.NVARCHAR:
                    retStr = "VARCHAR" + strMaxLength;
                    break;
                case MSSQLDataType.VARBINARY:
                    retStr = "BYTEA";
                    break;
                case MSSQLDataType.UNIQUEIDENTIFIER:
                    retStr = "UUID";
                    break;
                default:
                    retStr = string.Empty;
                    break;
            }

            return retStr;
        }

        public static string resolve_constraint_name(string name) {
            return string.IsNullOrEmpty(name) || name.Length <= 39 ? name :
                name.Substring(0, 30) + "_" + PublicMethods.random_string(8);
        }

        public static List<SchemaInfo> get_columns(string tableName, List<SchemaInfo> columns)
        {
            List<MSSQLDataType> invalidDataTypes = new List<MSSQLDataType>() {
                MSSQLDataType.None,
                MSSQLDataType.DECIMAL,
                MSSQLDataType.IMAGE,
                MSSQLDataType.NTEXT
            };

            return columns
                .Where(c => c.Table == tableName && !invalidDataTypes.Any(x => x == c.DataType) &&
                    !string.IsNullOrEmpty(resolve_data_type(c.DataType, c.MaxLength, c.IsIdentity)))
                .OrderBy(c => !c.Order.HasValue ? 0 : c.Order.Value).ToList();
        }

        private static int rows_count_from_ms_sql(string tableName)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();

            cmd.Connection = con;
            cmd.CommandText = "SELECT COUNT(*) FROM dbo." + tableName;

            con.Open();

            try
            {
                return ProviderUtil.succeed_int((IDataReader)cmd.ExecuteReader());
            }
            catch (Exception ex) { return 0; }
            finally { con.Close(); }
        }

        private static DataTable read_from_ms_sql(string tableName, int from, int to, string orderBy)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();

            if (string.IsNullOrEmpty(orderBy)) orderBy = "GETDATE()";

            cmd.Connection = con;
            cmd.CommandText = 
                "SELECT * " + 
                "FROM ( " + 
                        "SELECT ROW_NUMBER() OVER(ORDER BY " + orderBy + " ASC) AS RNDORDER, * " +
                        "FROM dbo." + tableName + 
                    ") AS X " + 
                "WHERE X.RNDORDER BETWEEN " + from.ToString() + " AND " + to.ToString() + " " +
                "ORDER BY X.RNDORDER ASC";

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();

                DataTable dt = new DataTable("tbl");

                if (ProviderUtil.reader2table(ref reader, ref dt)) return dt;
                else return null;
            }
            catch (Exception ex) { return null; }
            finally { con.Close(); }
        }

        private static int transfer_data(string tableName, List<SchemaInfo> columns, int from, int to, string orderBy)
        {
            DataTable table = read_from_ms_sql(tableName, from, to, orderBy);

            int succeedCount = 0;

            if (table != null)
            {
                List<string> colNames = new List<string>();

                for (int i = 0; i < table.Columns.Count; i++)
                    colNames.Add(table.Columns[i].ColumnName);

                colNames = colNames.Where(n => columns.Any(c => c.Column == n && (!c.IsIdentity.HasValue || !c.IsIdentity.Value))).ToList();

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    DataRow row = table.Rows[i];

                    Dictionary<string, object> data = new Dictionary<string, object>();

                    colNames.ForEach(n => data[resolve_name(n)] = row[n]);

                    if (PostgreSQLDBUtil.insert(resolve_table_name(tableName), data)) succeedCount++;
                }
            }

            return succeedCount;
        }

        public static int transfer_data(string tableName, List<SchemaInfo> columns)
        {
            if (!PostgreSQLDBUtil.delete(resolve_table_name(tableName))) return 0;

            string orderBy = columns.Where(c => c.IsIdentity.HasValue && c.IsIdentity.Value)
                .Select(c => c.Column).FirstOrDefault();

            int rowsCount = rows_count_from_ms_sql(tableName), succeedCount = 0;

            int step = 1000, from = 1;

            while (from <= rowsCount)
            {
                succeedCount += transfer_data(tableName, columns, from, to: from + step - 1, orderBy: orderBy);
                from += step;
            }

            return succeedCount;
        }

        public static Dictionary<string, object> transfer_data(List<SchemaInfo> info)
        {
            List<string> tables = info.Select(i => i.Table).Distinct().ToList();

            Dictionary<string, object> result = new Dictionary<string, object>();

            int total = 0;

            tables.ForEach(tbl =>
            {
                int count = transfer_data(tbl, get_columns(tbl, info));

                total += count;

                result["total"] = total;
                result[tbl] = count;
            });

            return result;
        }
    }
}
