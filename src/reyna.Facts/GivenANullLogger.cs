namespace Reyna.Facts
{
    using Xunit;
    
    public class GivenANullLogger
    {
        public GivenANullLogger()
        {
            this.Logger = new NullLogger();
        }

        private NullLogger Logger { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.Logger);
        }

        [Fact]
        public void WhenCallingLoggingMethodsShouldNotThrow()
        {
            this.Logger.Debug("test {0}", 1);
            this.Logger.Err("test {0}", 2);
            this.Logger.Warn("test {0}", 3);
            this.Logger.Info("test {0}", 4);
            this.Logger.Verbose("test {0}", 5);
            this.Logger.ToggleVerbose(true, "localhost");
            this.Logger.Dispose();
        }
    }
}