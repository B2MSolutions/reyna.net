namespace AcceptanceTests
{
    using System.IO;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reyna;

    [TestClass()]
    public class ReynaServiceTest
    {
        [TestMethod]
        public void ShouldEncryptDbIfPasswordIsPassedAndDbIsNotEncrypted()
        {
            var sqliteRepository = new SQLiteRepository();
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
    }
}
