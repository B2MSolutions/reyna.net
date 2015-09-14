namespace Reyna
{
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class AutoResetEventAdapter : IWaitHandle
    {
        public AutoResetEventAdapter(bool initialState)
        {
            this.AutoResetEvent = new AutoResetEvent(initialState);
        }

        private AutoResetEvent AutoResetEvent { get; set; }

        public bool Set()
        {
            return this.AutoResetEvent.Set();
        }

        public bool WaitOne()
        {
            return this.AutoResetEvent.WaitOne();
        }

        public bool Reset()
        {
            return this.AutoResetEvent.Reset();
        }
    }
}
