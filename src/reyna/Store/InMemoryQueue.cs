namespace Reyna
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Reyna.Interfaces;

    internal sealed class InMemoryQueue : IRepository
    {
        private Queue<IMessage> queue;

        public InMemoryQueue()
        {
            this.queue = new Queue<IMessage>();
        }

        public event EventHandler<EventArgs> MessageAdded;

        public long AvailableMessagesCount 
        {
            get
            {
                return this.queue.Count;
            }
        }

        private object SyncRoot
        {
            get
            {
                return ((ICollection)this.queue).SyncRoot;
            }
        }

        public void Initialise()
        {
        }

        public void Add(IMessage message)
        {
            lock (this.SyncRoot)
            {
                this.queue.Enqueue(message);
                this.FireMessageAdded();
            }
        }

        public void Add(IMessage message, long storageSizeLimit)
        {
            this.Add(message);
        }

        public IMessage Get()
        {
            lock (this.SyncRoot)
            {
                if (this.queue.Count == 0)
                {
                    return null;
                }

                return this.queue.Peek();
            }
        }

        public IMessage Remove()
        {
            lock (this.SyncRoot)
            {
                if (this.queue.Count == 0)
                {
                    return null;
                }

                return this.queue.Dequeue();
            }
        }

        public IMessage GetNextMessageAfter(long messageId)
        {
            throw new NotImplementedException();
        }

        public void Delete(IMessage message)
        {
            throw new NotImplementedException();
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
