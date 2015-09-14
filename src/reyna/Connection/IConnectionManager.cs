namespace Reyna
{
    using Reyna.Interfaces;

    public interface IConnectionManager
    {
        Result CanSend { get; }
    }
}
