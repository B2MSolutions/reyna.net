namespace Reyna.Interfaces
{
    using System;

    internal interface IRepository
    {
        event EventHandler<EventArgs> MessageAdded;

        long AvailableMessagesCount { get; }

        void Initialise();

        void Add(IMessage message);

        void Add(IMessage message, long storageSizeLimit);

        IMessage Get();

        IMessage GetNextMessageAfter(long messageId);

        IMessage Remove();

        void DeleteMessagesFrom(IMessage message);
    }
}
