using System.Data;
using Microsoft.Data.Sqlite;

namespace MTG_CLI
{
    public partial class SQLManager
    {
        private SqliteCommand? _command;
        private SqliteConnection _connection;
        private SqliteDataReader? _reader;
        
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

        public SQLManager WithParam(string param, long value)
        {
            _command?.Parameters.AddWithValue(param, value);
            return this;
        }

        public SQLManager WithParam(string param, int value)
        {
            _command?.Parameters.AddWithValue(param, value.ToString());
            return this;
        }

        public SQLManager WithFilters(FilterSettings filterSettings)
        {
            _command?.Parameters.AddWithValue("@MinCnt", filterSettings.GetMinCount());
            _command?.Parameters.AddWithValue("@MaxCnt", filterSettings.GetMaxCount());

            string[] rarities = filterSettings.GetRarities();
            for (int x = 0; x < rarities.Count(); x++)
                _command?.Parameters.AddWithValue($"@r{x}", rarities[x].ToLower());

            string colors = filterSettings.GetColors();
            bool all = (colors.Length == 0);
            char[] COLOR_LIST = {'W', 'U', 'B', 'R', 'G', 'X'};
            foreach (char curChar in COLOR_LIST)
                _command?.Parameters.AddWithValue($"@{curChar}", all || colors.Contains(curChar));
                
            return this;
        }

        public int Execute()
        {
            return _command?.ExecuteNonQuery() ?? 0;
        }

        public T? ExecuteScalar<T>()
        {
            object? res = _command?.ExecuteScalar() ?? null;

            return (res != null ? (T)res : default(T));
        }

        public void Read()
        {
            _reader = _command?.ExecuteReader() ?? null;
        }

        public bool HasReader()
        {
            return _reader != null;
        }

        public bool ReadNext()
        {
            return (_reader != null && _reader.Read());
        }

        public T ReadValue<T>(string fieldName, T fallback)
        {
            if (_reader == null || _reader.IsDBNull(fieldName))
                return fallback;

            return _reader.GetFieldValue<T>(fieldName);
        }

        public void Close()
        {
            _reader?.Close();
        }
    }
}