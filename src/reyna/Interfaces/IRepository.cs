namespace Reyna.Interfaces
{
    using System;

    internal interface IRepository
    {
        event EventHandler<EventArgs> MessageAdded;

        void Initialise();

        void Add(IMessage message);

        IMessage Get();

        IMessage Remove();
    }
}
