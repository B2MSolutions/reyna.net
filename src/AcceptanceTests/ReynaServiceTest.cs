namespace AcceptanceTests
{
    using System.IO;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reyna;
    using System;
    using Reyna.Interfaces;

    [TestClass()]
    public class ReynaServiceTest
    {
        [TestMethod]
        public void ShouldEncryptDbIfPasswordIsPassedAndDbIsNotEncrypted()
        {
            var sqliteRepository = new SQLiteRepository(new byte[] { 0x33, 0xFF, 0xAB });
            sqliteRepository.Create();

            var reynaService = new ReynaService(new byte[] { 0x33, 0xFF, 0xAB }, null);
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

        [TestMethod]
        [DeploymentItem("test.xml")]
        public void ShouldKeepSizeTheSame()
        {
            long size = 2 * 1024 * 1024; // 2 mega
            var password = new byte[] { 0x33, 0xFF, 0xAB };

            ReynaService.ResetStorageSizeLimit();
            var sqliteRepository = new SQLiteRepository(password);
            sqliteRepository.Create();
            
            var reynaService = new ReynaService(password, null);
            reynaService.Start();

            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
            var path = Path.Combine(assemblyFile.DirectoryName, "reyna.db");

            putMessageFromFile(reynaService, assemblyFile.DirectoryName, 100);
            
            FileInfo fileInfo = new FileInfo(path);
            Assert.AreEqual(5649408, fileInfo.Length);

            ReynaService.SetStorageSizeLimit(password, size);
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
            var sqliteRepository = new SQLiteRepository(password);
            sqliteRepository.Create();

            var reynaService = new ReynaService(password, null);
            reynaService.Start();

            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
            var path = Path.Combine(assemblyFile.DirectoryName, "reyna.db");

            ReynaService.SetStorageSizeLimit(password, size);
            
            putMessageFromFile(reynaService, assemblyFile.DirectoryName, 100);
            reynaService.Stop();
            var fileInfo = new FileInfo(path);
            Assert.AreEqual(1812480, fileInfo.Length);
            File.Delete(path);
        }

        private void putMessageFromFile(IReyna reynaStore, string path, int numberOfMessages)
        {
            var body =  Read(Path.Combine(path, "test.xml"));
            for (var index = 0; index < numberOfMessages; index++ )
            {
                var message = new Message(new Uri("https://elemez.com"), body);
                reynaStore.Put(message);
            }

            while (((ReynaService)reynaStore).VolatileStore.Get() != null)
            {
                System.Threading.Thread.Sleep(3 * 1000);
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
