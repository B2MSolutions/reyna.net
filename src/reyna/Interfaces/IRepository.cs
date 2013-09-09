namespace Reyna.Interfaces
{
    internal interface IRepository
    {
        bool DoesNotExist { get; }

        void Create();

        IMessage Enqueue(IMessage message);

        IMessage Peek();

        IMessage Dequeue();
    }
}
