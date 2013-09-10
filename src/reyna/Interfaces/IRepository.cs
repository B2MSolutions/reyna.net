namespace Reyna.Interfaces
{
    using System;

    internal interface IRepository
    {
        bool DoesNotExist { get; }

        void Create();

        IMessage Enqueue(IMessage message);

        IMessage Peek();

        IMessage Dequeue();
    }
}
