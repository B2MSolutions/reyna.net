namespace reyna.Integration.Facts
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using reyna.Interfaces;
    using Xunit;
using System.Collections.Generic;

    public class GivenASQLiteRepository
    {
        public GivenASQLiteRepository()
        {
            File.Delete("reyna.db");
            this.Repository = new SQLiteRepository();
        }

        private SQLiteRepository Repository { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.Repository);
        }

        [Fact]
        public void WhenCallingDoesNotExistAndDatabaseDoesNotExistShouldReturnTrue()
        {
            Assert.True(this.Repository.DoesNotExist("reyna.db"));
        }

        [Fact]
        public void WhenCallingDoesNotExistAndDatabaseExistsShouldReturnFalse()
        {
            File.WriteAllBytes("reyna.db", new byte[] { });
            
            Assert.False(this.Repository.DoesNotExist("reyna.db"));
        }

        [Fact]
        public void WhenCallingCreateShouldCreateDatabaseFile()
        {
            this.Repository.Create("reyna.db");

            Assert.True(File.Exists("reyna.db"));
        }

        [Fact]
        public void WhenCallingCreateShouldCreateDatabaseFileAndTables()
        {
            this.Repository.Create("reyna.db");
            
            int nonSystemtTablesCount = ExecuteScalar("SELECT count(1) FROM sqlite_master WHERE type='table' AND name not like 'sqlite?_%' escape '?'");
            int mesageTableCount = ExecuteScalar("SELECT count(1) FROM sqlite_master WHERE type='table' AND name = 'Message'");
            int headerTableCount = ExecuteScalar("SELECT count(1) FROM sqlite_master WHERE type='table' AND name = 'Header'");

            Assert.True(File.Exists("reyna.db"));
            Assert.Equal(2, nonSystemtTablesCount);
            Assert.Equal(1, mesageTableCount);
            Assert.Equal(1, headerTableCount);
        }

        [Fact]
        public void WhenCallingInsertShouldRetunExected()
        {
            var message = this.GetMessage();

            this.Repository.Create("reyna.db");
            this.Repository.Insert(message);

            int mesageRowsCount = ExecuteScalar("SELECT COUNT(1) FROM Message");
            int headerRowsCount = ExecuteScalar("SELECT COUNT(1) FROM Header");
            Assert.Equal(1, mesageRowsCount);
            Assert.Equal(2, headerRowsCount);

            var storedMessage = this.GetMessages()[0];
            Assert.Equal(new Uri("http://HOST.com:9080/home"), storedMessage.Url);
            Assert.Equal("{\"body\": body}", storedMessage.Body);
            Assert.Equal(2, storedMessage.Headers.Count);
            Assert.Equal("Token", storedMessage.Headers["Token"]);
            Assert.Equal("application/josn", storedMessage.Headers["Content_Type"]);
        }

        private IMessage GetMessage()
        {
            var message = new Message(new Uri("http://HOST.com:9080/home"), "{\"body\": body}");
            message.Headers.Add("Content_Type", "application/josn");
            message.Headers.Add("Token", "Token");
            
            return message;
        }
        
        private int ExecuteScalar(string sql)
        {
            using (var connection = new SQLiteConnection("Data Source=reyna.db"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        private IMessage[] GetMessages()
        {
            var messages = new List<IMessage>();

            using (var connection = new SQLiteConnection("Data Source=reyna.db"))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT * FROM Message", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToInt32(reader["id"]);
                        var url = new Uri(reader["url"] as string);
                        var body = reader["body"] as string;
                        var message = new Message(url, body) { Id = id };
                        messages.Add(message);
                    }
                }
                
                foreach (var message in messages)
                {
                    using (var command = new SQLiteCommand(string.Format("SELECT * FROM Header WHERE messageid = {0}", message.Id), connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var key = reader["key"] as string;
                            var value = reader["value"] as string;
                            message.Headers.Add(key, value);
                        }
                    }
                }
            }

            return messages.ToArray();
        }
    }
}
