using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using RaaiVan.Modules.FormGenerator;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Jobs
{
    public class CustomAPI
    {
        public static string handle_request(Guid applicationId, Guid currentUserId, string command, HttpRequest req)
        {
            command = command.ToLower();
            Dictionary<string, object> response = new Dictionary<string, object>();

            switch (command)
            {
                case "raaivanquestionaireabstract":
                case "raaivan_questionaire_abstract":
                    response = raaivan_questionaire_abstract(applicationId,
                        PublicMethods.parse_bool(req.Params["DemoRequest"]),
                        PublicMethods.parse_bool(req.Params["DemoDone"]),
                        PublicMethods.parse_bool(req.Params["ProformaRequest"]),
                        PublicMethods.parse_bool(req.Params["ProformaDone"]),
                        PublicMethods.parse_bool(req.Params["ReportRequest"]),
                        PublicMethods.parse_bool(req.Params["ReportDone"]),
                        PublicMethods.parse_bool(req.Params["HasRequest"]));
                    break;
                case "raaivanquestionaireemailrecords":
                case "raaivan_questionaire_email_records":
                    response = raaivan_questionaire_email_records(applicationId,
                        PublicMethods.parse_string(req.Params["Email"]),
                        PublicMethods.parse_bool(req.Params["DemoRequest"]),
                        PublicMethods.parse_bool(req.Params["DemoDone"]),
                        PublicMethods.parse_bool(req.Params["ProformaRequest"]),
                        PublicMethods.parse_bool(req.Params["ProformaDone"]),
                        PublicMethods.parse_bool(req.Params["ReportRequest"]),
                        PublicMethods.parse_bool(req.Params["ReportDone"]),
                        PublicMethods.parse_bool(req.Params["HasRequest"]));
                    break;
                case "isoquestionaireabstract":
                case "iso_questionaire_abstract":
                    response = iso_questionaire_abstract(applicationId);
                    break;
                case "isoquestionaireemailrecords":
                case "iso_questionaire_email_records":
                    response = iso_questionaire_email_records(applicationId,
                        PublicMethods.parse_string(req.Params["Email"]));
                    break;
                case "gkmquestionaireabstract":
                case "gkm_questionaire_abstract":
                    response = gkm_questionaire_abstract(applicationId);
                    break;
                case "gkmquestionaireemailrecords":
                case "gkm_questionaire_email_records":
                    response = gkm_questionaire_email_records(applicationId,
                        PublicMethods.parse_string(req.Params["Email"]));
                    break;
                case "sendemail":
                case "send_email":
                    send_email(applicationId, PublicMethods.parse_string(req.Params["InstanceID"], false),
                        PublicMethods.parse_string(req.Params["ProductName"], false),
                        PublicMethods.parse_string(req.Params["Email"]), ref response);
                    break;
                case "checkkey":
                case "check_key":
                    check_key(PublicMethods.parse_string(req.Params["Key"]), ref response);
                    break;
                case "checkformeditability":
                case "check_form_editability":
                    check_form_editability(applicationId, PublicMethods.parse_guid(req.Params["InstanceID"]), 
                        PublicMethods.parse_guid(req.Params["DateFromElementID"]),
                        PublicMethods.parse_guid(req.Params["DateToElementID"]),
                        PublicMethods.parse_guid(req.Params["ElapsedTimeElementID"]),
                        PublicMethods.parse_int(req.Params["MaxTime"]),
                        ref response);
                    break;
                case "getphrase":
                case "get_phrase":
                    get_phrase(PublicMethods.parse_string(req.Params["Phrase"]), ref response);
                    break;
            }

            return PublicMethods.toJSON(response);
        }

        private static Dictionary<string, object> _parse_raaivan_questionaire_abstract(DBResultSet results)
        {
            RVDataTable table = results.get_table();

            Dictionary<string, object> dic = new Dictionary<string, object>();

            ArrayList lst = null;
            dic["Items"] = lst = new ArrayList();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                string email = table.GetString(i, "Email");
                DateTime? creationDate = table.GetDate(i, "Date");

                int count = table.GetInt(i, "Count", defaultValue: 0).Value;
                Guid? instanceId = table.GetGuid(i, "InstanceID");
                Guid? detailInstanceId = table.GetGuid(i, "DetailInstanceID");

                DateTime? demoRequestDate = table.GetDate(i, "DemoRequest");
                int demoRequestCount = table.GetInt(i, "DemoRequestCount", defaultValue: 0).Value;
                DateTime? demoDone = table.GetDate(i, "DemoDone");

                DateTime? proformaRequestDate = table.GetDate(i, "ProformaRequest");
                int proformaRequestCount = table.GetInt(i, "ProformaRequestCount", defaultValue: 0).Value;
                DateTime? proformaDone = table.GetDate(i, "ProformaDone");

                DateTime? reportRequestDate = table.GetDate(i, "ReportRequest");
                int reportRequestCount = table.GetInt(i, "ReportRequestCount", defaultValue: 0).Value;
                DateTime? reportDone = table.GetDate(i, "ReportDone");

                DateTime? lastRequestDate = table.GetDate(i, "LastRequestDate");

                Dictionary<string, object> itm = new Dictionary<string, object>();

                itm["Email"] = email;
                if (creationDate.HasValue) itm["CreationDate"] = PublicMethods.get_local_date(creationDate, true);
                itm["Count"] = count;
                if (instanceId.HasValue) itm["InstanceID"] = instanceId;
                if (detailInstanceId.HasValue) itm["DetailInstanceID"] = detailInstanceId;
                if (demoRequestDate.HasValue) itm["DemoRequestDate"] = PublicMethods.get_local_date(demoRequestDate, true);
                itm["DemoRequestCount"] = demoRequestCount;
                if (demoDone.HasValue) itm["DemoDone"] = PublicMethods.get_local_date(demoDone, true);
                if (proformaRequestDate.HasValue) itm["ProformaRequestDate"] = PublicMethods.get_local_date(proformaRequestDate, true);
                itm["ProformaRequestCount"] = proformaRequestCount;
                if (proformaDone.HasValue) itm["ProformaDone"] = PublicMethods.get_local_date(proformaDone, true);
                if (reportRequestDate.HasValue) itm["ReportRequestDate"] = PublicMethods.get_local_date(reportRequestDate, true);
                itm["ReportRequestCount"] = reportRequestCount;
                if (reportDone.HasValue) itm["ReportDone"] = PublicMethods.get_local_date(reportDone, true);
                if (lastRequestDate.HasValue) itm["LastRequestDate"] = PublicMethods.get_local_date(lastRequestDate, true);

                lst.Add(itm);
            }

            return dic;
        }

        private static Dictionary<string, object> _parse_iso_questionaire_abstract(DBResultSet results)
        {
            RVDataTable table = results.get_table();

            Dictionary<string, object> dic = new Dictionary<string, object>();

            ArrayList lst = null;
            dic["Items"] = lst = new ArrayList();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                string email = table.GetString(i, "Email");
                DateTime? creationDate = table.GetDate(i, "Date");

                int count = table.GetInt(i, "Count", defaultValue: 0).Value;
                Guid? instanceId = table.GetGuid(i, "InstanceID");
                Guid? detailInstanceId = table.GetGuid(i, "DetailInstanceID");

                DateTime? lastRequestDate = table.GetDate(i, "LastRequestDate");

                Dictionary<string, object> itm = new Dictionary<string, object>();

                itm["Email"] = email;
                if (creationDate.HasValue) itm["CreationDate"] = PublicMethods.get_local_date(creationDate, true);
                itm["Count"] = count;
                if (instanceId.HasValue) itm["InstanceID"] = instanceId;
                if (detailInstanceId.HasValue) itm["DetailInstanceID"] = detailInstanceId;
                if (lastRequestDate.HasValue) itm["LastRequestDate"] = PublicMethods.get_local_date(lastRequestDate, true);

                lst.Add(itm);
            }

            return dic;
        }

        protected static Dictionary<string, object> raaivan_questionaire_abstract(Guid applicationId, bool? demoRequest, 
            bool? demoDone, bool? proformaRequest, bool? proformaDone, bool? reportRequest, bool? reportDone, bool? hasRequest)
        {
            string spName = "EXT_RPT_RaaiVanQuestionaireAbstract";

            try
            {
                DBResultSet results = DBConnector.read(applicationId, spName, applicationId,
                    demoRequest, proformaRequest, reportRequest, hasRequest, demoDone, proformaDone, reportDone);

                return _parse_raaivan_questionaire_abstract(results);
            }
            catch (Exception ex) { return new Dictionary<string, object>(); }
        }

        protected static Dictionary<string, object> raaivan_questionaire_email_records(Guid applicationId, 
            string email, bool? demoRequest, bool? demoDone, bool? proformaRequest, bool? proformaDone, 
            bool? reportRequest, bool? reportDone, bool? hasRequest)
        {
            string spName = "EXT_RPT_RaaiVanQuestionaireEmailRecords";

            try
            {
                DBResultSet results = DBConnector.read(applicationId, spName, applicationId, email,
                    demoRequest, proformaRequest, reportRequest, hasRequest, demoDone, proformaDone, reportDone);

                return _parse_raaivan_questionaire_abstract(results);
            }
            catch (Exception ex) { return new Dictionary<string, object>(); }
        }

        protected static Dictionary<string, object> iso_questionaire_abstract(Guid applicationId)
        {
            string spName = "EXT_RPT_ISOQuestionaireAbstract";

            try
            {
                DBResultSet results = DBConnector.read(applicationId, spName, applicationId);

                return _parse_iso_questionaire_abstract(results);
            }
            catch (Exception ex) { return new Dictionary<string, object>(); }
        }

        protected static Dictionary<string, object> iso_questionaire_email_records(Guid applicationId, string email)
        {
            string spName = "EXT_RPT_ISOQuestionaireEmailRecords";

            try
            {
                DBResultSet results = DBConnector.read(applicationId, spName, applicationId, email);

                return _parse_iso_questionaire_abstract(results);
            }
            catch (Exception ex) { return new Dictionary<string, object>(); }
        }

        protected static Dictionary<string, object> gkm_questionaire_abstract(Guid applicationId)
        {
            string spName = "EXT_RPT_GKMQuestionaireAbstract";

            try
            {
                DBResultSet results = DBConnector.read(applicationId, spName, applicationId);

                return _parse_iso_questionaire_abstract(results);
            }
            catch (Exception ex) { return new Dictionary<string, object>(); }
        }

        protected static Dictionary<string, object> gkm_questionaire_email_records(Guid applicationId, string email)
        {
            string spName = "EXT_RPT_GKMQuestionaireEmailRecords";

            try
            {
                DBResultSet results = DBConnector.read(applicationId, spName, applicationId, email);
                
                return _parse_iso_questionaire_abstract(results);
            }
            catch (Exception ex) { return new Dictionary<string, object>(); }
        }

        protected static void send_email(Guid applicationId, string instanceId, string productName, string email,
            ref Dictionary<string, object> res)
        {
            if (string.IsNullOrEmpty(productName)) return;
            productName = productName.ToLower();

            string bodyText = string.Empty;
            string emailSubject = string.Empty;
            string senderEmail = string.Empty;
            string senderPassword = string.Empty;
            string senderName = string.Empty;
            string smtpAddress = string.Empty;
            int port = 587;
            bool useSSL = false;

            switch (productName)
            {
                case "raaivan":
                    {
                        emailSubject = "تعیین نیازمندی های نرم افزاری مدیریت دانش برای " +
                            email.Substring(0, email.IndexOf("@"));
                        senderEmail = "sales@raaivan.ir";
                        senderPassword = "Hrgn35#7";
                        senderName = "رای ون";
                        smtpAddress = "mail.raaivan.ir";

                        //string url = "http://raaivan.ir/" + productName.ToLower() + "?id=" + instanceId;
                        string url = "http://raaivan.ir/?p=6&id=" + instanceId;

                        bodyText =
                            "<div style='max-width:600px; margin:2rem auto 0rem auto; font-family:tahoma; direction:rtl; text-align:right;'>" +
                                "<div style='text-align:center;'>" +
                                    "<img alt='نرم افزار مدیریت دانش رای ون' style='max-width:20rem;' src='http://raaivan.ir/wp-content/themes/r-theme/assets/images/RaaiVanLogo.png' />" +
                                "</div>" +
                                "<div style='text-align:justify; margin-top:1rem;'>" +
                                    "شما برای شناسایی نیازمندی های نرم افزاری مدیریت دانش خود ابراز تمایل کرده اید. با کلیک روی لینک زیر به صفحه ارزیابی هدایت می شوید. این لینک منقضی نمی شود." +
                                "</div>" +
                                "<div style='width:200px; margin:1.5rem auto 2rem auto; padding:1rem; text-align:center; background-color:rgb(120,183,0); border-radius:100rem; -moz-border-radius:100rem; -webkit-border-radius:100rem; -o-border-radius:100rem;'>" +
                                    "<a href='" + url + "' style='color:white; font-weight:bold; text-decoration:none;'>" +
                                        "شروع ارزیابی" +
                                    "</a>" +
                                "</div>" +
                                "<div style='text-align:center; margin-bottom:1rem;'>" +
                                    "همچنین می توانید آدرس اینترنتی زیر را در مرورگر خود اجرا فرمایید:" +
                                "</div>" +
                                "<div style='text-align:center;'>" +
                                    url +
                                "</div>" +
                            "</div>";
                    }
                    break;
                case "isokmassessment":
                    {
                        emailSubject = "ممیزی سازمان از نظر مدیریت دانش در  " + "ISO" + " برای " +
                            email.Substring(0, email.IndexOf("@"));
                        senderEmail = "sales@raaivan.ir";
                        senderPassword = "Hrgn35#7";
                        senderName = "دانش مارکت";
                        smtpAddress = "mail.raaivan.ir";

                        string url = "http://inknowtex.ir/?p=2009&id=" + instanceId;

                        bodyText =
                            "<div style='max-width:600px; margin:2rem auto 0rem auto; font-family:tahoma; direction:rtl; text-align:right;'>" +
                                "<div style='text-align:center;'>" +
                                    "<img alt='مدیریت دانش در ISO' style='max-width:20rem;' src='http://raaivan.ir/wp-content/themes/r-theme/assets/images/ISO.png' />" +
                                "</div>" +
                                "<div style='text-align:justify; margin-top:1rem;'>" +
                                    "شما برای ارزیابی وضعیت مدیریت دانش در " + "ISO" + " در سازمان خود ابراز تمایل کرده اید. با کلیک روی لینک زیر به صفحه ارزیابی هدایت می شوید. این لینک منقضی نمی شود." +
                                "</div>" +
                                "<div style='width:200px; margin:1.5rem auto 2rem auto; padding:1rem; text-align:center; background-color:rgb(120,183,0); border-radius:100rem; -moz-border-radius:100rem; -webkit-border-radius:100rem; -o-border-radius:100rem;'>" +
                                    "<a href='" + url + "' style='color:white; font-weight:bold; text-decoration:none;'>" +
                                        "شروع ارزیابی" +
                                    "</a>" +
                                "</div>" +
                                "<div style='text-align:center; margin-bottom:1rem;'>" +
                                    "همچنین می توانید آدرس اینترنتی زیر را در مرورگر خود اجرا فرمایید:" +
                                "</div>" +
                                "<div style='text-align:center;'>" +
                                    url +
                                "</div>" +
                            "</div>";
                        break;
                    }
                case "gkmassessment":
                    {
                        emailSubject = "ارزیابی سطح پیاده سازی جامع مدیریت دانش" + " برای " +
                            email.Substring(0, email.IndexOf("@"));
                        senderEmail = "sales@raaivan.ir";
                        senderPassword = "Hrgn35#7";
                        senderName = "اینوتکس ایران";
                        smtpAddress = "mail.raaivan.ir";

                        string url = "http://inknowtex.ir/?p=3673&id=" + instanceId;

                        bodyText =
                            "<div style='max-width:600px; margin:2rem auto 0rem auto; font-family:tahoma; direction:rtl; text-align:right;'>" +
                                "<div style='text-align:center;'>" +
                                    "<img alt='پیاده سازی جامع مدیریت دانش' style='max-width:20rem;' src='http://inknowtex.ir/wp-content/themes/colornews/assets/images/GKM.png' />" +
                                "</div>" +
                                "<div style='text-align:justify; margin-top:1rem;'>" +
                                    "شما برای ارزیابی وضعیت پیاده سازی جامع مدیریت دانش در سازمان خود ابراز تمایل کرده اید. با کلیک روی لینک زیر به صفحه ارزیابی هدایت می شوید. این لینک منقضی نمی شود." +
                                "</div>" +
                                "<div style='width:200px; margin:1.5rem auto 2rem auto; padding:1rem; text-align:center; background-color:rgb(120,183,0); border-radius:100rem; -moz-border-radius:100rem; -webkit-border-radius:100rem; -o-border-radius:100rem;'>" +
                                    "<a href='" + url + "' style='color:white; font-weight:bold; text-decoration:none;'>" +
                                        "شروع ارزیابی" +
                                    "</a>" +
                                "</div>" +
                                "<div style='text-align:center; margin-bottom:1rem;'>" +
                                    "همچنین می توانید آدرس اینترنتی زیر را در مرورگر خود اجرا فرمایید:" +
                                "</div>" +
                                "<div style='text-align:center;'>" +
                                    url +
                                "</div>" +
                            "</div>";
                        break;
                    }
                case "kdm":
                    {
                        emailSubject = "ارزیابی آنلاین وضعیت مدیریت مستندات دانشی" + " برای " +
                            email.Substring(0, email.IndexOf("@"));
                        senderEmail = "sales@raaivan.ir";
                        senderPassword = "Hrgn35#7";
                        senderName = "دانش مارکت";
                        smtpAddress = "mail.raaivan.ir";

                        string url = "http://inknowtex.ir/?p=2094&id=" + instanceId;

                        bodyText =
                            "<div style='max-width:600px; margin:2rem auto 0rem auto; font-family:tahoma; direction:rtl; text-align:right;'>" +
                                "<div style='text-align:center;'>" +
                                    "<img alt='مدیریت مستندات دانشی' style='max-width:20rem;' src='http://inknowtex.ir/wp-content/themes/r-theme/assets/images/kdm.png' />" +
                                "</div>" +
                                "<div style='text-align:justify; margin-top:1rem;'>" +
                                    "شما برای ارزیابی وضعیت مدیریت مستندات دانشی" + " در سازمان خود ابراز تمایل کرده اید. با کلیک روی لینک زیر به صفحه ارزیابی هدایت می شوید. این لینک منقضی نمی شود." +
                                "</div>" +
                                "<div style='width:200px; margin:1.5rem auto 2rem auto; padding:1rem; text-align:center; background-color:rgb(120,183,0); border-radius:100rem; -moz-border-radius:100rem; -webkit-border-radius:100rem; -o-border-radius:100rem;'>" +
                                    "<a href='" + url + "' style='color:white; font-weight:bold; text-decoration:none;'>" +
                                        "شروع ارزیابی" +
                                    "</a>" +
                                "</div>" +
                                "<div style='text-align:center; margin-bottom:1rem;'>" +
                                    "همچنین می توانید آدرس اینترنتی زیر را در مرورگر خود اجرا فرمایید:" +
                                "</div>" +
                                "<div style='text-align:center;'>" +
                                    url +
                                "</div>" +
                            "</div>";
                        break;
                    }
                case "llm":
                    {
                        emailSubject = "ارزیابی آنلاین وضعیت مدیریت درس آموخته ها" + " برای " +
                            email.Substring(0, email.IndexOf("@"));
                        senderEmail = "sales@raaivan.ir";
                        senderPassword = "Hrgn35#7";
                        senderName = "دانش مارکت";
                        smtpAddress = "mail.raaivan.ir";

                        string url = "http://inknowtex.ir/?p=2096&id=" + instanceId;

                        bodyText =
                            "<div style='max-width:600px; margin:2rem auto 0rem auto; font-family:tahoma; direction:rtl; text-align:right;'>" +
                                "<div style='text-align:center;'>" +
                                    "<img alt='مدیریت درس آموخته ها' style='max-width:20rem;' src='http://inknowtex.ir/wp-content/themes/r-theme/assets/images/llm.png' />" +
                                "</div>" +
                                "<div style='text-align:justify; margin-top:1rem;'>" +
                                    "شما برای ارزیابی وضعیت مدیریت درس آموخته ها" + " در سازمان خود ابراز تمایل کرده اید. با کلیک روی لینک زیر به صفحه ارزیابی هدایت می شوید. این لینک منقضی نمی شود." +
                                "</div>" +
                                "<div style='width:200px; margin:1.5rem auto 2rem auto; padding:1rem; text-align:center; background-color:rgb(120,183,0); border-radius:100rem; -moz-border-radius:100rem; -webkit-border-radius:100rem; -o-border-radius:100rem;'>" +
                                    "<a href='" + url + "' style='color:white; font-weight:bold; text-decoration:none;'>" +
                                        "شروع ارزیابی" +
                                    "</a>" +
                                "</div>" +
                                "<div style='text-align:center; margin-bottom:1rem;'>" +
                                    "همچنین می توانید آدرس اینترنتی زیر را در مرورگر خود اجرا فرمایید:" +
                                "</div>" +
                                "<div style='text-align:center;'>" +
                                    url +
                                "</div>" +
                            "</div>";
                        break;
                    }
            }

            bool result = PublicMethods.send_email(applicationId, email, emailSubject,
                bodyText, senderEmail, senderPassword, senderName, smtpAddress, port, useSSL);

            res["Result"] = result ? "ok" : "nok";
        }

        protected static void check_key(string key, ref Dictionary<string, object> res)
        {
            bool isValid = key.Length == 25 &&
                (new System.Text.RegularExpressions.Regex("^[0-9]+$")).IsMatch(key);

            if (isValid)
            {
                for (int i = 0; i < 5; ++i)
                {
                    int sum = 0;

                    for (int x = 0; x < 4; ++x)
                        sum += int.Parse(key.Substring((i * 5) + x, 1));

                    if (int.Parse(key.Substring((i * 5) + 4, 1)) != (sum % 10))
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            res["Result"] = isValid ? "ok" : "nok";
        }

        protected static void check_form_editability(Guid applicationId, Guid? instanceId, 
            Guid? dateFromElementId, Guid? dateToElementId, Guid? elapsedTimeElementId, int? maxTimeInMinutes, 
            ref Dictionary<string, object> res)
        {
            FormType form = null;
            List<FormElement> elems = null;

            if (instanceId.HasValue) form = FGController.get_form_instance(applicationId, instanceId.Value);
            
            if (form == null || (form.Filled.HasValue && form.Filled.Value))
            {
                res["Result"] = "nok";
                if (form != null) res["Filled"] = true;
                return;
            }
            
            if(elapsedTimeElementId.HasValue && maxTimeInMinutes.HasValue && maxTimeInMinutes.Value > 0)
            {
                elems = FGController.get_form_instance_elements(applicationId, instanceId.Value, true);

                FormElement elapsedTimeElem = 
                    elems.Where(u => u.RefElementID == elapsedTimeElementId).FirstOrDefault();

                if(elapsedTimeElem != null)
                {
                    int elapsedTime = 0;
                    if(int.TryParse(elapsedTimeElem.TextValue, out elapsedTime) && elapsedTime >= maxTimeInMinutes)
                    {
                        res["Result"] = "nok";
                        res["Editable"] = false;
                        res["TimeIsOver"] = true;
                        return;
                    }
                }
            }

            if(!dateFromElementId.HasValue && !dateToElementId.HasValue)
            {
                res["Result"] = "ok";
                res["Editable"] = true;
                return;
            }

            if (elems == null)
                elems = FGController.get_form_instance_elements(applicationId, instanceId.Value, true);

            FormElement dateFromElem = null, dateToElem = null;

            if (dateFromElementId.HasValue)
                dateFromElem = elems.Where(u => u.RefElementID == dateFromElementId).FirstOrDefault();

            if (dateToElementId.HasValue)
                dateToElem = elems.Where(u => u.RefElementID == dateToElementId).FirstOrDefault();

            List<KeyValuePair<string, bool>> dates = new List<KeyValuePair<string, bool>>();

            if (dateFromElem != null && !string.IsNullOrEmpty(dateFromElem.TextValue))
                dates.Add(new KeyValuePair<string, bool>(dateFromElem.TextValue.Trim(), true));
            if (dateToElem != null && !string.IsNullOrEmpty(dateToElem.TextValue))
                dates.Add(new KeyValuePair<string, bool>(dateToElem.TextValue.Trim(), false));

            bool editable = true;
            
            foreach(KeyValuePair<string, bool> dt in dates)
            {
                string validDate = "1396/12/12 12:12";

                if (dt.Key.Length != validDate.Length ||
                    !(new Regex("^\\d{4}(\\/\\d{2}){2}\\s\\d{2}:\\d{2}$").IsMatch(dt.Key))) continue;

                MatchCollection parts = new Regex("(\\d{4})|(\\d{2})").Matches(dt.Key);

                DateTime? date = 
                    PublicMethods.persian_to_gregorian_date(int.Parse(parts[0].Value), int.Parse(parts[1].Value),
                    int.Parse(parts[2].Value), int.Parse(parts[3].Value), int.Parse(parts[4].Value), 0);

                bool isFrom = dt.Value;

                if(!date.HasValue || (isFrom && date.Value > DateTime.Now) || (!isFrom && date.Value < DateTime.Now))
                {
                    editable = false;
                    res[isFrom ? "NotBegun" : "Expired"] = true;
                    break;
                }
            }

            res["Result"] = editable ? "ok" : "nok";
            res["Editable"] = editable;
        }

        //Dictionary

        private static Dictionary<string, object> _parse_phrases(DBResultSet results)
        {
            RVDataTable table = results.get_table();

            Dictionary<string, object> dic = new Dictionary<string, object>();

            dic["Items"] = new ArrayList();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                string dictionary = table.GetString(i, "Dictionary");
                string version = table.GetString(i, "Version");
                string data = table.GetString(i, "Data");

                add_to_response(dictionary, version, data, ref dic);
            }

            return dic;
        }

        protected static void add_to_response(string dictionary, string version, string data,
            ref Dictionary<string, object> dic)
        {
            Dictionary<string, object> itm = new Dictionary<string, object>();

            itm["Dictionary"] = dictionary;
            itm["Version"] = version;
            itm["Data"] = data;

            if (!dic.ContainsKey("Items")) dic["Items"] = new ArrayList();

            ((ArrayList)dic["Items"]).Add(itm);
        }

        protected static Dictionary<string, object> get_phrase_local(string phrase)
        {
            string spName = "EP_GetPhrase";

            try
            {
                DBResultSet results = DBConnector.read(null, spName, phrase);

                return _parse_phrases(results);
            }
            catch (Exception ex) { return new Dictionary<string, object>(); }
        }

        protected static bool save_phrase_local(string phrase, string dictionary, string version, string data)
        {
            string spName = "EP_AddPhrase";

            try
            {
                return DBConnector.succeed(null, spName, phrase, dictionary, version, data);
            }
            catch (Exception ex) { return false; }
        }

        protected static bool phrase_exists_local(string dictionary, ref Dictionary<string, object> dic)
        {
            return dic.ContainsKey("Items") && ((ArrayList)dic["Items"]).ToArray()
                .Any(u => ((Dictionary<string, object>)u).ContainsKey("Dictionary") &&
                (string)((Dictionary<string, object>)u)["Dictionary"] == dictionary);
        }

        protected static void get_phrase(string phrase, ref Dictionary<string, object> response)
        {
            response = get_phrase_local(phrase);

            //Dictionary: Webster, Version: 1
            string dictionary = "Webster";
            string version = "1";

            if (!phrase_exists_local(dictionary, ref response))
            {
                string apiKey = "dba04a15-62b0-4287-8802-73f27f321a60";
                string query = "http://www.dictionaryapi.com/api/v1/references/collegiate/xml/" + phrase + "?key=" + apiKey;

                string dt = PublicMethods.web_request(query);

                if (!string.IsNullOrEmpty(dt) && dt.Length > ("<?xml version=\"1.0\" encoding=\"utf - 8\" ?>  <entry_list version=\"1.0\">  </entry_list>".Length + 10))
                {
                    add_to_response(dictionary, version, dt, ref response);
                    save_phrase_local(phrase, dictionary, version, dt);
                }
            }
            //end of 'Dictionary: Webster, Version: 1'
        }

        //end of Dictionary
    }
}