namespace AcceptanceTests
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using System.Reflection;
    using Reyna;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class SQLiteRepositoryTest
    {
        [TestInitialize()]
        public void Setup()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(SQLiteException))]
        public void IfDatabaseIsEncryptedButConnectionMissingPasswordShouldThrowException()
        {
            var sqliteRepository = new SQLiteRepository(new byte[] { 0xFF, 0xEE, 0xDD, 0x10, 0x20, 0x30 });
            sqliteRepository.Create();

            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
            var path = Path.Combine(assemblyFile.DirectoryName, "reyna.db");

            this.ExecuteScalar("SELECT count(1) FROM sqlite_master WHERE type='table' AND name not like 'sqlite?_%' escape '?'", path);

            File.Delete(path);
        }

        private int ExecuteScalar(string sql, string pathToReyna)
        {
            var connectionString = string.Format("Data Source={0}", pathToReyna);
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
    }
}
