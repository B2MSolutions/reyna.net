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

    internal sealed class SQLiteRepository : IRepository
    {
        private const string CreateTableSql = "CREATE TABLE Message (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT, body TEXT);CREATE TABLE Header (id INTEGER PRIMARY KEY AUTOINCREMENT, messageid INTEGER, key TEXT, value TEXT, FOREIGN KEY(messageid) REFERENCES message(id));";
        private const string InsertMessageSql = "INSERT INTO Message(url, body) VALUES(@url, @body); SELECT last_insert_rowid();";
        private const string InsertHeaderSql = "INSERT INTO Header(messageid, key, value) VALUES(@messageId, @key, @value);";
        private const string DeleteMessageSql = "DELETE FROM Header WHERE messageid = @messageId;DELETE FROM Message WHERE id = @messageId";
        private const string SelectTop1MessageSql = "SELECT id, url, body FROM Message ORDER BY id ASC LIMIT 1";
        private const string SelectHeaderSql = "SELECT key, value FROM Header WHERE messageid = @messageId";
        private const string SelectMinIdWithTypeSql = "SELECT min(id) FROM Message WHERE url = @type";
        private const string SelectNumberOfMessagesSql = "SELECT count(*) from Message";
        private const string SelectMessageIdWithOffsetSql = "SELECT id FROM Message LIMIT 1 OFFSET @offset";
        private const string DeleteMessagesToIdSql = "DELETE FROM Message WHERE id < @id";
        private const string DeleteHeadersToMessageIdSql = "DELETE FROM Header WHERE messageid < @id";

        private const int SizeDifferenceToStartCleaning = 307200; ////300Kb in bytes

        private static readonly object locker = new object();

        public SQLiteRepository()
        {
        }

        public SQLiteRepository(byte[] password)
        {
            this.Password = password;
        }

        private delegate IMessage ExecuteFunctionInTransaction(DbTransaction transaction);

        private delegate void ExecuteActionInTransaction(IMessage message, DbTransaction transaction);

        public event EventHandler<EventArgs> MessageAdded;

        internal bool Exists
        {
            get
            {
                return File.Exists(this.DatabasePath);
            }
        }

        internal byte[] Password { get; set; }

        private string DatabasePath
        {
            get
            {
                var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
                return Path.Combine(assemblyFile.DirectoryName, "reyna.db");
            }
        }

        public void Initialise()
        {
            if (this.Exists)
            {
                return;
            }
            
            this.Create();
        }

        public void Add(IMessage message)
        {
            this.ExecuteInTransaction((t) =>
                {
                    lock (locker)
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

                        if (this.MessageAdded != null)
                        {
                            this.MessageAdded.Invoke(this, EventArgs.Empty);
                        }
                    }
                });
        }

        public void Add(IMessage message, long storageSizeLimit)
        {
            lock (locker)
            {
                this.ExecuteInTransaction(t =>
                {
                    long dbSize = this.GetDbSize(t);

                    if (this.DbSizeApproachesLimit(dbSize, storageSizeLimit))
                    {
                        this.ClearOldRecords(t, message);
                    }

                    this.Add(message);
                });
            }
        }

        public IMessage Get()
        {
            return this.GetFirstInQueue();
        }

        public IMessage Remove()
        {
            return this.GetFirstInQueue((message, t) =>
            {
                var sql = SQLiteRepository.DeleteMessageSql;
                var messageId = this.CreateParameter("@messageId", message.Id);
                this.ExecuteNonQuery(sql, t, messageId);
            });
        }

        public void ShrinkDb(long limit)
        {
            lock (locker)
            {
                this.ExecuteInTransaction(t =>
                {
                    limit -= SizeDifferenceToStartCleaning;
                    long size = this.GetDbSize(t);

                    if (size <= limit)
                    {
                        return;
                    }

                    do
                    {
                        this.Shrink(t, limit, size);
                        size = this.GetDbSize(t);
                    }
                    while (size > limit);

                    this.Vacuum(t);
                });
            }
        }

        internal void Create()
        {
            SQLiteConnection.CreateFile(this.DatabasePath);

            this.ExecuteInTransaction((t) =>
            {
                var sql = SQLiteRepository.CreateTableSql;
                this.ExecuteNonQuery(sql, t);
            });
        }

        private long GetDbSize(DbTransaction transaction)
        {
            return (long)this.ExecuteScalar("pragma page_size", transaction) * (long)this.ExecuteScalar("pragma page_count", transaction);
        }

        private bool DbSizeApproachesLimit(long size, long limit)
        {
            return (limit > size) && (limit - size) < SizeDifferenceToStartCleaning;
        }

        private void ClearOldRecords(DbTransaction transaction, IMessage message)
        {
            long? oldestMessageId = this.FindOldestMessageIdWithType(transaction, message.Url);

            if (oldestMessageId.HasValue)
            {
                this.RemoveExistingMessage(transaction, oldestMessageId.Value);
            }
        }

        private void RemoveExistingMessage(DbTransaction transaction, long messageId)
        {
            var messageParameter = this.CreateParameter("@messageid", messageId);
            this.ExecuteNonQuery(SQLiteRepository.DeleteMessageSql, transaction, messageParameter);
        }

        private long? FindOldestMessageIdWithType(DbTransaction transaction, Uri type)
        {
            var typeParameter = this.CreateParameter("@type", type);
            return (long?)this.ExecuteScalar(SelectMinIdWithTypeSql, transaction, typeParameter);
        }

        private void Shrink(DbTransaction transaction, long sizeLimit, long size)
        {
            double limitPercentage = 1 - ((double)sizeLimit / size);
            long numberOfMessages = this.GetNumberOfMessages(transaction);
            long numberOfMessagesToRemove = (long)Math.Round(numberOfMessages * limitPercentage);
            numberOfMessagesToRemove = numberOfMessagesToRemove == 0 ? 1 : numberOfMessagesToRemove;

            long thresholdId = this.GetMessageIdToWhichShrink(transaction, numberOfMessagesToRemove);

            this.RemoveFromMessagesToId(transaction, thresholdId);
            this.RemoveFromHeadersToMessageId(transaction, thresholdId);
        }

        private long GetNumberOfMessages(DbTransaction transaction)
        {
            return (long)this.ExecuteScalar(SelectNumberOfMessagesSql, transaction);
        }

        private long GetMessageIdToWhichShrink(DbTransaction transaction, long numberOfMessagesToRemove)
        {
            var offsetParameter = this.CreateParameter("@offset", numberOfMessagesToRemove);
            return (long)this.ExecuteScalar(SelectMessageIdWithOffsetSql, transaction, offsetParameter);
        }

        private void RemoveFromMessagesToId(DbTransaction transaction, long thresholdId)
        {
            var id = this.CreateParameter("@id", thresholdId);
            this.ExecuteNonQuery(DeleteMessagesToIdSql, transaction, id);
        }

        private void RemoveFromHeadersToMessageId(DbTransaction transaction, long thresholdId)
        {
            var id = this.CreateParameter("@messageid", thresholdId);
            this.ExecuteNonQuery(DeleteHeadersToMessageIdSql, transaction, id);
        }

        private void Vacuum(DbTransaction transaction)
        {
            this.ExecuteNonQuery("vacuum", transaction);
        }

        private DbConnection CreateConnection()
        {
            var connectionString = string.Format("Data Source={0}", this.DatabasePath);
            var connection = new SQLiteConnection(connectionString);

            if (this.Password != null && this.Password.Length > 0)
            {
                connection.SetPassword(this.Password);
            }

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

                    this.AddReynaHeader(message);
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

        private void AddReynaHeader(IMessage message)
        {
            message.Headers.Add("reyna-id", message.Id.ToString());
        }
    }
}
