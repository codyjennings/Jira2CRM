using System;
using System.IO;
using System.Net;

namespace Jira_Comment_Extraction
{
    public enum authenticationType
    {
        Basic
    }

    public enum httpVerb
    {
        GET
    }

    class RESTClient
    {
        public string endPoint { get; set; }
        public httpVerb httpMethod { get; set; }
        public authenticationType authType { get; set; }
        public string userName { get; set; }
        public string passWord { get; set; }

        public RESTClient()
        {
            endPoint = string.Empty;
            httpMethod = httpVerb.GET;
        }

        public string makeRequest()
        {
            string strResponseValue = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = httpMethod.ToString();
            string authHeader = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(userName + ":" + passWord));
            request.Headers.Add("Authorization", "Basic " + authHeader);
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if(response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException("Error Code: " + response.StatusCode.ToString());
                }
                using (Stream responseStream = response.GetResponseStream())
                {
                    if(responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            strResponseValue = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                strResponseValue = "{\"errorMessages\":[\"" + ex.Message.ToString() + "\"],\"errors\":{}}";
            }
            finally
            {
                if(response != null)
                {
                    ((IDisposable)response).Dispose();
                }
            }
            return strResponseValue;
        }
    }
    class JiraConnection
    {
    }
}
