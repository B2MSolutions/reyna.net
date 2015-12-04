namespace Reyna
{
    internal interface IPeriodicBackoutCheck
    {
        void Record(string task);

        bool IsTimeElapsed(string task, long periodInMilliseconds);
    }
}
