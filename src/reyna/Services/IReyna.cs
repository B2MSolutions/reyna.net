namespace Reyna.Interfaces
{
    public interface IReyna : IService
    {
        void Put(IMessage message);
    }
}
