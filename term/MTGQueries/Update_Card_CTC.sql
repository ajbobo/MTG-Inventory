INSERT INTO user_inventory (SetCode, CollectorNumber, Name, Attrs, Count)
VALUES (
    (SELECT SetCode FROM cards LIMIT 1), 
    @CollectorNumber, 
    (SELECT Name FROM cards WHERE CollectorNumber = @CollectorNumber), 
    @Attrs, 
    @Count
    )
ON CONFLICT DO UPDATE SET Count = @Count