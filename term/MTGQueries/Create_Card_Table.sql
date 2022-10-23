DROP TABLE IF EXISTS cards;
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