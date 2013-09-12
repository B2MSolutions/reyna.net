namespace Reyna.Interfaces
{
    using System;

    public interface INetworkStateService : IService
    {
        event EventHandler<EventArgs> NetworkConnected;
    }
}
