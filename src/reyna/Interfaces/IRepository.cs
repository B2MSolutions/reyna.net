namespace Reyna.Interfaces
{
    using System;

    internal interface IRepository
    {
        event EventHandler<EventArgs> MessageEnqueued;

        bool DoesNotExist { get; }

        void Create();

        IMessage Enqueue(IMessage message);

        IMessage Peek();

        IMessage Dequeue();
    }
}
