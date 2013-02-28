namespace reyna
{
    using System;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using reyna.Interfaces;

    public class SQLiteRepository : IRepository
    {
        public bool DoesNotExist
        {
            get
            {
                return !File.Exists(this.DatabasePath);
            }
        }

        public void Create()
        {
            SQLiteConnection.CreateFile(this.DatabasePath);

            this.ExecuteInTransaction((t) =>
                {
                    this.ExecuteNonQuery("CREATE TABLE Message (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT, body TEXT);CREATE TABLE Header (id INTEGER PRIMARY KEY AUTOINCREMENT, messageid INTEGER, key TEXT, value TEXT, FOREIGN KEY(messageid) REFERENCES message(id));", t);
                });
        }

        public IMessage Enqueue(IMessage message)
        {
            return this.ExecuteInTransaction((t) =>
                {
                    var sql = this.CreateInsertSql(message);
                    var messageId = Convert.ToInt32(this.ExecuteScalar(this.CreateInsertSql(message), t));

                    sql = this.CreateInsertSql(messageId, message.Headers);
                    this.ExecuteScalar(sql, t);

                    return this.AssignIdTo(message, messageId);
                });
        }

        public IMessage Peek()
        {
            return this.GetFirstInQueue();
        }

        public IMessage Dequeue()
        {
            return this.GetFirstInQueue((message, t) =>
                {
                    var sql = this.CreateDeleteSql(message);
                    this.ExecuteNonQuery(sql, t);
                });
        }

        private string CreateSelectTop1Sql()
        {
            return "SELECT id, url, body FROM Message ORDER BY id ASC LIMIT 1";
        }

        private string CreateSelectHeaderSql(IMessage message)
        {
            return string.Format("SELECT key, value FROM Header WHERE messageid = {0}", message.Id);
        }

        private string CreateInsertSql(IMessage message)
        {
            return string.Format("INSERT INTO Message(url, body) VALUES('{0}', '{1}'); SELECT last_insert_rowid();", message.Url, message.Body);
        }

        private string CreateInsertSql(int messageId, WebHeaderCollection headers)
        {
            var builder = new StringBuilder();

            foreach (string key in headers)
            {
                builder.Append(string.Format("INSERT INTO Header(messageid, key, value) VALUES({0}, '{1}', '{2}');", messageId, key, headers[key]));
            }

            return builder.ToString();
        }

        private string CreateDeleteSql(IMessage message)
        {
            return string.Format("DELETE FROM Header WHERE messageid = {0};DELETE FROM Message WHERE id = {0}", message.Id);
        }

        private IMessage AssignIdTo(IMessage message, int id)
        {
            var clone = new Message(message.Url, message.Body)
            {
                Id = id
            };
            foreach (string key in message.Headers)
            {
                clone.Headers.Add(key, message.Headers[key]);
            }

            return clone;
        }

        private string DatabasePath
        {
            get
            {
                var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
                return Path.Combine(assemblyFile.DirectoryName, "reyna.db");
            }
        }

        private DbConnection CreateConnection()
        {
            var connectionString = string.Format("Data Source={0}", this.DatabasePath);
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        private DbCommand CreateCommand(string sql, DbTransaction transaction)
        {
            var command = transaction.Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            command.Connection = transaction.Connection;
            return command;
        }

        private int ExecuteNonQuery(string sql, DbTransaction transaction)
        {
            using (var command = this.CreateCommand(sql, transaction))
            {
                return command.ExecuteNonQuery();
            }
        }

        private object ExecuteScalar(string sql, DbTransaction transaction)
        {
            using (var command = this.CreateCommand(sql, transaction))
            {
                return command.ExecuteScalar();
            }
        }

        private DbDataReader ExecuteReader(string sql, DbTransaction transaction)
        {
            using (var command = this.CreateCommand(sql, transaction))
            {
                return command.ExecuteReader();
            }
        }

        private IMessage ExecuteInTransaction(Func<DbTransaction, IMessage> func)
        {
            using (var connection = this.CreateConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var message = func(transaction);
                transaction.Commit();
                return message;
            }
        }

        private void ExecuteInTransaction(Action<DbTransaction> action)
        {
            using (var connection = this.CreateConnection())
            using (var transaction = connection.BeginTransaction())
            {
                action(transaction);
                transaction.Commit();
            }
        }

        private IMessage GetFirstInQueue(params Action<IMessage, DbTransaction>[] postActions)
        {
            return this.ExecuteInTransaction((t) =>
            {
                IMessage message = null;

                var sql = this.CreateSelectTop1Sql();
                using (var reader = this.ExecuteReader(sql, t))
                {
                    reader.Read();
                    message = this.CreateFromDataReader(reader);
                }

                if (message == null)
                {
                    return null;
                }

                sql = this.CreateSelectHeaderSql(message);
                using (var reader = this.ExecuteReader(sql, t))
                {
                    while (reader.Read())
                    {
                        this.AddHeader(message, reader);
                    }
                }

                foreach (var postAction in postActions)
                {
                    postAction(message, t);
                }

                return message;
            });
        }

        private IMessage CreateFromDataReader(DbDataReader reader)
        {
            if (!reader.HasRows)
            {
                return null;
            }
            
            var id = Convert.ToInt32(reader["id"]);
            var url = new Uri(reader["url"] as string);
            var body = reader["body"] as string;

            var message = new Message(url, body)
            {
                Id = id
            };

            return message;
        }

        private void AddHeader(IMessage message, DbDataReader reader)
        {
            var key = reader["key"] as string;
            var value = reader["value"] as string;
            message.Headers.Add(key, value);
        }
    }
}
