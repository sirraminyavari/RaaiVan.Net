using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Xml.Linq;

/// <summary>
/// Summary description for ICallWebService
/// </summary>
/// 

public interface ICallWebService
{
    string Url { get; set; }
    string MethodName { get; set; }
    string ResultString { get; set; }
    XDocument ResultXML { get; set; }
    Dictionary<string, string> PParams { get; set; }
    string Invoke();
    string Invoke(bool encode);
    void AddParameter(string Key, string val);
}

public class CallWebService : ICallWebService
{
    private string url;
    private string methodName;
    private string resultString;
    public Dictionary<string, string> PParams { get; set; }
    private XDocument resultXML;

    public string Url
    {
        get { return url; }
        set { url = value; }
    }

    public string MethodName
    {
        get { return methodName; }
        set { methodName = value; }
    }

    public string ResultString
    {
        get { return resultString; }
        set { resultString = value; }
    }

    public XDocument ResultXML
    {
        get { return resultXML; }
        set { resultXML = value; }
    }


    public CallWebService() { }

    public CallWebService(string url, string methodName)
    {
        Url = url;
        MethodName = methodName;
    }

    public void AddParameter(string Key, string val)
    {
        if (PParams == null) PParams = new Dictionary<string, string>();
        PParams.Add(Key, val);
    }

    /// <summary>
    /// Invokes service
    /// </summary>
    /// <param name="encode">Added parameters will encode? (default: true)</param>
    public string Invoke(bool encode)
    {
        return CallWebMethod(Url, MethodName, PParams, new Dictionary<string, string>(), encode);
    }

    /// <summary>
    /// Invokes service
    /// </summary>
    public string Invoke()
    {
        return CallWebMethod(Url, MethodName, PParams, new Dictionary<string, string>(), true);
    }


    public static string CallWebMethod(string url, string methodName, 
        IDictionary<string, string> parameters, IDictionary<string, string> headers, bool encode)
    {
        try
        {
            // security checking
            // checkUser(username);
            //if (Isvalid) ...
            string soapStr =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
              <soap:Body>
                <{0} xmlns=""http://tempuri.org/"">
                  {1}
                </{0}>
              </soap:Body>
            </soap:Envelope>";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";
            req.Headers.Add("SOAPAction", "\"http://tempuri.org/" + methodName + "\"");
            // req.Headers.Add("SOAPAction", "\"http://schemas.microsoft.com/sharepoint/soap/CopyIntoItems\"");
            req.ContentType = "text/xml;charset=\"utf-8\"";
            req.Accept = "text/xml";
            req.Method = "POST";
            req.Headers["Accept-Encoding"] = "gzip";
            req.Headers["Accept-Language"] = "en-us";
            req.Credentials = CredentialCache.DefaultNetworkCredentials;
            req.AutomaticDecompression = DecompressionMethods.GZip;

            if (headers != null)
                foreach (string k in headers.Keys) req.Headers[k] = headers[k];

            //req.Proxy = WebRequest.GetSystemWebProxy();

            using (Stream stm = req.GetRequestStream())
            {
                string postValues = "";
                foreach (var param in parameters)
                {
                    postValues += string.Format("<{0}>{1}</{0}>", (encode ? HttpUtility.UrlEncode(param.Key) : param.Key),
                        (encode ? HttpUtility.UrlEncode(param.Value) : param.Value));
                }

                soapStr = string.Format(soapStr, methodName, postValues);
                using (StreamWriter stmw = new StreamWriter(stm))
                {
                    stmw.Write(soapStr);
                }
            }

            string ResultString = string.Empty;
            using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                string result = responseReader.ReadToEnd();
                // System.Xml.XmlDocument = System.Xml.XmlParserContext
                XDocument ResultXML = XDocument.Parse(result);
                //ResultString = result;
                ResultString = ResultXML.Root.Value;
            }
            return ResultString;
        }
        catch (WebException ex)
        {
            string exMessage = ex.Message;
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml";
            var postStream = webRequest.GetRequestStream();
            byte[] requestBuffer = new byte[1024];
            postStream.Write(requestBuffer, 0, requestBuffer.Length);
            postStream.Close();
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            var responseStream = webResponse.GetResponseStream();
            var responseReader = new StreamReader(responseStream);
            if (ex.Response != null)
                using (responseReader = new StreamReader(ex.Response.GetResponseStream())) exMessage = responseReader.ReadToEnd();
            return exMessage;
        }
    }

    public static string CallWebMethod(string url, string methodName, 
        IDictionary<string, string> parameters, IDictionary<string, string> headers)
    {
        return CallWebMethod(url, methodName, parameters, headers, true);
    }

    public static string CallWebMethod(string url, string methodName, IDictionary<string, string> parameters)
    {
        return CallWebMethod(url, methodName, parameters, new Dictionary<string, string>(), true);
    }
}