using Microsoft.Data.Sqlite;

namespace Migrator2
{
    public class SQLManager
    {
        public static InternalQuery CREATE_USER_INVENTORY_TABLE { get; private set; } = new InternalQuery(
            @"
                INSERT INTO user_inventory (SetCode, CollectorNumber, Name, Attrs, Count)
                VALUES (
                    @SetCode,
                    @CollectorNumber,
                    @Name,
                    @Attrs,
                    @Count
                );
            ",
                "@SetCode",
                "@CollectorNumber",
                "@Name",
                "@Attrs",
                "@Count"
        );

        public class InternalQuery
        {
            private SqliteCommand _command = new SqliteCommand();

            public InternalQuery(string query, params string[] parameters)
            {
                _command.CommandText = query;
                foreach (string param in parameters)
                {
                    _command.Parameters.AddWithValue(param, "");
                }
            }

            public int Execute(SqliteConnection connection, params object?[] parameters)
            {
                _command.Connection = connection;

                for (int x = 0; x < parameters.Length; x++)
                {
                    _command.Parameters[x].Value = parameters[x];
                }

                return _command.ExecuteNonQuery();
            }

        }
    }
}