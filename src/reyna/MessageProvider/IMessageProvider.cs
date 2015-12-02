namespace Reyna.MessageProvider
{
    using Reyna.Interfaces;

    internal interface IMessageProvider
    {
        bool CanSend { get; }

        IMessage GetNext();

        void Delete(IMessage message);
    }
}
