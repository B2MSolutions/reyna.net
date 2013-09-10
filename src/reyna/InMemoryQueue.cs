namespace Reyna
{
    using System;
    using System.Collections.Generic;
    using Reyna.Interfaces;

    public class InMemoryQueue : IMessageStore
    {
        private object padlock;

        private Queue<IMessage> queue;

        public InMemoryQueue()
        {
            this.padlock = new object();
            this.queue = new Queue<IMessage>();
        }

        public event EventHandler<EventArgs> MessageAdded;

        public void Add(IMessage message)
        {
            lock (this.padlock)
            {
                this.queue.Enqueue(message);
                this.FireMessageAdded();
            }
        }

        public IMessage Get()
        {
            lock (this.padlock)
            {
                return this.queue.Peek();
            }
        }

        public IMessage Remove()
        {
            lock (this.padlock)
            {
                if (this.queue.Count == 0)
                {
                    return null;
                }

                return this.queue.Dequeue();
            }
        }

        private void FireMessageAdded()
        {
            if (this.MessageAdded == null)
            {
                return;
            }
               
            this.MessageAdded.Invoke(this, new EventArgs());
        }
    }
}
