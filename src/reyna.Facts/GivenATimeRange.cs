namespace Reyna.Facts
{
    using System;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenATimeRange
    {
        [Fact]
        public void WhenTimeIsWithinRangeContainsShouldReturnTrue()
        {
            Time time = new Time(12, 00);
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            Assert.True(range.Contains(time));

            time = new Time(11, 02);
            Assert.True(range.Contains(time));

            time = new Time(04, 00);
            range = new TimeRange(new Time(21, 00), new Time(05, 00));
            Assert.True(range.Contains(time));

            range = new TimeRange(new Time(00, 00), new Time(23, 59));
            time = new Time(23, 59);
            Assert.True(range.Contains(time));
            time = new Time(00, 00);
            Assert.True(range.Contains(time));
            time = new Time(00, 01);
            Assert.True(range.Contains(time));
            time = new Time(12, 00);
            Assert.True(range.Contains(time));
        }

        [Fact]
        public void WhenTimeIsNotWithinRangeContainsShouldReturnFalse()
        {
            Time time = new Time(12, 02);
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            Assert.False(range.Contains(time));

            time = new Time(19, 58);
            range = new TimeRange(new Time(19, 59), new Time(19, 57));
            Assert.False(range.Contains(time));
        }
    }
}
