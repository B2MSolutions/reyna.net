namespace Reyna.Interfaces
{
    internal interface ISystemNotifier
    {
        void NotifyOnNetworkConnect(string eventName);

        void ClearNotification(string eventName);
    }
}
