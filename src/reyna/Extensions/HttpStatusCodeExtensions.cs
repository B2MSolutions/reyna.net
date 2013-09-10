namespace Reyna.Extensions
{
    using System.Net;
    using Reyna.Interfaces;

    public static class HttpStatusCodeExtensions
    {
        public static Result ToResult(HttpStatusCode httpStatusCode)
        {
            var statusCode = (int)httpStatusCode;

            if (statusCode >= 200 && statusCode < 300)
            {
                return Result.Ok;
            }

            if (statusCode >= 300 && statusCode < 500)
            {
                return Result.PermanentError;
            }

            if (statusCode >= 500 && statusCode < 600)
            {
                return Result.TemporaryError;
            }

            return Result.PermanentError;
        }
    }
}
