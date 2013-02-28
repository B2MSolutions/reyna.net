namespace reyna
{
    using System;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.IO;
    using System.Net;
    using System.Text;
    using reyna.Interfaces;

    public class SQLiteRepository : IRepository
    {
        public bool DoesNotExist(string name)
        {
            return !File.Exists(name);
        }

        public void Create(string name)
        {
            SQLiteConnection.CreateFile(name);

            this.DatabaseAction((t) =>
                {
                    this.ExecuteNonQuery("CREATE TABLE Message (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT, body TEXT);CREATE TABLE Header (id INTEGER PRIMARY KEY AUTOINCREMENT, messageid INTEGER, key TEXT, value TEXT, FOREIGN KEY(messageid) REFERENCES message(id));", t);
                    t.Commit();
                });
        }

        public IMessage Enqueue(IMessage message)
        {
            return this.DatabaseFunc((t) =>
                {
                    var sql = this.CreateInsertSql(message);
                    var messageId = Convert.ToInt32(this.ExecuteScalar(this.CreateInsertSql(message), t));

                    sql = this.CreateInsertSql(messageId, message.Headers);
                    this.ExecuteScalar(sql, t);

                    var clone = this.AssignIdTo(message, messageId);

                    t.Commit();
                    return clone;
                });
        }

        public IMessage Peek()
        {
            // TODO
            // complete refactoring.

            //return this.DatabaseFunc((t) =>
            //    {
            //        var sql = this.CreateSelectTop1Sql();
            //        using (var reader = this.ExecuteReader(sql, t))
            //        {
            //        }

            //        t.Commit();

            //        return Message;
            //    });

            IMessage message = null;

            using (var connection = new SQLiteConnection("Data Source=reyna.db"))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT * FROM Message ORDER BY id ASC LIMIT 1", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToInt32(reader["id"]);
                        var url = new Uri(reader["url"] as string);
                        var body = reader["body"] as string;
                        message = new Message(url, body) { Id = id };
                    }
                }

                if (message == null)
                {
                    return null;
                }

                using (var command = new SQLiteCommand(string.Format("SELECT * FROM Header WHERE messageid = {0}", message.Id), connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var key = reader["key"] as string;
                        var value = reader["value"] as string;
                        message.Headers.Add(key, value);
                    }
                }
            }

            return message;
        }

        public IMessage Dequeue()
        {
            //return this.ActOnDatabase((t) =>
            //{
                //var sql = this.CreateInsertSql(message);
                //var messageId = Convert.ToInt32(this.ExecuteScalar(sql, t));

                //sql = this.CreateInsertSql(messageId, message.Headers);
                //this.ExecuteScalar(sql, t);

                //var clone = this.AssignIdTo(message, messageId);

                //t.Commit();
                //return clone;
            //});


            // TODO: encapsulate these 2 calls in a transaction?
            var message = this.Peek();
            if (message == null)
            {
                return null;
            }

            return this.DatabaseFunc((t) =>
                {
                    // TODO: roll the above code into this transaction

                    var sql = this.CreateDeleteSql(message);
                    this.ExecuteNonQuery(sql, t);
                    t.Commit();
                    return message;
                });
        }

        private string CreateSelectTop1Sql()
        {
            return "SELECT * FROM Message ORDER BY id ASC LIMIT 1";
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
            // TODO - remove Cloneable interface from Message. this is the only place where we need it.
            var clone = message.Clone() as Message;
            clone.Id = id;

            return clone;
        }

        private DbConnection CreateConnection()
        {
            var connection = new SQLiteConnection("Data Source=reyna.db");
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

        private IMessage DatabaseFunc(Func<DbTransaction, IMessage> func)
        {
            using (var connection = this.CreateConnection())
            using (var transaction = connection.BeginTransaction())
            {
                return func(transaction);
            }
        }

        private void DatabaseAction(Action<DbTransaction> action)
        {
            using (var connection = this.CreateConnection())
            using (var transaction = connection.BeginTransaction())
            {
                action(transaction);
            }
        }
    }
}
