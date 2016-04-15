namespace Reyna
{
    using System;

    public interface ILogger : IDisposable
    {
        void ToggleVerbose(bool enabled, string host);

        void Err(string format, params object[] args);

        void Warn(string format, params object[] args);

        void Debug(string format, params object[] args);

        void Info(string format, params object[] args);

        void Verbose(string format, params object[] args);
    }
}
