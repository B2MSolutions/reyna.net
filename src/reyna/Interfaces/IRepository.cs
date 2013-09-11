namespace Reyna.Interfaces
{
    using System;

    public interface IRepository
    {
        bool DoesNotExist { get; }

        void Create();

        IMessage Enqueue(IMessage message);

        IMessage Peek();

        IMessage Dequeue();
    }
}
