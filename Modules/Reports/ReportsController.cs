using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Reports
{
    public static class ReportsController
    {
        public static void get_report(Guid applicationId, ModuleIdentifier moduleIdentifier, string reportName,
            ref RVDataTable retReport, ref string retActions, ref Dictionary<string, string> columnsDic, 
            List<ReportParameter> parameters)
        {
            DataProvider.GetReport(applicationId,
                moduleIdentifier, reportName, ref retReport, ref retActions, ref columnsDic, parameters);
        }
    }
}
