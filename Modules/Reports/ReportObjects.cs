using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Reports
{
    public class ReportUtilities
    {
        private static List<KeyValuePair<string, Guid>> _ReportIDs = null;

        private static void _init_report_ids() {
            if (_ReportIDs == null) _ReportIDs = new List<KeyValuePair<string, Guid>>();
            if (_ReportIDs.Count > 0) return;

            _ReportIDs.Add(new KeyValuePair<string, Guid>("RV_OveralReport", Guid.Parse("1D2E331F-78AA-4778-84C2-11FA2BFC1BBF")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("RV_LogsReport", Guid.Parse("0285BD01-4B1F-465E-9830-127C3533E575")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("RV_ErrorLogsReport", Guid.Parse("876936F0-653F-4B73-803E-3179D102B38D")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("RV_KnowledgeSupplyIndicatorsReport", Guid.Parse("6B4724E9-3DB1-4879-8E18-4BF7B51F9BAA")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("RV_KnowledgeDemandIndicatorsReport", Guid.Parse("B4CB2F7E-B673-4023-9940-75D292E29CA7")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("RV_SocialContributionIndicatorsReport", Guid.Parse("AE876616-C6F6-4B98-A034-80F16D56C75F")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("RV_ApplicationsPerformanceReport", Guid.Parse("B3F87E3C-F362-4C67-AC77-793522C47F81")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_UsersListReport", Guid.Parse("BEF0F10D-75D3-44FD-B23F-B7F62C7E4DB7")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_MostVisitedItemsReport", Guid.Parse("8895C55A-2C0B-4179-A4DC-BE15F4F3AD59")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_ProfileFilledPercentageReport", Guid.Parse("52C8CF0B-5125-49AD-8A5D-D1B719DD4B31")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_ResumeJobExperienceReport", Guid.Parse("029D800C-B9A7-4978-BF15-ACBE224015FA")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_ResumeEducationReport", Guid.Parse("95FF231F-ACED-42E8-BFEF-87DAC8EFA721")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_ResumeCoursesReport", Guid.Parse("EBA69281-8789-4784-8628-6E74751FD3B5")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_ResumeHonorsReport", Guid.Parse("9856317A-0A48-4AA7-94C5-716F07AC3310")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_ResumeLanguagesReport", Guid.Parse("886A0BDD-324A-4629-8AFB-BA5E2408D725")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_UsersWithSpecificPercentageOfFilledProfileReport", Guid.Parse("C902AA16-FA23-4D7E-A743-EE1A4D1297E6")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("USR_UsersPerformanceReport", Guid.Parse("280DECDE-47B5-49FE-87F7-084672A5465E")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_NodesListReport", Guid.Parse("E82D0656-4AF3-41A5-9D59-4ABA6B810BB5")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_MostFavoriteNodesReport", Guid.Parse("090A7CE7-0CB0-4386-92BF-4F73BC1ABD8D")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_CreatorUsersReport", Guid.Parse("89C37E25-032B-4425-A5C4-687416A0E16E")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_NodeCreatorsReport", Guid.Parse("96E435E4-CFE0-4973-8068-6D06CA93C1B5")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_UserCreatedNodesReport", Guid.Parse("44B85E05-2E77-4CF4-ADC3-7D4E545B9C19")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_NodesCreatedNodesReport", Guid.Parse("E5DA7365-5E6E-498F-90EB-851FBC627A1E")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_NodeCreatedNodesReport", Guid.Parse("03496B68-3AC3-4E63-BFA7-8747AC376256")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_NodesOwnNodesReport", Guid.Parse("8D4540DC-AD47-4FF4-86FC-89A045B794A9")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_NodeOwnNodesReport", Guid.Parse("B79E00EF-2369-4C90-BCD6-0199C1C0EDC0")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_RelatedNodesCountReport", Guid.Parse("DFAAC83F-1D62-4C23-B786-5BEB35451E65")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_RelatedNodesReport", Guid.Parse("56BCDE92-4EA6-4DC1-905C-85ABA63A46C6")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("CN_DownloadedFilesReport", Guid.Parse("5EEE9519-12E9-4302-B1F0-3200A936A44A")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("KW_KnowledgeAdminsReport", Guid.Parse("265B8244-BA5E-4863-903A-00FFE69F3B6A")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("KW_KnowledgeAdminsDetailReport", Guid.Parse("7ADC79EE-9145-4617-96C2-5226819B041D")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("KW_KnowledgeEvaluationsReport", Guid.Parse("59EEBA34-F70D-47F2-96F1-09F83269B2FC")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("KW_KnowledgeEvaluationsDetailReport", Guid.Parse("BB164254-ADC6-4F19-8E53-0AB24C201A58")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("KW_KnowledgeEvaluationsHistoryReport", Guid.Parse("80F82DAC-DD96-4D99-B4B4-DE004C0BDAF4")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("FG_FormsListReport", Guid.Parse("932FF707-65D6-44F7-AFC5-47FE92C56E2F")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("FG_PollDetailReport", Guid.Parse("39858A36-DFEA-4B23-A8DC-F8ED352804BA")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("WF_NodesWorkFlowStatesReport", Guid.Parse("9A6DCD49-D49A-42DD-B001-A3F778169F65")));
            _ReportIDs.Add(new KeyValuePair<string, Guid>("WF_StateNodesCountReport", Guid.Parse("FA9B25AC-7D30-43ED-884C-B3565C66CB7E")));
        }

        public static List<KeyValuePair<string, Guid>> ReportIDs
        {
            get
            {
                _init_report_ids();
                return _ReportIDs == null ? new List<KeyValuePair<string, Guid>>() : _ReportIDs;
            }
        }

        public static Guid? get_report_id(ModuleIdentifier moduleIdentifier, string reportName) {
            string name = (moduleIdentifier.ToString() + "_" + reportName).ToLower();
            Guid? id = null;
            if(ReportIDs.Any(u => u.Key.ToLower() == name))
                id = ReportIDs.Where(u => u.Key.ToLower() == name).First().Value;
            return id;
        }

        public static ReportParameter get_parameter(string name, string type, string value)
        {
            ReportParameter newParam = new ReportParameter();

            newParam.Name = name;
            newParam.Value = DBNull.Value;

            switch (type.ToLower())
            {
                case "bool":
                    newParam.Type = typeof(bool);
                    if (!string.IsNullOrEmpty(value)) newParam.Value = value.ToLower() == "true";
                    break;
                case "string":
                    newParam.Type = typeof(string);
                    if (!string.IsNullOrEmpty(value)) newParam.Value = value;
                    break;
                case "base64":
                    newParam.Type = typeof(string);
                    if (!string.IsNullOrEmpty(value)) newParam.Value = Base64.decode(value);
                    break;
                case "char":
                    newParam.Type = typeof(char);
                    if (!string.IsNullOrEmpty(value)) newParam.Value = (char)value[0];
                    break;
                case "guid":
                    newParam.Type = typeof(Guid);
                    if (!string.IsNullOrEmpty(value)) newParam.Value = Guid.Parse(value);
                    break;
                case "int":
                    newParam.Type = typeof(int);
                    if (!string.IsNullOrEmpty(value)) newParam.Value = int.Parse(value);
                    break;
                case "float":
                    newParam.Type = typeof(float);
                    if (!string.IsNullOrEmpty(value)) newParam.Value = float.Parse(value);
                    break;
                case "double":
                    newParam.Type = typeof(double);
                    if (!string.IsNullOrEmpty(value)) newParam.Value = double.Parse(value);
                    break;
                case "datetime":
                    newParam.Type = typeof(DateTime);
                    if (!string.IsNullOrEmpty(value))
                    {
                        newParam.Value = DateTime.Parse(value);

                        List<string> lst = new List<string>() {
                            "dateto", "todate", "enddate", "finishdate", "uppercreationdatelimit"
                        };

                        string nm = name.ToLower();

                        if(lst.Any(u => nm.IndexOf(u) >= 0)) newParam.Value = ((DateTime)newParam.Value).AddDays(1);
                    }
                    break;
                case "now":
                    newParam.Type = typeof(DateTime);
                    newParam.Value = DateTime.Now;
                    break;
                case "structure":
                    if (string.IsNullOrEmpty(value)) break;
                    List<string> _data = ListMaker.get_string_items(Base64.decode(value), '|');
                    if (_data.Count < 2) break;

                    List<string> names = ListMaker.get_string_items(_data[0], ':');
                    List<string> types = ListMaker.get_string_items(_data[1], ':');
                    List<string> values = _data.Count <= 2 ? new List<string>() : ListMaker.get_string_items(_data[2], ':');

                    if (names.Count < 2 || types.Count != (names.Count - 2)) break;

                    newParam.Name = name;
                    newParam.TypeName = names[0];
                    int recordsCount = int.Parse(names[1]);

                    if (values.Count != 0 && values.Count != (types.Count * recordsCount)) break;

                    newParam.StructuredParameters = new List<ReportParameter>();
                    for (var i = 2; i < names.Count; ++i)
                        newParam.StructuredParameters.Add(get_parameter(names[i], types[i - 2], string.Empty));

                    newParam.StructuredData = new List<List<ReportParameter>>();

                    for (int i = 0, fieldsCount = types.Count; i < recordsCount; ++i)
                    {
                        List<ReportParameter> p = new List<ReportParameter>();

                        for (int j = 0; j < newParam.StructuredParameters.Count; ++j)
                            p.Add(get_parameter(names[j + 2], types[j], values[(i * fieldsCount) + j]));

                        newParam.StructuredData.Add(p);
                    }
                    
                    break;
            }

            return newParam;
        }
    }

    public class ReportParameter
    {
        public string Name;
        public Type Type;
        public string TypeName;
        public object Value;
        public List<ReportParameter> StructuredParameters;
        public List<List<ReportParameter>> StructuredData;
    }
}
