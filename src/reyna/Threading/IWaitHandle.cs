namespace Reyna.Interfaces
{
    internal interface IWaitHandle
    {
        bool Set();

        bool WaitOne();

        bool Reset();
    }
}
