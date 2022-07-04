using Microsoft.Data.Sqlite;

namespace Migrator2
{
    public partial class SQLManager
    {
        public enum InternalQuery
        {
            CREATE_USER_INVENTORY,
            ADD_TO_USER_INVENTORY,
        };

        private static string[] _queries = new string[Enum.GetNames(typeof(InternalQuery)).Length];

        static SQLManager()
        {
            AddQuery(InternalQuery.CREATE_USER_INVENTORY,
                @" DROP TABLE IF EXISTS user_inventory;
                CREATE TABLE user_inventory (SetCode varchar(4), CollectorNumber varchar(3), Name varchar(128), Attrs varchar(25), Count int );
                ");
            AddQuery(InternalQuery.ADD_TO_USER_INVENTORY,
                @" INSERT INTO user_inventory (SetCode, CollectorNumber, Name, Attrs, Count) 
                VALUES ( @SetCode, @CollectorNumber, @Name, @Attrs, @Count ); 
                ");
        }

        private static void AddQuery(InternalQuery id, string query)
        {
            _queries[(int)id] = query;
        }
    }
}