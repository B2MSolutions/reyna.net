namespace reyna
{
    using reyna.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Text;

    public class SQLiteRepository : IRepository
    {
        private const string TableCreationSql = "CREATE TABLE Message (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT, body TEXT);CREATE TABLE Header (id INTEGER PRIMARY KEY AUTOINCREMENT, messageid INTEGER, key TEXT, value TEXT, FOREIGN KEY(messageid) REFERENCES message(id));";

        public bool DoesNotExist(string name)
        {
            return !File.Exists(name);
        }

        public void Create(string name)
        {
            SQLiteConnection.CreateFile(name);

            this.ExecuteNonQuery(SQLiteRepository.TableCreationSql);
        }

        public IMessage Insert(IMessage message)
        {
            var messageInsertSql = "INSERT INTO Message(url, body) VALUES('{0}', '{1}'); SELECT last_insert_rowid();";
            var headerInsertSql = "INSERT INTO Header(messageid, key, value) VALUES({0}, '{1}', '{2}');";

            using (var connection = new SQLiteConnection("Data Source=reyna.db"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var messageId = 0;

                    using (var command = new SQLiteCommand(string.Format(messageInsertSql, message.Url, message.Body), connection, transaction))
                    {
                        messageId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    var builder = new StringBuilder();
                    foreach (string key in message.Headers)
                    {
                        builder.Append(string.Format(headerInsertSql, messageId, key, message.Headers[key]));
                    }

                    using (var command = new SQLiteCommand(builder.ToString(), connection, transaction))
                    {
                        command.ExecuteNonQuery();
                    }

                    var result = message.Clone() as Message;
                    result.Id = messageId;

                    transaction.Commit();
                    return result;
                }
            }

        }

        private int ExecuteNonQuery(string sql)
        {
            using (var connection = new SQLiteConnection("Data Source=reyna.db"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        private object ExecuteScalar(string sql)
        {
            using (var connection = new SQLiteConnection("Data Source=reyna.db"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    return command.ExecuteScalar();
                }
            }
        }
    }
}
