using RaaiVan.Modules.GlobalUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.Log
{
    public static class LGParsers
    {
        public static List<Log> logs(DBResultSet results)
        {
            List<Log> retList = new List<Log>();

            RVDataTable table = results.get_table();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                Log log = new Log()
                {
                    LogID = table.GetLong(i, "LogID"),
                    UserID = table.GetGuid(i, "UserID"),
                    Date = table.GetDate(i, "Date"),
                    HostName = table.GetString(i, "HostName"),
                    HostAddress = table.GetString(i, "HostAddress"),
                    Info = table.GetString(i, "Info")
                };

                bool error = false;

                Action acn = PublicMethods.parse_enum<Action>(table.GetString(i, "Action"), defaultValue: Action.None, error: ref error);

                if (error) continue;

                ModuleIdentifier mi = PublicMethods.parse_enum<ModuleIdentifier>(table.GetString(i, "ModuleIdentifier"),
                    defaultValue: ModuleIdentifier.RV, error: ref error);

                if (error) continue;

                log.Action = acn;
                log.ModuleIdentifier = mi;

                retList.Add(log);
            }

            return retList;
        }
    }
}
