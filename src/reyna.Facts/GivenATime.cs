namespace Reyna.Facts
{
    using System;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenATime
    {
        [Fact]
        public void BeforeShouldReturnExpected()
        {
            Assert.Equal(-1, new Time(12, 00).CompareTo(new Time(12, 01)));
            Assert.Equal(-1, new Time(11, 00).CompareTo(new Time(12, 00)));
            Assert.Equal(1, new Time(12, 01).CompareTo(new Time(12, 00)));
            Assert.Equal(0, new Time(12, 01).CompareTo(new Time(12, 01)));
        }

        [Fact]
        public void AfterShouldReturnExpected()
        {
            Assert.Equal(-1, new Time(12, 00).CompareTo(new Time(12, 01)));
            Assert.Equal(-1, new Time(11, 00).CompareTo(new Time(12, 00)));
            Assert.Equal(1, new Time(12, 01).CompareTo(new Time(12, 00)));
            Assert.Equal(0, new Time(12, 01).CompareTo(new Time(12, 01)));
        }

        [Fact]
        public void ShouldSetMinuteOfDay()
        {
            Assert.Equal((12 * 60) + 1, new Time(12, 01).MinuteOfDay);
            Assert.Equal(1000, new Time(1000).MinuteOfDay);
        }

        [Fact]
        public void ShouldThrowOnConstructionIfHourTooLarge()
        {
            Assert.Throws<ArgumentException>(() => new Time(24, 00));
        }

        [Fact]
        public void ShouldThrowOnConstructionIfMinutesTooLarge()
        {
            Assert.Throws<ArgumentException>(() => new Time(23, 60));
        }

        [Fact]
        public void ShouldThrowOnConstructionIfHourTooSmall()
        {
            Assert.Throws<ArgumentException>(() => new Time(-1, 00));
        }

        [Fact]
        public void ShouldThrowOnConstructionIfMinutesTooSmall()
        {
            Assert.Throws<ArgumentException>(() => new Time(23, -1));
        }

        [Fact]
        public void ShouldThrowOnConstructionIfMinuteOfDayTooSmall()
        {
            Assert.Throws<ArgumentException>(() => new Time(-1));
        }

        [Fact]
        public void ShouldThrowOnConstructionIfMinuteOfDayTooLarge()
        {
            Assert.Throws<ArgumentException>(() => new Time(24 * 60));
        }
    }
}
