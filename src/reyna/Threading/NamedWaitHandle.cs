namespace Reyna
{
    using OpenNETCF.Threading;
    using Reyna.Interfaces;

    public class NamedWaitHandle : IWaitHandle
    {
        public NamedWaitHandle(bool initialState, string name)
        {
            this.EventWaitHandle = new EventWaitHandle(initialState, EventResetMode.ManualReset, name);
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

        public void Close()
        {
            this.EventWaitHandle.Close();
        }
    }
}
