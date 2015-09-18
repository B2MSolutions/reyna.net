namespace Reyna
{
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class NamedWaitHandle : IWaitHandle
    {
        public NamedWaitHandle(bool initialState, string name)
        {
            this.EventWaitHandle = new EventWaitHandle(initialState, EventResetMode.ManualReset);
        }

        private EventWaitHandle EventWaitHandle { get; set; }

        public bool Set()
        {
            return this.EventWaitHandle.Set();
        }

        public bool WaitOne()
        {
            return this.EventWaitHandle.WaitOne();
        }

        public bool Reset()
        {
            return this.EventWaitHandle.Reset();
        }
    }
}
