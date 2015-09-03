namespace Reyna
{
    public interface IConnectionInfo
    {
        bool Connected { get; }

        bool Mobile { get; }

        bool Wifi { get; }

        bool Roaming { get; }
    }
}
