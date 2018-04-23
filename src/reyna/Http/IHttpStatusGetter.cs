namespace Reyna.Interfaces
{
    using System.Net;

    public interface IHttpStatusGetter
    {
        HttpStatusCode GetStatusCode(HttpWebResponse response);
    }
}