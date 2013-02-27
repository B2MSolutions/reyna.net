namespace reyna.Interfaces
{
    public interface IRepository
    {
        bool DoesNotExist(string name);

        void Create(string name);

        IMessage Enqueue(IMessage message);

        IMessage Peek();

        IMessage Dequeue();
    }
}
