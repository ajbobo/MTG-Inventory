using Microsoft.Data.Sqlite;

namespace Migrator2
{
    public partial class SQLManager
    {
        private SqliteCommand? _command;
        private SqliteConnection _connection;
        
        public SQLManager(SqliteConnection connection)
        {
            _connection = connection;
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

        public int Go()
        {
            return _command?.ExecuteNonQuery() ?? 0;
        }
    }
}