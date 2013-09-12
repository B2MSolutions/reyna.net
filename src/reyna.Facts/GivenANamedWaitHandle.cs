namespace Reyna.Facts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class GivenANamedWaitHandle
    {
        public GivenANamedWaitHandle()
        {
            this.NamedWaitHandle = new NamedWaitHandle(false, "NAME");
        }

        private NamedWaitHandle NamedWaitHandle { get; set; }

        [Fact]
        public void WhenCallingSetShouldNotThrow()
        {
            this.NamedWaitHandle.Set();
        }

        [Fact]
        public void WhenCallingWaitOneShouldNotThrow()
        {
            this.NamedWaitHandle = new NamedWaitHandle(true, "NAME");
            this.NamedWaitHandle.WaitOne();
        }

        [Fact]
        public void WhenCallingResetShouldNotThrow()
        {
            this.NamedWaitHandle.Reset();
        }
    }
}
