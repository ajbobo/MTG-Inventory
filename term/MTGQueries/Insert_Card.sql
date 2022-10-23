INSERT INTO cards (
        SetCode,
        CollectorNumber,
        Name,
        Rarity,
        ColorIdentity,
        ManaCost,
        TypeLine,
        FrontText,
        Price,
        PriceFoil
    )
VALUES (
        @SetCode,
        @CollectorNumber,
        @Name,
        @Rarity,
        @ColorIdentity,
        @ManaCost,
        @TypeLine,
        @FrontText,
        @Price,
        @PriceFoil
    )