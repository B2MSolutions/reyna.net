namespace Reyna
{
    using OpenNETCF.WindowsCE.Notification;
    using Reyna.Interfaces;

    internal sealed class SystemNotifier : ISystemNotifier
    {
        private const string NamedEventPrefix = "\\\\.\\Notifications\\NamedEvents\\";

        public void NotifyOnNetworkConnect(string eventName)
        {
            Notify.RunAppAtEvent(this.GetFullyQualifiedEventName(eventName), NotificationEvent.NetConnect);
        }

        public void ClearNotification(string eventName)
        {
            Notify.RunAppAtEvent(this.GetFullyQualifiedEventName(eventName), NotificationEvent.None);
        }

        private string GetFullyQualifiedEventName(string eventName)
        {
            return string.Format("{0}{1}", SystemNotifier.NamedEventPrefix, eventName);
        }
    }
}
