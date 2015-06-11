namespace Reyna.Interfaces
{
    using System;

    internal interface IRepository
    {
        event EventHandler<EventArgs> MessageAdded;

        void Initialise();

        void Add(IMessage message);

        void Add(IMessage message, long storageSizeLimit);

        IMessage Get();

        IMessage Remove();
    }
}
