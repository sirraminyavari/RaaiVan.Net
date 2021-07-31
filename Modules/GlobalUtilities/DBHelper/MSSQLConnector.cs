using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaaiVan.Modules.GlobalUtilities.DBCompositeTypes;

namespace RaaiVan.Modules.GlobalUtilities
{
    public static class MSSQLConnector
    {
        private static RVDataTable get_table(IDataReader reader)
        {
            RVDataTable tbl = new RVDataTable("tbl");
            return !ProviderUtil.reader2table(ref reader, ref tbl, closeReader: false) ? null : tbl;
        }

        public static DBResultSet read(Func<DBResultSet, bool> action, string procedureName, params object[] parameters)
        {
            procedureName = "[dbo].[" + procedureName + "]";

            try
            {
                if (action != null || parameters.Any(p => p != null && typeof(IDBCompositeType).IsAssignableFrom(p.GetType())))
                    return read_structured(action, procedureName, parameters);

                DBResultSet ret = new DBResultSet();

                using (IDataReader reader = ProviderUtil.execute_reader(procedureName, parameters))
                {
                    do
                    {
                        ret.add_table(get_table(reader));
                    } while (reader.NextResult());

                    if (!reader.IsClosed) reader.Close();
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static DBResultSet read_structured(Func<DBResultSet, bool> action, string procedureName, params object[] parameters)
        {
            DBResultSet ret = new DBResultSet();

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            try
            {
                List<string> args = new List<string>();

                for (int i = 0; i < parameters.Length; i++)
                {
                    object p = parameters[i];
                    string name = "@p" + (i + 1).ToString();

                    if (p == null || p == DBNull.Value) args.Add("null");
                    else if (typeof(IDBCompositeType).IsAssignableFrom(p.GetType()))
                    {
                        SqlParameter param = ((IDBCompositeType)p).toMSSQLParameter(name);
                        if (param == null) return ret;
                        cmd.Parameters.Add(param);
                        args.Add(name);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(name, p);
                        args.Add(name);
                    }
                }

                cmd.CommandText = "EXEC" + " " + procedureName + " " + string.Join(", ", args);

                con.Open();

                SqlTransaction tran = action == null ? null : con.BeginTransaction();
                if (tran != null) cmd.Transaction = tran;

                using (IDataReader reader = (IDataReader)cmd.ExecuteReader())
                {
                    do
                    {
                        ret.add_table(get_table(reader));
                    } while (reader.NextResult());

                    if (!reader.IsClosed) reader.Close();
                }

                if(action != null && tran != null)
                {
                    if (!action(ret)) tran.Rollback();
                    else tran.Commit();

                    tran.Dispose();
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
