using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PersonalAssistant.Service.Weather
{
    /// <summary>
    /// Summary description for RequestHandler
    /// </summary>
    public class RequestHandler
    {
        public RequestHandler()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private string response = "";
        private AsyncCallback successCallback, failCallback;
        public void Process(string url, AsyncCallback succCallback, AsyncCallback failCallback)
        {
            this.successCallback = succCallback;
            this.failCallback = failCallback;
            var request = (HttpWebRequest)System.Net.WebRequest.Create(url);
            request.BeginGetResponse(GetUrlCallback, request);

        }
        private void GetUrlCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                try
                {
                    WebResponse webResponse = request.EndGetResponse(result);
                    Stream streamResponse = webResponse.GetResponseStream();

                    // And read it out
                    StreamReader reader = new StreamReader(streamResponse);
                    response = reader.ReadToEnd();

                    reader.Close();
                    reader.Dispose();
                    successCallback.Invoke(new Task((object obj) => { }, response));
                    return;
                }
                catch (WebException e)
                {
                    response = "couldn't connect";
                }
            }
            else
            {
                response = "couldn't connect";
            }
            failCallback.Invoke(new Task((object obj) => { }, response));
        }
    }
}