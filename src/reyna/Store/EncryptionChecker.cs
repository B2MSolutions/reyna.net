namespace Reyna
{
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Interfaces;

    public class EncryptionChecker : IEncryptionChecker
    {
        private string DatabasePath
        {
            get
            {
                var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
                return Path.Combine(assemblyFile.DirectoryName, "reyna.db");
            }
        }

        public bool DbEncrypted()
        {
            var bytes = new byte[16];
            using (var file = File.OpenRead(this.DatabasePath))
            {
                file.Read(bytes, 0, bytes.Length);
            }

            var stringResult = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return stringResult.IndexOf("SQLite format 3\0") == -1;
        }

        public void EncryptDb(byte[] password)
        {
            var connectionString = string.Format("Data Source={0}", this.DatabasePath);
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            connection.ChangePassword(password);
            connection.Close();
        }
    }
}
