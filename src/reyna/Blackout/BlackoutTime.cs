namespace Reyna
{
    using System;
    using System.Globalization;

    public class BlackoutTime
    {
        public bool CanSendAtTime(DateTime now, string ranges)
        {
            if (String.IsNullOrEmpty(ranges))
            {
                return true;
            }

            string[] rangesSplit = ranges.Split(',');
            foreach (string range in rangesSplit)
            {
                TimeRange timeRange = this.ParseTime(range);
                Time timeNow = new Time(now.Hour, now.Minute);
                if (timeRange.Contains(timeNow))
                {
                    return false;
                }
            }

            return true;
        }

        public TimeRange ParseTime(string range)
        {
            try
            {
                string[] rangeSplit = range.Split('-');
                string format = "HH:mm";

                DateTime from = DateTime.ParseExact(rangeSplit[0], format, CultureInfo.InvariantCulture);
                DateTime to = DateTime.ParseExact(rangeSplit[1], format, CultureInfo.InvariantCulture);

                Time timeFrom = new Time(from.Hour, from.Minute);
                Time timeTo = new Time(to.Hour, to.Minute);
                return new TimeRange(timeFrom, timeTo);
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
