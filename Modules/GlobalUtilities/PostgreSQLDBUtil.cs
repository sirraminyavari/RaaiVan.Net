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
        private static string DBNAME = "ramin";
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
    }
}
