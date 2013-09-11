namespace Reyna
{
    using System;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Extensions;
    using Reyna.Interfaces;

    internal sealed class HttpClient : IHttpClient
    {
        public HttpClient()
        {
// TODO
// remove obsolete pragma
#pragma warning disable 0618
            ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
#pragma warning restore 0618
        }

        public Result Post(IMessage message)
        {
            try
            {
                var request = WebRequest.Create(message.Url) as HttpWebRequest;
                request.Method = "POST";

                // TODO
                // replace this fugly header setting
                foreach (string key in message.Headers.Keys)
                {
                    var value = message.Headers[key];

                    if (key == "content-type")
                    {
                        request.ContentType = value;
                        continue;
                    }

                    request.Headers.Add(key, value);
                }

                return this.RequestAndRespond(request, message.Body);
            }
            catch (Exception)
            {
                return Result.PermanentError;
            }
        }

        internal static HttpStatusCode GetStatusCode(HttpWebResponse response)
        {
            if (response == null)
            {
                return HttpStatusCode.ServiceUnavailable;
            }

            return response.StatusCode;
        }

        private Result RequestAndRespond(HttpWebRequest request, string content)
        {
            HttpStatusCode statusCode = HttpStatusCode.NotFound;

            try
            {
                var contentBytes = Encoding.UTF8.GetBytes(content);
                request.ContentLength = contentBytes.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(contentBytes, 0, contentBytes.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    statusCode = HttpClient.GetStatusCode(response);
                }
            }
            catch (WebException webException)
            {
                var response = webException.Response as HttpWebResponse;
                statusCode = HttpClient.GetStatusCode(response);
            }

            return HttpStatusCodeExtensions.ToResult(statusCode);
        }
    }
}
