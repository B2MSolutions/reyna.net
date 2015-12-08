namespace Reyna
{
    using System;
    using System.Text;

    internal class BatchMessage
    {
        public BatchMessage(int reynaId, Uri url, string payload)
        {
            this.ReynaId = reynaId;
            this.Url = url;
            this.Payload = payload;
        }

        public int ReynaId { get; set; }

        public Uri Url { get; set; }

        public string Payload { get; set; }

        public string ToJson()
        {
            var buffer = new StringBuilder();
            buffer.Append("{");
            buffer.Append("\"url\"");
            buffer.Append(":");
            buffer.Append("\"");
            buffer.Append(this.Url);
            buffer.Append("\", ");
            
            buffer.Append("\"reynaId\"");
            buffer.Append(":");
            buffer.Append(this.ReynaId);
            buffer.Append(", ");

            buffer.Append("\"payload\"");
            buffer.Append(":");
            buffer.Append(this.Payload);

            buffer.Append("}");

            return buffer.ToString();
        }
    }
}
