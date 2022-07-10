using Microsoft.Data.Sqlite;

namespace MTG_CLI
{
    public partial class SQLManager
    {
        private SqliteCommand? _command;
        private SqliteConnection _connection;
        
        public SQLManager()
        {
            _connection = new SqliteConnection("Data source=inv.db");
            _connection.Open();
        }

        public SQLManager Query(InternalQuery query)
        {
            _command = new SqliteCommand();
            _command.Connection = _connection;
            _command.CommandText = _queries[(int)query];
            return this;
        }

        public SQLManager WithParam(string param, string value)
        {
            _command?.Parameters.AddWithValue(param, value);
            return this;
        }

        public SQLManager WithParam(string param, int value)
        {
            _command?.Parameters.AddWithValue(param, value.ToString());
            return this;
        }

        public int Execute()
        {
            return _command?.ExecuteNonQuery() ?? 0;
        }

        public SqliteDataReader? Read()
        {
            return _command?.ExecuteReader() ?? null;
        }

        public T? ExecuteScalar<T>()
        {
            object? res = _command?.ExecuteScalar() ?? null;

            return (res != null ? (T)res : default(T));
        }
    }
}