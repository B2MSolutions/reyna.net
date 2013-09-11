namespace Reyna.Interfaces
{
    using System;

    public interface IService : IDisposable
    {
        void Start();
        
        void Stop();
    }
}
