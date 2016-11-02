﻿namespace Reyna
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Text;
    using Extensions;
    using Reyna.Interfaces;

    public sealed class HttpClient : IHttpClient
    {
        public HttpClient(ICertificatePolicy certificatePolicy)
        {
            if (certificatePolicy != null)
            {
#pragma warning disable 0618
                ServicePointManager.CertificatePolicy = certificatePolicy;
#pragma warning restore 0618
            }

            this.TimeProvider = new TimeProvider();
        }

        public ITimeProvider TimeProvider { get; set; }

        public static Result CanSend()
        {
            return new ConnectionManager().CanSend;
        }

        public Result Post(IMessage message)
        {
            try
            {
                var request = WebRequest.Create(message.Url) as HttpWebRequest;
                request.Method = "POST";

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

                request.Headers.Add("submitted", this.TimeProvider.GetEpochInMilliSeconds().ToString(CultureInfo.InvariantCulture));

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
