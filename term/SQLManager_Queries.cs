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
            GET_SET_NAME,
            GET_SET_CODE,
            CREATE_CARD_TABLE,
            INSERT_CARD,
            GET_SET_CARDS,
            GET_CARD_DETAILS,
            GET_CARD_NAMES,
            GET_CARD_NUMBER,
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
            AddQuery(InternalQuery.GET_SET_NAME,
                @"  SELECT Name FROM sets WHERE SetCode = @SetCode "
                );
            AddQuery(InternalQuery.GET_SET_CODE,
                @"  SELECT SetCode FROM sets WHERE name = @Name "
            );
            AddQuery(InternalQuery.CREATE_CARD_TABLE,
                @"  DROP TABLE IF EXISTS cards;
                    CREATE TABLE cards ( 
                        SetCode varchar(4), 
                        Collector_Number varchar(4), 
                        Name varchar(64), 
                        Rarity varchar(8),
                        ColorIdentity varchar(5),
                        ManaCost varchar(15),
                        TypeLine varchar(128),
                        FrontText varchar
                    )"
                );
            AddQuery(InternalQuery.INSERT_CARD,
                @"  INSERT INTO cards ( SetCode, Collector_Number, Name, Rarity, ColorIdentity, ManaCost, TypeLine, FrontText )
                    VALUES ( @SetCode, @Collector_Number, @Name, @Rarity, @ColorIdentity, @ManaCost, @TypeLine, @FrontText )"
                );
            AddQuery(InternalQuery.GET_SET_CARDS,
                @"  SELECT Collector_Number, Name, Rarity, ColorIdentity, ManaCost
                    FROM cards 
                    WHERE SetCode = @SetCode"
                );
            AddQuery(InternalQuery.GET_CARD_DETAILS,
                @"  SELECT Collector_Number, Name, TypeLine, FrontText
                    FROM cards
                    WHERE Collector_Number = @Collector_Number"
                );
            AddQuery(InternalQuery.GET_CARD_NAMES,
                @"  SELECT DISTINCT Name FROM cards "
                );
            AddQuery(InternalQuery.GET_CARD_NUMBER,
                @"  SELECT Collector_Number 
                    FROM cards 
                    WHERE Name = @Name"
                );
        }

        private static void AddQuery(InternalQuery id, string query)
        {
            _queries[(int)id] = query;
        }
    }
}