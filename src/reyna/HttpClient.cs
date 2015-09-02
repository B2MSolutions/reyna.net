namespace Reyna
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;    
    using Extensions;
    using Reyna.Interfaces;
    using Reyna.Power;

    internal sealed class HttpClient : IHttpClient
    {
        public HttpClient(ICertificatePolicy certificatePolicy)
        {
            if (certificatePolicy != null)
            {
#pragma warning disable 0618
                ServicePointManager.CertificatePolicy = certificatePolicy;
#pragma warning restore 0618
            }
        }

        public Result Post(IMessage message)
        {
            try
            {
                Result result = CanSend();
                if (result != Result.Ok)
                {
                    return result;
                }

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

                return this.RequestAndRespond(request, message.Body);
            }
            catch (Exception)
            {
                return Result.PermanentError;
            }
        }

        internal static Result CanSend()
        {
            ConnectionInfo info = new ConnectionInfo();
            if (!info.Connected)
            {
                return Result.NotConnected;
            }

            if (Preferences.WwanBlackoutRange == null)
            {
                SaveCellularDataAsWwanForBackwardsCompatibility();
            }

            PowerManager powerManager = new PowerManager();
            if (powerManager.IsBatteryCharging() && Preferences.OnChargeBlackout)
            {
                return Result.Blackout;
            }

            if (!powerManager.IsBatteryCharging() && Preferences.OffChargeBlackout)
            {
                return Result.Blackout;
            }

            if (info.Roaming && Preferences.RoamingBlackout)
            {                
                return Result.Blackout;
            }

            BlackoutTime blackoutTime = new BlackoutTime();

            if (info.Wifi && !CanSendNow(blackoutTime, Preferences.WlanBlackoutRange))
            {
                return Result.Blackout;
            }

            if (info.Mobile && !CanSendNow(blackoutTime, Preferences.WwanBlackoutRange))
            {
                return Result.Blackout;
            }

            return Result.Ok;
        }

        internal static HttpStatusCode GetStatusCode(HttpWebResponse response)
        {
            if (response == null)
            {
                return HttpStatusCode.ServiceUnavailable;
            }

            return response.StatusCode;
        }

        private static void SaveCellularDataAsWwanForBackwardsCompatibility()
        {
            TimeRange timeRange = Preferences.CellularDataBlackout;
            if (timeRange != null)
            {
                int hourFrom = (int)Math.Floor((double)timeRange.From.MinuteOfDay / 60);
                int minuteFrom = timeRange.From.MinuteOfDay % 60;
                string blackoutFrom = ZeroPad(hourFrom) + ":" + ZeroPad(minuteFrom);

                int hourTo = (int)Math.Floor((double)timeRange.To.MinuteOfDay / 60);
                int minuteTo = timeRange.To.MinuteOfDay % 60;
                string blackoutTo = ZeroPad(hourTo) + ":" + ZeroPad(minuteTo);

                Preferences.SetWwanBlackoutRange(blackoutFrom + "-" + blackoutTo);
            }
        }

        private static object ZeroPad(int numToPad)
        {
            return numToPad.ToString("D2");
        }

        private static bool CanSendNow(BlackoutTime blackoutTime, string range)
        {
            return blackoutTime.CanSendAtTime(System.DateTime.Now, range);
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
