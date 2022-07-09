using Microsoft.Data.Sqlite;

namespace MTG_CLI
{
    public partial class SQLManager
    {
        public enum InternalQuery
        {
            CREATE_SET_TABLE,
            INSERT_SET,
            GET_ALL_SETS,
            CREATE_CARD_TABLE,
            INSERT_CARD,
            GET_SET_CARDS,
            // These aren't in use for real yet
            CREATE_USER_INVENTORY,
            ADD_TO_USER_INVENTORY,
            GET_SET_PLAYSETS,
        };

        private static string[] _queries = new string[Enum.GetNames(typeof(InternalQuery)).Length];

        static SQLManager()
        {
            AddQuery(InternalQuery.CREATE_USER_INVENTORY,
                @" DROP TABLE IF EXISTS user_inventory;
                   CREATE TABLE user_inventory (SetCode varchar(4), CollectorNumber varchar(3), Name varchar(128), Attrs varchar(25), Count int );"
                );
            AddQuery(InternalQuery.ADD_TO_USER_INVENTORY,
                @" INSERT INTO user_inventory (SetCode, CollectorNumber, Name, Attrs, Count) 
                   VALUES ( @SetCode, @CollectorNumber, @Name, @Attrs, @Count ); "
                );
            AddQuery(InternalQuery.GET_SET_PLAYSETS,
                @" select * FROM (
                        SELECT CollectorNumber, Name, Sum(Count) as Total from user_inventory where setCode = @SetCode GROUP BY CollectorNumber
                    ) WHERE Total >= 4 ORDER BY CollectorNumber"
                );
            AddQuery(InternalQuery.CREATE_SET_TABLE,
                @"  DROP TABLE IF EXISTS sets;
                    CREATE TABLE sets ( SetCode varchar(4), Name varchar(128) )"
                );
            AddQuery(InternalQuery.INSERT_SET,
                @"  INSERT INTO sets ( SetCode, Name )
                    VALUES ( @SetCode, @Name )"
                );
            AddQuery(InternalQuery.GET_ALL_SETS,
                @"  SELECT Name, SetCode FROM sets "
                );
            AddQuery(InternalQuery.CREATE_CARD_TABLE,
                @"  DROP TABLE IF EXISTS cards;
                    CREATE TABLE cards ( SetCode varchar(4), Collector_Number varchar(4), Name varchar(128), Rarity varchar(8) )"
                );
            AddQuery(InternalQuery.INSERT_CARD,
                @"  INSERT INTO cards ( SetCode, Collector_Number, Name, Rarity )
                    VALUES ( @SetCode, @Collector_Number, @Name, @Rarity )"
                );
            AddQuery(InternalQuery.GET_SET_CARDS,
                @"  SELECT SetCode, Collector_Number, Name, Rarity FROM cards WHERE SetCode = @SetCode"
                );
        }

        private static void AddQuery(InternalQuery id, string query)
        {
            _queries[(int)id] = query;
        }
    }
}