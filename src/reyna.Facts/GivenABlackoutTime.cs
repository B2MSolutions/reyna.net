namespace Reyna.Facts
{
    using System;
    using Microsoft.Win32;
    using Xunit;
    using Xunit.Extensions;

    public class GivenABlackoutTime
    {
        public GivenABlackoutTime()
        {
            this.BlackoutTime = new BlackoutTime();
        }

        private BlackoutTime BlackoutTime { get; set; }

        [Fact]
        public void WhenCanSendAtTimeAndRangeStoredShouldReturnFalseIfInsideRange()
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(13, 30, 0);
            now = now.Date + ts;
            Assert.False(BlackoutTime.CanSendAtTime(now, "13:00-14:00"));
        }

        [Fact]
        public void WhenCanSendAtTimeAndAMRangeStoredShouldReturnFalseIfInsideRange()
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(10, 30, 0);
            now = now.Date + ts;
            Assert.False(BlackoutTime.CanSendAtTime(now, "10:00-11:00"));
        }

        [Fact]
        public void WhenCanSendAtTimeAndRangeStoredShouldReturnTrueIfOutsideRange()
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(11, 30, 0);
            now = now.Date + ts;
            Assert.True(BlackoutTime.CanSendAtTime(now, "10:00-11:00"));
        }

        [Fact]
        public void WhenParseTimeAndRangeIsAmShouldReturnExpected()
        {
            TimeRange actual = this.BlackoutTime.ParseTime("01:00-09:00");
            Assert.True(actual.From.MinuteOfDay == 60);
            Assert.True(actual.To.MinuteOfDay == (60 * 9));
        }

        [Fact]
        public void WhenParseTimeAndRangeIsPmShouldReturnExpected()
        {
            TimeRange actual = this.BlackoutTime.ParseTime("13:00-21:00");
            Assert.True(actual.From.MinuteOfDay == (60 * 13));
            Assert.True(actual.To.MinuteOfDay == (60 * 21));
        }

        [Fact]
        public void WhenParseTimeAndRangeIsAmAndHasMinutesShouldReturnExpected()
        {
            TimeRange actual = this.BlackoutTime.ParseTime("01:30-09:15");
            Assert.True(actual.From.MinuteOfDay == 60 + 30);
            Assert.True(actual.To.MinuteOfDay == (60 * 9) + 15);
        }

        [Fact]
        public void WhenParseTimeAndRangeIsPmAndHasMinutesShouldReturnExpected()
        {
            TimeRange actual = this.BlackoutTime.ParseTime("13:30-21:15");
            Assert.True(actual.From.MinuteOfDay == (60 * 13) + 30);
            Assert.True(actual.To.MinuteOfDay == (60 * 21) + 15);
        }

        [Fact]
        public void WhenCanSendAtTimeAndRangeFromIsGreaterThanToShouldReturnFalse()
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(18, 10, 0);
            now = now.Date + ts;
            Assert.False(BlackoutTime.CanSendAtTime(now, "17:30-09:00"));
        }

        [Fact]
        public void WhenCanSendAtTimeAndRangeFromIsGreaterThanToAndNowIsOutsideRangeShouldReturnTrue()
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(10, 10, 0);
            now = now.Date + ts;
            Assert.True(BlackoutTime.CanSendAtTime(now, "17:30-09:00"));
        }

        [Fact]
        public void WhenCanSendAtTimeAndNowIsInsideRangeWithMultipleShouldReturnFalse()
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(18, 10, 0);
            now = now.Date + ts;
            Assert.False(BlackoutTime.CanSendAtTime(now, "02:00-03:00,05:00-07:30,18:00-18:15"));
        }

        [Fact]
        public void WhenCanSendAtTimeAndInTotalBlackoutShouldReturnFalse()
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(0, 0, 0);
            now = now.Date + ts;
            Assert.False(BlackoutTime.CanSendAtTime(now, "00:00-23:59"));

            now = DateTime.Now;
            ts = new TimeSpan(23, 59, 30);
            now = now.Date + ts;
            Assert.False(BlackoutTime.CanSendAtTime(now, "00:00-23:59"));

            now = DateTime.Now;
            ts = new TimeSpan(10, 0, 0);
            now = now.Date + ts;
            Assert.False(BlackoutTime.CanSendAtTime(now, "00:00-23:59"));
        }

        [Fact]
        public void WhenCanSendAtTimeAndRangeIsEmptyShouldReturnTrue()
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(1, 0, 0);
            now = now.Date + ts;
            Assert.True(BlackoutTime.CanSendAtTime(now, "01:00-01:00"));

            Assert.True(BlackoutTime.CanSendAtTime(now, string.Empty));

            Assert.True(BlackoutTime.CanSendAtTime(now, null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("2")]
        [InlineData("20")]
        [InlineData("20-")]
        public void WhenCallingParseTimeWithInvalidShouldReturnNull(string value)
        {
            var actual = this.BlackoutTime.ParseTime(value);
            Assert.Null(actual);
        }
    }
}
