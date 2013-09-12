namespace Reyna
{
    using OpenNETCF.WindowsCE.Notification;
    using Reyna.Interfaces;

    internal sealed class SystemNotifier : ISystemNotifier
    {
        public void NotifyOnNetworkConnect(string eventName)
        {
            Notify.RunAppAtEvent(eventName, NotificationEvent.NetConnect);
        }

        public void ClearNotification(string eventName)
        {
            Notify.RunAppAtEvent(eventName, NotificationEvent.None);
        }
    }
}
