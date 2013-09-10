namespace Reyna.Interfaces
{
    using System;

    public interface IMessageStore
    {
        event EventHandler<EventArgs> MessageAdded;

        void Add(IMessage message);

        IMessage Get();

        IMessage Remove();
    }
}
