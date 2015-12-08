namespace Reyna
{
    using System.Collections.Generic;
    using System.Text;
    using Reyna.Interfaces;

    internal class Batch
    {
        public Batch()
        {
            this.Events = new List<BatchMessage>();
        }

        internal List<BatchMessage> Events { get; set; }

        public void Add(IMessage message)
        {
            this.Events.Add(new BatchMessage(message.Id, message.Url, message.Body));
        }

        public void RemoveLastMessage()
        {
            if (this.Events.Count > 1)
            {
                this.Events.RemoveAt(this.Events.Count - 1);
            }
        }

        public string ToJson()
        {
            var buffer = new StringBuilder();
            buffer.Append("{");
            buffer.Append("\"events\"");
            buffer.Append(":[");

            if (this.Events.Count > 0)
            {
                foreach (var item in this.Events)
                {
                    buffer.Append(item.ToJson());
                    buffer.Append(", ");
                }

                buffer = buffer.Remove(buffer.Length - 2, 2);
            }

            buffer.Append("]}");
            return buffer.ToString();
        }
    }
}
