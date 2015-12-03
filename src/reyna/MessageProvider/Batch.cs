namespace Reyna
{
    using System.Collections.Generic;
    using System.Text;

    internal class Batch
    {
        public Batch()
        {
            this.Events = new List<BatchMessage>();
        }

        internal List<BatchMessage> Events { get; set; }

        public void Add(BatchMessage message)
        {
            this.Events.Add(message);
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
