using Npgsql;
using RaaiVan.Modules.GlobalUtilities.DBCompositeTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities
{
    public static class PostgreSQLConnector
    {
        private static string HOST = "localhost";
        private static string DBNAME = "ekm_app";
        private static string USERNAME = "postgres";
        private static string PASSWORD = "hrdovjshjpvjo";

        private static bool COMPOSITE_MAPPINGS_DONE = false;

        private static string CONNECTION_STRING
        {
            get
            {
                if (!COMPOSITE_MAPPINGS_DONE)
                {
                    NpgsqlConnection.GlobalTypeMapper.MapComposite<GuidTableType>("guid_table_type");

                    COMPOSITE_MAPPINGS_DONE = true;
                }

                return "Host=" + HOST + "; Username=" + USERNAME + ";Password=" + PASSWORD + ";Database=" + DBNAME;
            }
        }

        public static void config(string host, string dbName, string username, string password)
        {
            if (!string.IsNullOrEmpty(host)) HOST = host;
            if (!string.IsNullOrEmpty(dbName)) DBNAME = dbName;
            if (!string.IsNullOrEmpty(username)) USERNAME = username;
            if (!string.IsNullOrEmpty(password)) PASSWORD = password;
        }

        private static Type resolve_db_data_type(Type dbType)
        {
            if (dbType == typeof(Int32)) return typeof(int);
            else if (dbType == typeof(Int64)) return typeof(long);
            else if (dbType == typeof(float)) return typeof(double);
            else if (dbType == typeof(Double)) return typeof(double);
            else if (dbType == typeof(Boolean)) return typeof(bool);
            else return dbType;
        }

        private static bool add_parameter(NpgsqlCommand cmd, object value)
        {
            Type x = typeof(int);


            if (value == null)
            {
                cmd.Parameters.AddWithValue(null);
                return true;
            }
            else if (typeof(IDBCompositeType).IsAssignableFrom(value.GetType()))
            {
                cmd.Parameters.Add(((IDBCompositeType)value).toNpgSqlParameter());
                return true;
            }
            else if (value.GetType() == typeof(Int32)) value = (int)value;
            else if (value.GetType() == typeof(Int64)) value = (long)value;
            else if (value.GetType() == typeof(float)) value = (double)value;
            else if (value.GetType() == typeof(Double)) value = (double)value;
            else if (value.GetType() == typeof(Boolean)) value = (bool)value;

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

            if (!validTypes.Any(t => t == value.GetType())) return false;

            cmd.Parameters.AddWithValue(value);

            return true;
        }

        private static RVDataTable get_table(NpgsqlDataReader reader)
        {
            List<string> columnNames = reader.GetColumnSchema().Select(c => c.ColumnName).ToList();

            RVDataTable tbl = new RVDataTable("tbl", postgreSqlMode: true);

            reader.GetColumnSchema().ToList().ForEach(col =>
            {
                string colName = string.IsNullOrEmpty(col.ColumnName) ?
                    PublicMethods.random_string(5) : col.ColumnName;
                tbl.Columns.Add(colName, resolve_db_data_type(col.DataType));
            });

            while (reader.Read())
            {
                object[] row = new object[tbl.Columns.Count];
                int cnt = reader.GetValues(row);
                tbl.Rows.Add(row);
            }

            return (RVDataTable)tbl;
        }

        public static DBResultSet read(string procedureName, params object[] parameters)
        {
            procedureName = MSSQL2PostgreSQL.resolve_table_name(procedureName);

            DBResultSet ret = new DBResultSet();
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
                                if (!add_parameter(cmd, parameters[i])) return new DBResultSet();

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
                                    ret.add_table(get_table(reader));
                                    reader.Close();
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            string strEx = ex.ToString();
                            throw ex;
                        }

                        tran.Commit();
                        con.Close();
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
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

        public static bool delete(string tableName)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(CONNECTION_STRING))
            {
                try
                {
                    con.Open();

                    string sql = "DELETE FROM public." + tableName;

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                    return false;
                }
            }
        }

        public static bool insert(string tableName, Dictionary<string, object> row)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(CONNECTION_STRING))
            {
                try
                {
                    con.Open();

                    string sql = "INSERT INTO " + tableName + "(" + string.Join(",", row.Keys.ToList()) + ") " +
                        "VALUES(" + string.Join(",", row.Keys.Select(k => row[k] == null ? "null" : "@" + k).ToList()) + ")";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        row.Keys.Where(k => row[k] != null).ToList().ForEach(k => cmd.Parameters.AddWithValue(k, row[k]));

                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                    return false;
                }
            }
        }
    }
}
