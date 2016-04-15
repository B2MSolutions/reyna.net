namespace Reyna
{
    public class NullLogger : ILogger
    {
        public void Err(string format, params object[] args)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void Debug(string format, params object[] args)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Verbose(string format, params object[] args)
        {
        }

        public void Dispose()
        {
        }

        public void ToggleVerbose(bool enabled, string host)
        {
        }
    }
}
