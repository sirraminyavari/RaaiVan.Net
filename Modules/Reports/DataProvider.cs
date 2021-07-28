using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Reports
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name, ModuleIdentifier moduleIdentifier = ModuleIdentifier.RPT)
        {
            return "[dbo]." + "[" + moduleIdentifier.ToString() + "_" + name + "]"; //'[dbo].' is database owner and 'RPT_' is module qualifier
        }

        protected static void _create_parameter(ReportParameter input, ref SqlCommand command, ref string arguments)
        {
            if (input.StructuredData != null)
            {
                DataTable tbl = new DataTable();

                foreach (ReportParameter prm in input.StructuredParameters)
                    tbl.Columns.Add(prm.Name, prm.Type);
                
                foreach (List<ReportParameter> lst in input.StructuredData)
                {
                    object[] rowData = new object[input.StructuredParameters.Count];

                    bool hasValue = false;

                    for (int i = 0; i < input.StructuredParameters.Count; ++i)
                    {
                        if (lst[i].Value != DBNull.Value && lst[i].Value != null) hasValue = true;
                        rowData[i] = lst[i].Value;
                    }

                    if(hasValue) tbl.Rows.Add(rowData); 
                }

                SqlParameter tblParam = new SqlParameter("@" + input.Name, SqlDbType.Structured);
                tblParam.TypeName = "[dbo].[" + input.TypeName + "]";
                tblParam.Value = tbl;

                command.Parameters.Add(tblParam);

                arguments += (string.IsNullOrEmpty(arguments) ? string.Empty : ", ") + "@" + input.Name;
            }
            else
            {
                if (input.Value != DBNull.Value) command.Parameters.AddWithValue(input.Name, input.Value);
                arguments += (string.IsNullOrEmpty(arguments) ? string.Empty : ", ") +
                    (input.Value == DBNull.Value ? "null" : "@" + input.Name);
            }
        }

        public static void GetReport(Guid applicationId, ModuleIdentifier moduleIdentifier, string reportName,
            ref RVDataTable retReport, ref string retActions, ref Dictionary<string, string> columnsDic, 
            List<ReportParameter> parameters)
        {
            retReport.Clear();
            retReport.TableName = "Report_" + PublicMethods.get_random_number().ToString();

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            string arguments = "@ApplicationID";

            foreach(ReportParameter prm in parameters)
               _create_parameter(prm, ref cmd, ref arguments);

            string spName = GetFullyQualifiedName(reportName, moduleIdentifier);

            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            List<Pair> otherTbls = new List<Pair>();

            try
            {
                con.Open();

                IDataReader reader = (IDataReader)cmd.ExecuteReader();

                if (ProviderUtil.reader2table(ref reader, ref retReport, false))
                {
                    if (reader.NextResult() && reader.Read())
                        retActions = (string)reader[0];
                }

                //Fetch Other Tables
                while (true)
                {
                    if (!reader.NextResult()) break;

                    RVDataTable oTbl = new RVDataTable();
                    if (!ProviderUtil.reader2table(ref reader, ref oTbl, false)) break;

                    string oInfo = reader.NextResult() && reader.Read() ? (string)reader[0] : string.Empty;

                    if (!string.IsNullOrEmpty(oInfo)) otherTbls.Add(new Pair(oTbl, oInfo));
                }
                //end of Fetch Other Tables

                if (!reader.IsClosed)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.RPT);
            }
            finally
            {
                con.Close();
                if (otherTbls.Count > 0) retReport = _fetch(applicationId, retReport, otherTbls, ref columnsDic);
            }
        }
        
        protected static RVDataTable _fetch(Guid applicationId, RVDataTable mainTable, List<Pair> otherTables, 
            ref Dictionary<string, string> columnsDic)
        {
            try
            {
                RVDataTable retTable = new RVDataTable();

                Dictionary<string, string> localDic = null;

                bool fetched = false;

                foreach (Pair p in otherTables)
                {
                    RVDataTable otherTable = (RVDataTable)p.First;
                    Dictionary<string, string> dic = PublicMethods.json2dictionary((string)p.Second);

                    if (dic.ContainsKey("IsDescription") && dic["IsDescription"].ToLower() == "true")
                    {
                        localDic = _parse_description_table(otherTable);
                        if (dic.ContainsKey("IsColumnsDictionary") && dic["IsColumnsDictionary"].ToLower() == "true")
                            columnsDic = localDic;
                        continue;
                    }
                    else
                    {
                        retTable = _fetch(applicationId, mainTable, otherTable, dic, localDic, ref columnsDic);
                        localDic = null;
                        fetched = true;
                    }
                }

                return fetched ? retTable : mainTable;
            }
            catch { return mainTable; }
        }

        protected static RVDataTable _fetch(Guid applicationId, RVDataTable mainTable, RVDataTable otherTable, 
            Dictionary<string, string> info, Dictionary<string, string> localDic, ref Dictionary<string, string> columnsDic)
        {
            if (!info.ContainsKey("ColumnsMap") || !info.ContainsKey("ColumnsToTransfer")) return mainTable;

            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (string itm in info["ColumnsMap"].Split(','))
            {
                string[] tuple = itm.Split(':');
                if (tuple.Length == 2) map[tuple[0].Trim()] = tuple[1].Trim();
            }

            List<string> transfer = info["ColumnsToTransfer"].Split(',').ToList();

            RVDataTable retTable = mainTable;

            Dictionary<string, string> colNamesDic = new Dictionary<string, string>();

            foreach (string str in transfer)
            {
                bool isValidName = (new System.Text.RegularExpressions.Regex("^[A-Za-z][A-Za-z0-9_]*$")).IsMatch(str);

                colNamesDic[str] = isValidName ? str : "r_" + PublicMethods.get_random_number(8) + "r";
                retTable.Columns.Add(colNamesDic[str], typeof(string));
                if (localDic != null && localDic.ContainsKey(str)) columnsDic[colNamesDic[str]] = localDic[str];
            }

            for (int i = 0, lnt = retTable.Rows.Count; i < lnt; ++i)
            {
                DataRow dr = null;
                foreach (DataRow r in otherTable.Rows)
                {
                    if (_is_equal(retTable.Rows[i], r, ref map)) dr = r;
                    else continue;
                }
                if (dr == null) continue;

                foreach (string str in transfer) retTable.Rows[i][colNamesDic[str]] = 
                        PublicMethods.markup2plaintext(applicationId,
                        Expressions.replace(dr[str].ToString(), Expressions.Patterns.HTMLTag, " "));
            }
            
            return retTable;
        }

        protected static bool _is_equal(DataRow sourceDataRow, DataRow destDataRow, 
            ref Dictionary<string, string> columnsMap)
        {
            foreach (string key in columnsMap.Keys)
                if (sourceDataRow[key].ToString().ToLower() != destDataRow[columnsMap[key]].ToString().ToLower()) return false;
            return true;
        }

        protected static Dictionary<string, string> _parse_description_table(RVDataTable tbl)
        {
            Dictionary<string, string> retDic = new Dictionary<string, string>();

            try { foreach (DataRow rw in tbl.Rows) retDic[(string)rw["ColumnName"]] = (string)rw["Translation"]; } //0: ColumnName, 1: Translation
            catch (Exception ex) { string strEx = ex.ToString(); }

            return retDic;
        }
    }
}
