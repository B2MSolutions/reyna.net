namespace reyna
{
    using System.Net;
    using System.Text;
    using reyna.Interfaces;
    using Extensions;
    using System.Security.Cryptography.X509Certificates;
    using System.IO;
    using System;

    // TODO
    // split into separate class (make internal)

    // TODO
    // unit testing by wrapup httpWebRequest and HttpWebResponse instead of integration facts
    public class AcceptAllCertificatePolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(
            ServicePoint srvPoint,
            X509Certificate certificate,
            WebRequest request,
            int certificateProblem)
        {
            return true;
        }
    }

    internal class HttpClient : IHttpClient
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
                    statusCode = this.GetStatusCode(response);
                }
            }
            catch (WebException webException)
            {
                var response = webException.Response as HttpWebResponse;
                statusCode = this.GetStatusCode(response);
            }
            catch
            {
            }

            return statusCode.ToResult();
        }

        private HttpStatusCode GetStatusCode(HttpWebResponse response)
        {
            if (response == null)
            {
                return HttpStatusCode.ServiceUnavailable;
            }

            return response.StatusCode;
        }
    }
}
