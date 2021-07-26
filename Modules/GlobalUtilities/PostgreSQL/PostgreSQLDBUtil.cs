using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace RaaiVan.Modules.GlobalUtilities
{
    public class PostgreSQLDBUtil
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

        public static void procedure_call_test()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(CONNECTION_STRING))
                using (NpgsqlCommand cmd = new NpgsqlCommand("func_test", con))
                {
                    con.Open();

                    using (NpgsqlTransaction tran = con.BeginTransaction())
                    {
                        try
                        {
                            cmd.Transaction = tran;
                            cmd.CommandType = CommandType.StoredProcedure;

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
                                NpgsqlDataReader rd = cmd.ExecuteReader();
                                
                                List<string> columnNames = rd.GetColumnSchema().Select(c => c.ColumnName).ToList();

                                DataTable tbl = new DataTable("tbl");

                                rd.GetColumnSchema().ToList().ForEach(col =>
                                {
                                    string colName = string.IsNullOrEmpty(col.ColumnName) ? 
                                        PublicMethods.random_string(5) : col.ColumnName;
                                    tbl.Columns.Add(colName, col.DataType);
                                });

                                while (rd.Read())
                                {
                                    object[] row = new object[tbl.Columns.Count];
                                    int cnt = rd.GetValues(row);
                                    tbl.Rows.Add(row);
                                }

                                rd.Close();
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
            }
            catch (Exception ex)
            {
                string strEx = ex.ToString();
            }
        }

        public static void procedure_call_test_2()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(CONNECTION_STRING))
                using (NpgsqlCommand cmd = new NpgsqlCommand("func_test_2", con))
                {
                    con.Open();

                    using (NpgsqlTransaction tran = con.BeginTransaction())
                    {
                        try
                        {
                            cmd.Transaction = tran;
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue((int)1); //int
                            cmd.Parameters.AddWithValue((long)1); //bigint
                            cmd.Parameters.AddWithValue((double)1.1); //float
                            cmd.Parameters.AddWithValue(true); //boolean
                            cmd.Parameters.AddWithValue('a'); //char
                            cmd.Parameters.AddWithValue(DateTime.Now); //timestamp
                            cmd.Parameters.AddWithValue("ramin"); //varchar
                            cmd.Parameters.AddWithValue("ramin"); //text
                            cmd.Parameters.AddWithValue(Guid.NewGuid()); //uuid
                            cmd.Parameters.AddWithValue(new byte[10]); //bytea

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
                                NpgsqlDataReader rd = cmd.ExecuteReader();
                                int cnt = 0;

                                while (rd.Read())
                                    ++cnt;

                                rd.Close();
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
            }
            catch (Exception ex)
            {
                string strEx = ex.ToString();
            }
        }
    }
}
