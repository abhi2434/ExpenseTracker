using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ExpenseTracker.Utilities
{
    public class Web
    {
        public static HttpContext CurrentContext
        {
            get
            {
                return HttpContext.Current;
            }
        }
        public static string UserName { get; set; }
        public static string GetCurrentUserName()
        {
            return Web.CurrentContext.User.Identity.Name;
        }
        public static string GetUserName(HttpContextBase context)
        {
            return context.User.Identity.Name;
        }

        public static string GetControllerName(HttpContextBase context)
        {
            return Web.GetControllerName(context.Request);
        }
        public static string GetControllerName(HttpRequestBase request)
        {
            try
            {
                return request.RequestContext.RouteData.Values["controller"].ToString();
            }
            catch { return string.Empty; }
        }

        public static string GetActionName(HttpContextBase context)
        {
            return Web.GetActionName(context.Request);
        }
        public static string GetActionName(HttpRequestBase request)
        {
            try
            {
                return request.RequestContext.RouteData.Values["action"].ToString();
            }
            catch { return string.Empty; }
        }
        public static string GetClientIPAddress(HttpContext context)
        {
            HttpContextWrapper wrapper = new HttpContextWrapper(context);
            return GetClientIPAddress(wrapper);
        }
        public static string GetClientIPAddress()
        {
            HttpContextWrapper wrapper = new HttpContextWrapper(Web.CurrentContext);
            return GetClientIPAddress(wrapper);
        }
        public static string GetClientIPAddress(HttpContextBase context)
        {
            try
            {
                string sIPAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(sIPAddress))
                {
                    return context.Request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    string[] ipArray = sIPAddress.Split(',');
                    return ipArray[0];
                }
            }
            catch { }
            return String.Empty;
        }
        public static Stream GetFileStream(string url, string contentType, Dictionary<string, string> headers = null)
        {
            string serverResponse = string.Empty;

            string delimeter = "?";
            foreach (var header in headers)
            {
                url += string.Format("{0}{1}={2}", delimeter, header.Key, header.Value);
                delimeter = "&";
            }

            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            if (contentType.ToLower().Contains("pdf")
                || contentType.ToLower().Contains("json")
                || contentType.ToLower().Contains("html"))
            {
                HttpWebRequest hrequest = (HttpWebRequest)request;
                hrequest.ContentType = contentType;
                hrequest.Accept = contentType;
                hrequest.KeepAlive = true;
            }
            try
            {
                WebResponse response = request.GetResponse();

                Stream resdataStream = response.GetResponseStream();
                return resdataStream;

            }
            catch { }

            return null;
        }
        public static string PostNormRequest(string url, string postcontent = "", Dictionary<string, string> headers = null)
        {
            string serverResponse = string.Empty;
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

            string content = postcontent;
            byte[] byteArray = Encoding.UTF8.GetBytes(content);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            using (Stream dataStream = request.GetRequestStream())
            {

                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                try
                {
                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream resdataStream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(resdataStream))
                            {
                                serverResponse = reader.ReadToEnd();
                                reader.Close();
                            }

                            response.Close();

                            resdataStream.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is WebException && ((WebException)e).Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse errResp = ((WebException)e).Response;
                        using (Stream respStream = errResp.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(respStream);
                            string response = reader.ReadToEnd();

                            reader.Close();
                        }
                    }
                }
            }
            return serverResponse;

        }

        public static string PostRequest(string url, string postcontent = "", Dictionary<string, string> headers = null)
        {
            string serverResponse = string.Empty;
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";

            StringBuilder bodyBuilder = new StringBuilder();
            if (headers != null)
            {
                foreach (var header in headers)
                    bodyBuilder.AppendFormat("{0}={1}&", header.Key, header.Value);
            }
            if (!string.IsNullOrWhiteSpace(postcontent))
                bodyBuilder.AppendFormat("XMLString={0}", postcontent);
            string content = bodyBuilder.ToString();
            byte[] byteArray = Encoding.UTF8.GetBytes(content);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            using (Stream dataStream = request.GetRequestStream())
            {

                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                try
                {
                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream resdataStream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(resdataStream))
                            {
                                serverResponse = reader.ReadToEnd();
                                reader.Close();
                            }

                            response.Close();

                            resdataStream.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is WebException && ((WebException)e).Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse errResp = ((WebException)e).Response;
                        using (Stream respStream = errResp.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(respStream);
                            string response = reader.ReadToEnd();

                            reader.Close();
                        }
                    }
                }
            }
            return serverResponse;

        }

        public static string SessionUser
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["username"] != null)
                    return HttpContext.Current.Session["username"].ToString();

                return string.Empty;
            }
            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["username"] != null)
                {
                    HttpContext.Current.Session["username"] = value;
                }


            }
        }
    }
}
