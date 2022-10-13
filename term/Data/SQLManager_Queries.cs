namespace MTG_CLI
{
    public partial class SQLiteManager
    {
        private string[] _queries = new string[Enum.GetNames(typeof(InternalQuery)).Length];

        private void PopulateQueries()
        {
            AddQuery(InternalQuery.CREATE_USER_INVENTORY,
                @"  DROP TABLE IF EXISTS user_inventory;
                    CREATE TABLE user_inventory (
                        SetCode varchar(5), 
                        CollectorNumber varchar(3), 
                        Name varchar(128), 
                        Attrs varchar(25), 
                        Count int 
                    );
                    CREATE UNIQUE INDEX idx_CollectorNumber_Attrs ON user_inventory (CollectorNumber, Attrs);
                ");
            AddQuery(InternalQuery.ADD_TO_USER_INVENTORY,
                @" INSERT INTO user_inventory (SetCode, CollectorNumber, Name, Attrs, Count) 
                   VALUES ( @SetCode, @CollectorNumber, @Name, @Attrs, @Count ); 
                ");
            AddQuery(InternalQuery.CREATE_SET_TABLE,
                @"  DROP TABLE IF EXISTS sets;
                    CREATE TABLE sets ( 
                        SetCode varchar(4), 
                        Name varchar(128) 
                    )
                ");
            AddQuery(InternalQuery.INSERT_SET,
                @"  INSERT INTO sets ( SetCode, Name )
                    VALUES ( @SetCode, @Name )
                ");
            AddQuery(InternalQuery.GET_ALL_SETS,
                @"  SELECT Name, SetCode 
                    FROM sets 
                ");
            AddQuery(InternalQuery.GET_SET_NAME,
                @"  SELECT Name 
                    FROM sets 
                    WHERE SetCode = @SetCode 
                ");
            AddQuery(InternalQuery.GET_SET_CODE,
                @"  SELECT SetCode 
                    FROM sets WHERE name = @Name 
                ");
            AddQuery(InternalQuery.CREATE_CARD_TABLE,
                @"  DROP TABLE IF EXISTS cards;
                    CREATE TABLE cards ( 
                        SetCode varchar(5), 
                        CollectorNumber varchar(4), 
                        Name varchar(64), 
                        Rarity varchar(8),
                        ColorIdentity varchar(5),
                        ManaCost varchar(15),
                        TypeLine varchar(128),
                        FrontText varchar,
                        Price varchar(10),
                        PriceFoil varchar(10)
                    )
                ");
            AddQuery(InternalQuery.INSERT_CARD,
                @"  INSERT INTO cards ( SetCode, CollectorNumber, Name, Rarity, ColorIdentity, ManaCost, TypeLine, FrontText, Price, PriceFoil )
                    VALUES ( @SetCode, @CollectorNumber, @Name, @Rarity, @ColorIdentity, @ManaCost, @TypeLine, @FrontText, @Price, @PriceFoil )
                ");
            AddQuery(InternalQuery.GET_SET_CARDS,
                @"  SELECT cds.CollectorNumber,
                           IFNULL(inv.Total || IFNULL(foil.Symbol, '') || IFNULL(other.Symbol, ''), 0) AS Cnt,
                           CAST(IFNULL(inv.Total, 0) AS NUM) AS CntNum,
                           cds.Name,
                           cds.Rarity,
                           cds.ColorIdentity,
                           cds.ManaCost,
                           cds.Price,
                           cds.PriceFoil
                    FROM cards cds
                            LEFT JOIN (SELECT CollectorNumber, CAST(SUM(Count) as TEXT) AS Total
                                        FROM user_inventory
                                        GROUP BY CollectorNumber) inv
                                    ON cds.CollectorNumber = inv.CollectorNumber
                            LEFT JOIN (SELECT CollectorNumber, '*' AS Symbol
                                        FROM user_inventory
                                        WHERE attrs LIKE '%foil%'
                                          AND Count > 0
                                        GROUP BY CollectorNumber) foil
                                    ON cds.CollectorNumber = foil.CollectorNumber
                            LEFT JOIN (SELECT CollectorNumber, 'Ω' AS Symbol
                                        FROM user_inventory
                                        WHERE attrs <> 'Standard'
                                          AND attrs <> 'foil'
                                          AND Count > 0
                                        GROUP BY CollectorNumber) other
                                    ON cds.CollectorNumber = other.CollectorNumber
                    WHERE (CntNum >= @MinCnt AND CntNum <= @MaxCnt)
                        AND (Rarity IN (@r0, @r1, @r2, @r3))
                        AND ((ColorIdentity LIKE '%W%' and @W) OR 
                             (ColorIdentity LIKE '%U%' and @U) OR 
                             (ColorIdentity LIKE '%B%' and @B) OR 
                             (ColorIdentity LIKE '%R%' and @R) OR
                             (ColorIdentity LIKE '%G%' and @G) OR
                             (ColorIdentity =    '' and @X))
                    ORDER BY cds.ROWID
                ");
            AddQuery(InternalQuery.GET_SINGLE_CARD_COUNT,
                @"  SELECT ifnull(inv.Total || IFNULL(foil.Symbol, '') || IFNULL(other.Symbol, ''), 0) AS Cnt
                    FROM cards cds
                            LEFT JOIN (SELECT CollectorNumber, CAST(SUM(Count) as TEXT) AS Total
                                        FROM user_inventory
                                        GROUP BY CollectorNumber) inv
                                    ON cds.CollectorNumber = inv.CollectorNumber
                            LEFT JOIN (SELECT CollectorNumber, '*' AS Symbol
                                        FROM user_inventory
                                        WHERE attrs LIKE '%foil%'
                                           AND Count > 0
                                        GROUP BY CollectorNumber) foil
                                    ON cds.CollectorNumber = foil.CollectorNumber
                            LEFT JOIN (SELECT CollectorNumber, 'Ω' AS Symbol
                                        FROM user_inventory
                                        WHERE attrs <> 'Standard'
                                          AND attrs <> 'foil'
                                          AND Count > 0
                                        GROUP BY CollectorNumber) other
                                    ON cds.CollectorNumber = other.CollectorNumber
                    WHERE cds.CollectorNumber = @CollectorNumber
                ");
            AddQuery(InternalQuery.GET_CARD_DETAILS,
                @"  SELECT CollectorNumber, Name, TypeLine, FrontText
                    FROM cards
                    WHERE CollectorNumber = @CollectorNumber
                ");
            AddQuery(InternalQuery.GET_CARD_CTCS, // This needs the join to get the name of cards that don't have any CTCs
                @"  SELECT cds.Name, inv.Attrs, inv.Count
                    FROM cards cds
                            LEFT JOIN user_inventory inv
                                    ON cds.CollectorNumber = inv.CollectorNumber
                    WHERE cds.CollectorNumber = @CollectorNumber
                    ORDER BY Attrs
                ");
            AddQuery(InternalQuery.UPDATE_CARD_CTC,
                @"  INSERT INTO user_inventory (SetCode, CollectorNumber, Name, Attrs, Count)
                    VALUES (
                        (SELECT SetCode FROM cards LIMIT 1), 
                        @CollectorNumber, 
                        (SELECT Name FROM cards WHERE CollectorNumber = @CollectorNumber), 
                        @Attrs, 
                        @Count
                        )
                    ON CONFLICT DO UPDATE SET Count = @Count
                ");
            AddQuery(InternalQuery.GET_CARD_NAMES,
                @"  SELECT DISTINCT Name 
                    FROM cards 
                ");
            AddQuery(InternalQuery.GET_CARD_NUMBER,
                @"  SELECT CollectorNumber 
                    FROM cards 
                    WHERE Name = @Name
                ");
            AddQuery(InternalQuery.GET_USER_INVENTORY,
                @"  SELECT SetCode, CollectorNumber, Name, Attrs, Count
                    FROM user_inventory
                    WHERE Count > 0
                    ORDER BY CollectorNumber, Attrs
                ");
        }

        private void AddQuery(InternalQuery id, string query)
        {
            _queries[(int)id] = query;
        }
    }
}