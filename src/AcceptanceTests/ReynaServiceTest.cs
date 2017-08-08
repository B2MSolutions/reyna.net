namespace AcceptanceTests
{
    using System.IO;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reyna;
    using System;
    using Reyna.Interfaces;
    using System.Threading;

    [TestClass()]
    public class ReynaServiceTest
    {
        private SQLiteRepository Repository { get; set; }

        [TestMethod]
        public void ShouldEncryptDbIfPasswordIsPassedAndDbIsNotEncrypted()
        {
            var sqliteRepository = new SQLiteRepository(new ReynaNullLogger(), new byte[] { 0x33, 0xFF, 0xAB });
            sqliteRepository.Create();

            var reynaService = new ReynaService(new byte[] { 0x33, 0xFF, 0xAB }, null, new ReynaNullLogger());
            reynaService.Start();

            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
            var path = Path.Combine(assemblyFile.DirectoryName, "reyna.db");

            var bytes = new byte[16];
            using (var file = File.OpenRead(path))
            {
                file.Read(bytes, 0, bytes.Length);
            }

            var stringResult = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Assert.AreNotEqual("SQLite format 3\0", stringResult);

            reynaService.Stop();
            File.Delete(path);
        }

        [TestMethod(), Timeout(60 * 1000)]
        [DeploymentItem("test.xml")]
        public void ShouldKeepSizeTheSame()
        {
            long size = 2 * 1024 * 1024; // 2 mega
            var password = new byte[] { 0x33, 0xFF, 0xAB };

            ReynaService.ResetStorageSizeLimit();
            var sqliteRepository = new SQLiteRepository(new ReynaNullLogger(), password);
            sqliteRepository.Create();

            var reynaService = new ReynaService(password, null, new ReynaNullLogger());
            reynaService.Start();

            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
            var path = Path.Combine(assemblyFile.DirectoryName, "reyna.db");

            putMessageFromFile(reynaService, assemblyFile.DirectoryName, 100);
            
            FileInfo fileInfo = new FileInfo(path);
            Assert.AreEqual(5649408, fileInfo.Length);

            reynaService.SetStorageSizeLimit(password, size);
            reynaService.Stop();
            fileInfo = new FileInfo(path);
            Assert.AreEqual(1754112, fileInfo.Length);
            File.Delete(path);
        }

        [TestMethod]
        [DeploymentItem("test.xml")]
        public void ShouldKeepSizeTheSameWhenAddingNewMessage()
        {
            long size = 2 * 1024 * 1024; // 2 mega
            var password = new byte[] { 0x33, 0xFF, 0xAB };

            ReynaService.ResetStorageSizeLimit();
            var sqliteRepository = new SQLiteRepository(new ReynaNullLogger(), password);
            sqliteRepository.Create();

            var reynaService = new ReynaService(password, null, new ReynaNullLogger());
            reynaService.Start();

            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
            var path = Path.Combine(assemblyFile.DirectoryName, "reyna.db");

            reynaService.SetStorageSizeLimit(password, size);
            
            putMessageFromFile(reynaService, assemblyFile.DirectoryName, 100);
            reynaService.Stop();
            var fileInfo = new FileInfo(path);
            Assert.AreEqual(1812480, fileInfo.Length);
            File.Delete(path);
        }

        [TestMethod]
        public void WhenAddingAndRemovingFromDifferentThreadShouldNotThrow()
        {
            var password = new byte[] { 0x33, 0xFF, 0xAB };

            ReynaService.ResetStorageSizeLimit();
            this.Repository = new SQLiteRepository(new ReynaNullLogger(), password);
            this.Repository.Create();

            Thread injectingThread = new Thread(this.AddMessage);
            Thread removingThread = new Thread(this.RemoveMessage);
            Thread readThread = new Thread(this.ReadMessages);
            Thread deleteThread = new Thread(this.DeleteMessages);

            injectingThread.Start();
            removingThread.Start();
            readThread.Start();
            deleteThread.Start();

            injectingThread.Join();
            removingThread.Join();
            readThread.Join();
            deleteThread.Join();

            Assert.AreEqual(0, this.Repository.AvailableMessagesCount);
        }

        private void AddMessage()
        {
            for (int i = 0; i < 1000; i++)
            {
                var message = this.GetMessage("http://HOST.com:9080/home1", "{\"body\": body}");
                this.Repository.Add(message);
            }
        }

        private void RemoveMessage()
        {
            Thread.Sleep(2000);

            while (this.Repository.Get() != null)
            {
                this.Repository.Remove();
            }
        }

        private void DeleteMessages()
        {
            Thread.Sleep(2000);

            for (int i = 0; i < 100; i++)
            {
                var message = this.Repository.GetNextMessageAfter(10);
                this.Repository.DeleteMessagesFrom(message);
            }
        }

        private void ReadMessages()
        {
            for (int i = 0; i < 100; i++)
            {
                this.Repository.GetNextMessageAfter(10);
                Thread.Sleep(10);
            }
        }

        private IMessage GetMessage(string url, string body)
        {
            var message = new Message(new Uri(url), body);
            message.Headers.Add("Content_Type", "application/josn");
            message.Headers.Add("PARAM", "VALUE");

            return message;
        }

        private void putMessageFromFile(IReyna reynaStore, string path, int numberOfMessages)
        {
            var body =  Read(Path.Combine(path, "test.xml"));
            for (var index = 0; index < numberOfMessages; index++ )
            {
                var message = new Message(new Uri("https://elemez.com"), body);
                reynaStore.Put(message);
            }
        }

        private static string Read(string fileName)
        {
            using (var stream = System.IO.File.OpenRead(fileName))
            {
                var bytes = new byte[stream.Length];
                int length = stream.Read(bytes, 0, (int)stream.Length);
                return System.Text.UTF8Encoding.UTF8.GetString(bytes, 0, length);
            }
        }
    }
}
