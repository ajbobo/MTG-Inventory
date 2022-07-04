using Microsoft.Data.Sqlite;

namespace Migrator2
{
    public partial class SQLManager
    {
        public enum InternalQuery
        {
            CREATE_USER_INVENTORY,
            ADD_TO_USER_INVENTORY,
            GET_SET_UNIQUE_CARDS,
            GET_SET_PLAYSETS,
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
            AddQuery(InternalQuery.GET_SET_UNIQUE_CARDS,
                @" SELECT SetCode, CollectorNumber, Name, Attrs, Count FROM user_inventory WHERE SetCode = @SetCode"
                );
            AddQuery(InternalQuery.GET_SET_PLAYSETS,
                @" select * FROM (
                        SELECT CollectorNumber, Name, Sum(Count) as Total from user_inventory where setCode = @SetCode GROUP BY CollectorNumber
                    ) WHERE Total >= 4 ORDER BY CollectorNumber"
                );
        }

        private static void AddQuery(InternalQuery id, string query)
        {
            _queries[(int)id] = query;
        }
    }
}