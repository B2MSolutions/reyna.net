namespace Reyna
{
    using System;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using Reyna.Interfaces;

    internal class SQLiteRepository : IRepository
    {
        private const string CreateTableSql = "CREATE TABLE Message (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT, body TEXT);CREATE TABLE Header (id INTEGER PRIMARY KEY AUTOINCREMENT, messageid INTEGER, key TEXT, value TEXT, FOREIGN KEY(messageid) REFERENCES message(id));";
        private const string InsertMessageSql = "INSERT INTO Message(url, body) VALUES(@url, @body); SELECT last_insert_rowid();";
        private const string InsertHeaderSql = "INSERT INTO Header(messageid, key, value) VALUES(@messageId, @key, @value);";
        private const string DeleteMessageSql = "DELETE FROM Header WHERE messageid = @messageId;DELETE FROM Message WHERE id = @messageId";
        private const string SelectTop1MessageSql = "SELECT id, url, body FROM Message ORDER BY id ASC LIMIT 1";
        private const string SelectHeaderSql = "SELECT key, value FROM Header WHERE messageid = @messageId";

        public delegate IMessage ExecuteFunctionInTransaction(DbTransaction transaction);

        public delegate void ExecuteActionInTransaction(IMessage message, DbTransaction transaction);

        public bool DoesNotExist
        {
            get
            {
                return !File.Exists(this.DatabasePath);
            }
        }

        private string DatabasePath
        {
            get
            {
                var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
                return Path.Combine(assemblyFile.DirectoryName, "reyna.db");
            }
        }

        public void Create()
        {
            SQLiteConnection.CreateFile(this.DatabasePath);

            this.ExecuteInTransaction((t) =>
                {
                    var sql = SQLiteRepository.CreateTableSql;
                    this.ExecuteNonQuery(sql, t);
                });
        }

        public IMessage Enqueue(IMessage message)
        {
            return this.ExecuteInTransaction((t) =>
                {
                    var sql = SQLiteRepository.InsertMessageSql;
                    var url = this.CreateParameter("@url", message.Url);
                    var body = this.CreateParameter("@body", message.Body);
                    var id = Convert.ToInt32(this.ExecuteScalar(sql, t, url, body));

                    sql = SQLiteRepository.InsertHeaderSql;
                    var messageId = this.CreateParameter("@messageId", id);
                    foreach (string headerKey in message.Headers.Keys)
                    {
                        var key = this.CreateParameter("@key", headerKey);
                        var value = this.CreateParameter("@value", message.Headers[headerKey]);
                        this.ExecuteScalar(sql, t, messageId, key, value);
                    }

                    return this.AssignIdTo(message, id);
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
                    var sql = SQLiteRepository.DeleteMessageSql;
                    var messageId = this.CreateParameter("@messageId", message.Id);
                    this.ExecuteNonQuery(sql, t, messageId);
                });
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

        private DbConnection CreateConnection()
        {
            var connectionString = string.Format("Data Source={0}", this.DatabasePath);
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        private DbParameter CreateParameter(string parameterName, object value)
        {
            return new SQLiteParameter(parameterName, value);
        }

        private DbCommand CreateCommand(string sql, DbTransaction transaction, params DbParameter[] parameters)
        {
            var command = transaction.Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            command.Connection = transaction.Connection;

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            return command;
        }

        private int ExecuteNonQuery(string sql, DbTransaction transaction, params DbParameter[] parameters)
        {
            using (var command = this.CreateCommand(sql, transaction, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        private object ExecuteScalar(string sql, DbTransaction transaction, params DbParameter[] parameters)
        {
            using (var command = this.CreateCommand(sql, transaction, parameters))
            {
                return command.ExecuteScalar();
            }
        }

        private DbDataReader ExecuteReader(string sql, DbTransaction transaction, params DbParameter[] parameters)
        {
            using (var command = this.CreateCommand(sql, transaction, parameters))
            {
                return command.ExecuteReader();
            }
        }

        private IMessage ExecuteInTransaction(ExecuteFunctionInTransaction func)
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

        private IMessage GetFirstInQueue(params ExecuteActionInTransaction[] postActions)
        {
            return this.ExecuteInTransaction((t) =>
            {
                IMessage message = null;

                var sql = SQLiteRepository.SelectTop1MessageSql;
                using (var reader = this.ExecuteReader(sql, t))
                {
                    reader.Read();
                    message = this.CreateFromDataReader(reader);
                }

                if (message == null)
                {
                    return null;
                }

                sql = SQLiteRepository.SelectHeaderSql;
                var messageId = this.CreateParameter("@messageId", message.Id);
                using (var reader = this.ExecuteReader(sql, t, messageId))
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
