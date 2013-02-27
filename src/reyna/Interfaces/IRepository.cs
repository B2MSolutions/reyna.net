namespace reyna.Interfaces
{
    public interface IRepository
    {
        bool DoesExist(string name);

        void Create(string name);

        void Insert(IMessage message);
    }
}
