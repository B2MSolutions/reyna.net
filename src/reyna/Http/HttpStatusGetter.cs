namespace Reyna
{
    using System.Net;
    using Interfaces;

    public class HttpStatusGetter : IHttpStatusGetter
    {
        public HttpStatusCode GetStatusCode(HttpWebResponse response)
        {
            return response == null ? HttpStatusCode.ServiceUnavailable : response.StatusCode;
        }
    }
}