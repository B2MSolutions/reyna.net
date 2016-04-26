namespace Reyna.Interfaces
{
    using System;

    public interface IStoreService
    {
        void Put(IMessage message);
    }
}
