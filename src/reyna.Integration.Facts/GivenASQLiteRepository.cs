namespace Reyna.Integration.Facts
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Reflection;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenASQLiteRepository
    {
        public GivenASQLiteRepository()
        {
            File.Delete(this.DatabasePath);
            this.Repository = new SQLiteRepository();
            this.Repository.Initialise();
        }

        private SQLiteRepository Repository { get; set; }

        private string DatabasePath 
        {
            get 
            {
                var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(assemblyFile.DirectoryName, "reyna.db");
            }
        }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.Repository);
        }

        [Fact]
        public void WhenCallingExistsAndDatabaseDoesNotExistShouldReturnFalse()
        {
            File.Delete(this.DatabasePath);

            Assert.False(this.Repository.Exists);
        }

        [Fact]
        public void WhenCallingExistsAndDatabaseFileExistsButNoCompleteShouldReturnFalse()
        {
            File.WriteAllBytes(this.DatabasePath, new byte[] { });
            
            Assert.False(this.Repository.Exists);
        }

        [Fact]
        public void WhenCallingCreateAndFileExistsButZeroBytesShouldCreate()
        {
            File.WriteAllBytes(this.DatabasePath, new byte[] { });

            this.Repository.Create();
            Assert.True(this.Repository.Exists);
        }

        [Fact]
        public void WhenCallingCreateShouldCreateDatabaseFile()
        {
            this.Repository.Create();

            Assert.True(File.Exists(this.DatabasePath));
            Assert.True(this.Repository.Exists);
        }

        [Fact]
        public void WhenCurrentDirectoryIsNotApplicationDirectoryThenCallingCreateShouldCreateDatabaseFileInApplicationDirectory()
        {
            var cd = Environment.CurrentDirectory;
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempPath);
                Environment.CurrentDirectory = tempPath;
                this.Repository.Create();

                Assert.False(File.Exists(Path.Combine(tempPath, "reyna.db")));
                Assert.True(File.Exists(this.DatabasePath));
            }
            finally
            {
                Environment.CurrentDirectory = cd;
                Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void WhenCallingCreateShouldCreateDatabaseFileAndTables()
        {
            this.Repository.Create();
            
            int nonSystemtTablesCount = this.ExecuteScalar("SELECT count(1) FROM sqlite_master WHERE type='table' AND name not like 'sqlite?_%' escape '?'");
            int mesageTableCount = this.ExecuteScalar("SELECT count(1) FROM sqlite_master WHERE type='table' AND name = 'Message'");
            int headerTableCount = this.ExecuteScalar("SELECT count(1) FROM sqlite_master WHERE type='table' AND name = 'Header'");

            Assert.True(File.Exists(this.DatabasePath));
            Assert.Equal(2, nonSystemtTablesCount);
            Assert.Equal(1, mesageTableCount);
            Assert.Equal(1, headerTableCount);
        }

        [Fact]
        public void WhenCallingAddShouldReturnSucceed()
        {
            var message = this.GetMessage("http://HOST.com:9080/home", "{\"body\": body}");

            this.Repository.Create();
            this.Repository.Add(message);

            int mesageRowsCount = this.ExecuteScalar("SELECT COUNT(1) FROM Message");
            int headerRowsCount = this.ExecuteScalar("SELECT COUNT(1) FROM Header");
            Assert.Equal(1, mesageRowsCount);
            Assert.Equal(2, headerRowsCount);

            var storedMessage = this.GetMessages()[0];
            Assert.Equal(new Uri("http://HOST.com:9080/home"), storedMessage.Url);
            Assert.Equal("{\"body\": body}", storedMessage.Body);
            
            Assert.Equal(2, storedMessage.Headers.Count);
            Assert.Equal("VALUE", storedMessage.Headers["PARAM"]);
            Assert.Equal("application/josn", storedMessage.Headers["Content_Type"]);
        }

        [Fact]
        public void WhenCallingAddWithLimitReturnSucceed()
        {
            var message = this.GetMessage("http://HOST.com:9080/home", "{\"body\": body}");

            this.Repository.Create();
            this.Repository.Add(message, 6000);

            int mesageRowsCount = this.ExecuteScalar("SELECT COUNT(1) FROM Message");
            int headerRowsCount = this.ExecuteScalar("SELECT COUNT(1) FROM Header");
            Assert.Equal(1, mesageRowsCount);
            Assert.Equal(2, headerRowsCount);

            var storedMessage = this.GetMessages()[0];
            Assert.Equal(new Uri("http://HOST.com:9080/home"), storedMessage.Url);
            Assert.Equal("{\"body\": body}", storedMessage.Body);

            Assert.Equal(2, storedMessage.Headers.Count);
            Assert.Equal("VALUE", storedMessage.Headers["PARAM"]);
            Assert.Equal("application/josn", storedMessage.Headers["Content_Type"]);
        }

        [Fact]
        public void WhenCallingAddWithLimitAndLimitIsLowerThanTheActualSizeShouldDeleteTheOldestMessageWithSameTypeReturnSucceed()
        {
            var message = this.GetMessage("http://HOST.com:9080/home", "{\"body\": body}");

            this.Repository.Create();
            this.Repository.Add(message);
            this.Repository.Add(message, 307200);

            int mesageRowsCount = this.ExecuteScalar("SELECT COUNT(*) FROM Message");
            int headerRowsCount = this.ExecuteScalar("SELECT COUNT(*) FROM Header");
            Assert.Equal(1, mesageRowsCount);
            Assert.Equal(2, headerRowsCount);

            var storedMessage = this.GetMessages()[0];
            Assert.Equal(new Uri("http://HOST.com:9080/home"), storedMessage.Url);
            Assert.Equal("{\"body\": body}", storedMessage.Body);

            Assert.Equal(2, storedMessage.Headers.Count);
            Assert.Equal("VALUE", storedMessage.Headers["PARAM"]);
            Assert.Equal("application/josn", storedMessage.Headers["Content_Type"]);
        }

        [Fact]
        public void WhenCallingShrinkDbShouldReduceStorageByRemovingOldestMessages()
        {
            var message = this.GetMessage("http://HOST.com:9080/home", "{\"body\": body}");
            this.Repository.Create();

            for (int i = 0; i < 100; i++)
            {
                this.Repository.Add(message);
            }

            this.Repository.SizeDifferenceToStartCleaning = 1024;
            this.Repository.ShrinkDb(5120);
            int result = this.ExecuteScalar("SELECT id FROM Message LIMIT 1");

            Assert.Equal(result, 88);
        }

        [Fact]
        public void WhenCallingGetShouldReturnExpectedMessage()
        {
            var message1 = this.GetMessage("http://HOST.com:9080/home1", "{\"body\": body}");
            var message2 = this.GetMessage("http://HOST.com:9080/home2", "body");
            var message3 = this.GetMessage("http://HOST.com:9080/home3", string.Empty);

            this.Repository.Create();
            this.Repository.Add(message1);
            this.Repository.Add(message2);
            this.Repository.Add(message3);

            var message = this.Repository.Get();

            Assert.Equal(new Uri("http://HOST.com:9080/home1"), message.Url);
            Assert.Equal("{\"body\": body}", message.Body);
            Assert.Equal("VALUE", message.Headers["PARAM"]);
            Assert.Equal("application/josn", message.Headers["Content_Type"]);
            
            Assert.Equal("1", message.Headers["reyna-id"]);
        }

        [Fact]
        public void WhenCallingGetShouldReturnCorrectReynaId()
        {
            var message1 = this.GetMessage("http://HOST.com:9080/home1", "{\"body\": body}");
            var message2 = this.GetMessage("http://HOST.com:9080/home2", "body");

            this.Repository.Create();
            this.Repository.Add(message1);
            this.Repository.Add(message2);

            var testMessage1 = this.Repository.Get();
            this.Repository.Remove();
            var testMessage2 = this.Repository.Get();
            Assert.Equal("1", testMessage1.Headers["reyna-id"]);
            Assert.Equal("2", testMessage2.Headers["reyna-id"]);
        }

        [Fact]
        public void WhenCallingDeQueueShouldRetunExectedMessage()
        {
            var message1 = this.GetMessage("http://HOST.com:9080/home1", "{\"body\": body}");
            var message2 = this.GetMessage("http://HOST.com:9080/home2", "body");
            var message3 = this.GetMessage("http://HOST.com:9080/home3", string.Empty);

            this.Repository.Create();
            this.Repository.Add(message1);
            this.Repository.Add(message2);
            this.Repository.Add(message3);

            var actualMessage1 = this.Repository.Remove();
            var actualMessage2 = this.Repository.Remove();
            var actualMessage3 = this.Repository.Remove();
            var actualMessage4 = this.Repository.Remove();

            Assert.Equal(new Uri("http://HOST.com:9080/home1"), actualMessage1.Url);
            Assert.Equal("{\"body\": body}", actualMessage1.Body);
            Assert.Equal("VALUE", actualMessage1.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage1.Headers["Content_Type"]);

            Assert.Equal(new Uri("http://HOST.com:9080/home2"), actualMessage2.Url);
            Assert.Equal("body", actualMessage2.Body);
            Assert.Equal("VALUE", actualMessage2.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage2.Headers["Content_Type"]);

            Assert.Equal(new Uri("http://HOST.com:9080/home3"), actualMessage3.Url);
            Assert.Equal(string.Empty, actualMessage3.Body);
            Assert.Equal("VALUE", actualMessage3.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage3.Headers["Content_Type"]);

            Assert.Null(actualMessage4);
        }

        [Fact]
        public void WhenCallingGetNextMessageAfterShouldRetunExectedMessage()
        {
            var message1 = this.GetMessage("http://HOST.com:9080/home1", "{\"body\": body}");
            var message2 = this.GetMessage("http://HOST.com:9080/home2", "body");
            var message3 = this.GetMessage("http://HOST.com:9080/home3", string.Empty);

            this.Repository.Create();
            this.Repository.Add(message1);
            this.Repository.Add(message2);
            this.Repository.Add(message3);

            var actualMessage1 = this.Repository.GetNextMessageAfter(0);
            var actualMessage2 = this.Repository.GetNextMessageAfter(1);
            var actualMessage3 = this.Repository.GetNextMessageAfter(2);
            var actualMessage4 = this.Repository.GetNextMessageAfter(3);

            Assert.Equal(new Uri("http://HOST.com:9080/home1"), actualMessage1.Url);
            Assert.Equal("{\"body\": body}", actualMessage1.Body);
            Assert.Equal("VALUE", actualMessage1.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage1.Headers["Content_Type"]);

            Assert.Equal(new Uri("http://HOST.com:9080/home2"), actualMessage2.Url);
            Assert.Equal("body", actualMessage2.Body);
            Assert.Equal("VALUE", actualMessage2.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage2.Headers["Content_Type"]);

            Assert.Equal(new Uri("http://HOST.com:9080/home3"), actualMessage3.Url);
            Assert.Equal(string.Empty, actualMessage3.Body);
            Assert.Equal("VALUE", actualMessage3.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage3.Headers["Content_Type"]);

            Assert.Null(actualMessage4);
        }

        [Fact]
        public void WhenCallingGetNextMessageAfterACertainIdShouldRetunExectedMessage()
        {
            var message1 = this.GetMessage("http://HOST.com:9080/home1", "{\"body\": body}");
            var message2 = this.GetMessage("http://HOST.com:9080/home2", "body");
            var message3 = this.GetMessage("http://HOST.com:9080/home3", string.Empty);

            this.Repository.Create();
            this.Repository.Add(message1);
            this.Repository.Add(message2);
            this.Repository.Add(message3);

            var actualMessage3 = this.Repository.GetNextMessageAfter(2);
            
            Assert.Equal(new Uri("http://HOST.com:9080/home3"), actualMessage3.Url);
            Assert.Equal(string.Empty, actualMessage3.Body);
            Assert.Equal("VALUE", actualMessage3.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage3.Headers["Content_Type"]);
        }

        [Fact]
        public void WhenCallingDeleteShouldDeleteThisMessageOnly()
        {
            var message1 = this.GetMessage("http://HOST.com:9080/home1", "{\"body\": body}");
            var message2 = this.GetMessage("http://HOST.com:9080/home2", "body");
            var message3 = this.GetMessage("http://HOST.com:9080/home3", string.Empty);

            this.Repository.Create();
            this.Repository.Add(message1);
            this.Repository.Add(message2);
            this.Repository.Add(message3);

            var actualMessage2 = this.Repository.GetNextMessageAfter(1);

            Assert.Equal(new Uri("http://HOST.com:9080/home2"), actualMessage2.Url);
            Assert.Equal("body", actualMessage2.Body);
            Assert.Equal("VALUE", actualMessage2.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage2.Headers["Content_Type"]);

            this.Repository.Delete(actualMessage2);

            actualMessage2 = this.Repository.GetNextMessageAfter(1);

            Assert.Equal(new Uri("http://HOST.com:9080/home3"), actualMessage2.Url);
            Assert.Equal(string.Empty, actualMessage2.Body);
            Assert.Equal("VALUE", actualMessage2.Headers["PARAM"]);
            Assert.Equal("application/josn", actualMessage2.Headers["Content_Type"]);

            this.Repository.Delete(actualMessage2);
            actualMessage2 = this.Repository.GetNextMessageAfter(1);
            Assert.Null(actualMessage2);
        }

        [Fact]
        public void WhenSubscribedToMessageAddedEventShouldReceiveEventWhenCallingAdd()
        {
            var received = false;
            this.Repository.MessageAdded += (sender, e) => { received = true; };
            this.Repository.Add(new Message(null, null));
            System.Threading.Thread.Sleep(100);

            Assert.True(received);
        }

        private IMessage GetMessage(string url, string body)
        {
            var message = new Message(new Uri(url), body);
            message.Headers.Add("Content_Type", "application/josn");
            message.Headers.Add("PARAM", "VALUE");
            
            return message;
        }
        
        private int ExecuteScalar(string sql)
        {
            var connectionString = string.Format("Data Source={0}", this.DatabasePath);
            using (var connection = new SQLiteConnection(connectionString))
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

            var connectionString = string.Format("Data Source={0}", this.DatabasePath);
            using (var connection = new SQLiteConnection(connectionString))
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
