using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Threading;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Log
{
    public class LogController
    {
        private static void _save_log(object obj)
        {
            DataProvider.SaveLog((Guid?)((Pair)obj).First, (Log)((Pair)obj).Second);
        }

        public static void save_log(Guid? applicationId, Log info)
        {
            Pair obj = new Pair(applicationId, info);
            ThreadPool.QueueUserWorkItem(new WaitCallback(_save_log), obj);
        }

        public static List<Log> get_logs(Guid? applicationId, List<Guid> userIds, List<Action> actions, 
            DateTime? beginDate = null, DateTime? finishDate = null, long? lastId = null, int? count = null)
        {
            List<Log> logs = new List<Log>();
            DataProvider.GetLogs(applicationId, ref logs, userIds, actions, beginDate, finishDate, lastId, count);
            return logs;
        }

        public static bool save_error_log(Guid? applicationId, Guid? userId, string subject, 
            string description, ModuleIdentifier? moduleIdentifier, LogLevel level = LogLevel.None)
        {
            return DataProvider.SaveErrorLog(applicationId, new ErrorLog() {
                UserID = userId,
                Subject = subject,
                Description = description,
                Date = DateTime.Now,
                ModuleIdentifier = moduleIdentifier,
                Level = level
            });
        }

        public static bool save_error_log(Guid? applicationId, Guid? userId, string subject,
            Exception exception, ModuleIdentifier? moduleIdentifier, LogLevel level = LogLevel.None)
        {
            if (exception != null && !string.IsNullOrEmpty(exception.Message) && exception.Message.ToLower() == "thread was being aborted.")
                return true; //page redirect throws this error and there is no need to be logged

            string description = PublicMethods.get_exception(exception);

            return save_error_log(applicationId, userId, subject, description, moduleIdentifier, level);
        }
    }
}
