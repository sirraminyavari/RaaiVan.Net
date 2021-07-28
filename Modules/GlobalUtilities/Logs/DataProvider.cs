using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Log
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[LG_" + name + "]"; //'[dbo].' is database owner and 'LG_' is module qualifier
        }

        private static void _parse_logs(ref IDataReader reader, ref List<Log> lstLogs)
        {
            while (reader.Read())
            {
                try
                {
                    Log log = new Log();

                    if (!string.IsNullOrEmpty(reader["LogID"].ToString())) log.LogID = (long)reader["LogID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) log.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Date"].ToString())) log.Date = (DateTime)reader["Date"];
                    if (!string.IsNullOrEmpty(reader["HostName"].ToString())) log.HostName = (string)reader["HostName"];
                    if (!string.IsNullOrEmpty(reader["HostAddress"].ToString())) log.HostAddress = (string)reader["HostAddress"];
                    if (!string.IsNullOrEmpty(reader["Info"].ToString())) log.Info = (string)reader["Info"];

                    Action acn = Action.None;
                    if (!Enum.TryParse<Action>(reader["Action"].ToString(), out acn)) continue;

                    ModuleIdentifier mi = ModuleIdentifier.RV;
                    if (!Enum.TryParse<ModuleIdentifier>(reader["ModuleIdentifier"].ToString(), out mi)) continue;

                    log.Action = acn;
                    log.ModuleIdentifier = mi;

                    lstLogs.Add(log);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static bool SaveLog(Guid? applicationId, Log info)
        {
            try
            {
                if (!info.UserID.HasValue) info.UserID = Guid.Empty;
                if (!info.Action.HasValue || info.Action == Action.None) return false;
                if (!info.Date.HasValue) info.Date = DateTime.Now;
                if (string.IsNullOrEmpty(info.Info)) info.Info = null;

                LogLevel level = LevelOfTheLog.get(info.Action.Value);
                string strLevel = level == LogLevel.None ? null : level.ToString();

                if (info.SubjectID.HasValue) info.SubjectIDs.Add(info.SubjectID.Value);
                else if (!info.SubjectID.HasValue && info.SubjectIDs.Count == 0) info.SubjectIDs.Add(Guid.Empty);

                info.NotAuthorized = info.Action.ToString().IndexOf('_') > 0 ||
                    info.Action == Action.NotAuthorizedAnonymousRequest || 
                    info.Action == Action.PotentialCSRFAttack || info.Action == Action.PotentialReplayAttack;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("SaveLog"), applicationId,
                    info.UserID, info.HostAddress, info.HostName, info.Action.ToString(), strLevel, info.NotAuthorized,
                    ProviderUtil.list_to_string<Guid>(info.SubjectIDs), ',', info.SecondSubjectID, info.ThirdSubjectID, info.FourthSubjectID,
                    info.Date, info.Info, (info.ModuleIdentifier.HasValue ? info.ModuleIdentifier.ToString() : string.Empty)));
            }
            catch { return false; }
        }

        public static void GetLogs(Guid? applicationId, ref List<Log> retLogs, List<Guid> userIds, List<Action> actions,
            DateTime? beginDate, DateTime? finishDate, long? lastId, int? count)
        {
            try
            {
                IDataReader reader = ProviderUtil.execute_reader(GetFullyQualifiedName("GetLogs"), applicationId,
                    ProviderUtil.list_to_string<Guid>(ref userIds), ProviderUtil.list_to_string<Action>(ref actions),
                    ',', beginDate, finishDate, lastId, count);
                _parse_logs(ref reader, ref retLogs);
            }
            catch (Exception ex) { string strEx = ex.ToString(); }
        }

        public static bool SaveErrorLog(Guid? applicationId, ErrorLog info)
        {
            try
            {
                if (info.UserID == Guid.Empty) info.UserID = null;
                if (!info.Date.HasValue) info.Date = DateTime.Now;

                string strLevel = info.Level == LogLevel.None ? LogLevel.Debug.ToString() : info.Level.ToString(); 
                
                return ProviderUtil.succeed(ProviderUtil.execute_reader(GetFullyQualifiedName("SaveErrorLog"),
                    applicationId, info.UserID, info.Subject, info.Description, info.Date, 
                    (info.ModuleIdentifier.HasValue ? info.ModuleIdentifier.ToString() : string.Empty), strLevel));
            }
            catch { return false; }
        }
    }
}
