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
        private const string SelectTop1MessageSqlFrom = "SELECT id, url, body FROM Message WHERE id > @messageId ORDER BY id ASC LIMIT 1";
        private const string SelectHeaderSql = "SELECT key, value FROM Header WHERE messageid = @messageId";
        private const string SelectMinIdWithTypeSql = "SELECT min(id) FROM Message WHERE url = @type";
        private const string SelectNumberOfMessagesSql = "SELECT count(*) from Message";
        private const string SelectMessageIdWithOffsetSql = "SELECT id FROM Message LIMIT 1 OFFSET @offset";
        private const string DeleteMessagesToIdSql = "DELETE FROM Message WHERE id < @id";
        private const string DeleteHeadersToMessageIdSql = "DELETE FROM Header WHERE messageid < @id";

        private static readonly object Locker = new object();

        public SQLiteRepository()
        {
            this.SizeDifferenceToStartCleaning = 307200; ////300Kb in bytes
        }

        public SQLiteRepository(byte[] password)
        {
            this.Password = password;
            this.SizeDifferenceToStartCleaning = 307200; ////300Kb in bytes
        }

        private delegate IMessage ExecuteFunctionInTransaction(DbTransaction transaction);

        private delegate void ExecuteActionInTransaction(IMessage message, DbTransaction transaction);

        public event EventHandler<EventArgs> MessageAdded;

        internal long SizeDifferenceToStartCleaning { get; set; }

        internal bool Exists
        {
            get
            {
                FileInfo fileInfo = new FileInfo(this.DatabasePath);
                return File.Exists(this.DatabasePath) && fileInfo.Length >= 4096;
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
            lock (Locker)
            {
                if (this.Exists)
                {
                    return;
                }

                this.Create();
            }
        }

        public void Add(IMessage message)
        {
            lock (Locker)
            {
                this.ExecuteInTransaction((t) => this.InsertMessage(t, message));
            }
        }

        public void Add(IMessage message, long storageSizeLimit)
        {
            lock (Locker)
            {
                this.ExecuteInTransaction(t =>
                {
                    long dbSize = this.GetDbSize(t);

                    if (this.DbSizeApproachesLimit(dbSize, storageSizeLimit))
                    {
                        this.ClearOldRecords(t, message);
                    }

                    this.InsertMessage(t, message);
                });
            }
        }

        public IMessage Get()
        {
            return this.GetNextMessageAfter(0);
        }

        public IMessage Remove()
        {
            var message = this.GetFirstInQueue();
            this.Delete(message);
            return message;
        }

        public void ShrinkDb(long limit)
        {
            lock (Locker)
            {
                this.Execute(connection =>
                {
                    limit -= this.SizeDifferenceToStartCleaning;
                    long size = this.GetDbSize(connection);

                    if (size <= limit)
                    {
                        return;
                    }

                    do
                    {
                        this.Shrink(connection, limit, size);
                        this.Vacuum(connection);
                        size = this.GetDbSize(connection);
                    }
                    while (size > limit);
                });
            }
        }

        public IMessage GetNextMessageAfter(long messageId)
        {
            var messageIdParameter = this.CreateParameter("@messageId", messageId);
            return this.GetMessage(SQLiteRepository.SelectTop1MessageSqlFrom, messageIdParameter);
        }

        public void Delete(IMessage message)
        {
            if (message == null)
            {
                return;
            }

            var sql = SQLiteRepository.DeleteMessageSql;
            var messageId = this.CreateParameter("@messageId", message.Id);
            this.ExecuteInTransaction((t) => this.ExecuteNonQuery(sql, t, messageId));
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

        private void InsertMessage(DbTransaction transaction, IMessage message)
        {
            var sql = SQLiteRepository.InsertMessageSql;
            var url = this.CreateParameter("@url", message.Url);
            var body = this.CreateParameter("@body", message.Body);
            var id = Convert.ToInt32(this.ExecuteScalar(sql, transaction, url, body));

            sql = SQLiteRepository.InsertHeaderSql;
            var messageId = this.CreateParameter("@messageId", id);

            foreach (string headerKey in message.Headers.Keys)
            {
                var key = this.CreateParameter("@key", headerKey);
                var value = this.CreateParameter("@value", message.Headers[headerKey]);
                this.ExecuteScalar(sql, transaction, messageId, key, value);
            }

            if (this.MessageAdded != null)
            {
                this.MessageAdded.Invoke(this, EventArgs.Empty);
            }
        }

        private long GetDbSize(DbTransaction transaction)
        {
            return (long)this.ExecuteScalar("pragma page_size", transaction) * (long)this.ExecuteScalar("pragma page_count", transaction);
        }

        private long GetDbSize(DbConnection connection)
        {
            return (long)this.ExecuteScalar("pragma page_size", connection) * (long)this.ExecuteScalar("pragma page_count", connection);
        }

        private bool DbSizeApproachesLimit(long size, long limit)
        {
            return (limit > size) && (limit - size) < this.SizeDifferenceToStartCleaning;
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
            object result = this.ExecuteScalar(SelectMinIdWithTypeSql, transaction, typeParameter);

            if (result is DBNull)
            {
                return null;
            }

            return (long)result;
        }

        private void Shrink(DbConnection connection, long sizeLimit, long size)
        {
            double limitPercentage = 1 - ((double)sizeLimit / size);
            long numberOfMessages = this.GetNumberOfMessages(connection);
            long numberOfMessagesToRemove = (long)Math.Round(numberOfMessages * limitPercentage);
            numberOfMessagesToRemove = numberOfMessagesToRemove == 0 ? 1 : numberOfMessagesToRemove;

            long thresholdId = this.GetMessageIdToWhichShrink(connection, numberOfMessagesToRemove);

            this.ExecuteInTransaction((t) =>
            {
                this.RemoveFromHeadersToMessageId(t, thresholdId);
                this.RemoveFromMessagesToId(t, thresholdId);
            });
        }

        private long GetNumberOfMessages(DbConnection connection)
        {
            return (long)this.ExecuteScalar(SelectNumberOfMessagesSql, connection);
        }

        private long GetMessageIdToWhichShrink(DbConnection connection, long numberOfMessagesToRemove)
        {
            var offsetParameter = this.CreateParameter("@offset", numberOfMessagesToRemove);
            return (long)this.ExecuteScalar(SelectMessageIdWithOffsetSql, connection, offsetParameter);
        }

        private void RemoveFromMessagesToId(DbTransaction transaction, long thresholdId)
        {
            var id = this.CreateParameter("@id", thresholdId);
            this.ExecuteNonQuery(DeleteMessagesToIdSql, transaction, id);
        }

        private void RemoveFromHeadersToMessageId(DbTransaction transaction, long thresholdId)
        {
            var id = this.CreateParameter("@id", thresholdId);
            this.ExecuteNonQuery(DeleteHeadersToMessageIdSql, transaction, id);
        }

        private void Vacuum(DbConnection connection)
        {
            this.ExecuteNonQuery("vacuum", connection);
        }

        private IMessage GetFirstInQueue()
        {
            return this.GetMessage(SQLiteRepository.SelectTop1MessageSql, null);
        }

        private IMessage GetMessage(string sql, DbParameter parameter)
        {
            return this.ExecuteInTransaction((t) =>
            {
                IMessage message = null;

                using (var reader = this.ExecuteReader(sql, t, parameter))
                {
                    reader.Read();
                    message = this.CreateFromDataReader(reader);
                }

                if (message == null)
                {
                    return null;
                }

                var messageParameter = this.CreateParameter("@messageid", message.Id);
                using (var reader = this.ExecuteReader(SQLiteRepository.SelectHeaderSql, t, messageParameter))
                {
                    while (reader.Read())
                    {
                        this.AddHeader(message, reader);
                    }

                    this.AddReynaHeader(message);
                }

                return message;
            });
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
                if (parameter != null)
                {
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        private DbCommand CreateCommand(string sql, DbConnection connection, params DbParameter[] parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Connection = connection;

            foreach (var parameter in parameters)
            {
                if (parameter != null)
                {
                    command.Parameters.Add(parameter);
                }
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

        private int ExecuteNonQuery(string sql, DbConnection connection, params DbParameter[] parameters)
        {
            using (var command = this.CreateCommand(sql, connection, parameters))
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

        private object ExecuteScalar(string sql, DbConnection connection, params DbParameter[] parameters)
        {
            using (var command = this.CreateCommand(sql, connection, parameters))
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

        private void Execute(Action<DbConnection> action)
        {
            using (var connection = this.CreateConnection())
            {
                action(connection);
            }
        }
    }
}
