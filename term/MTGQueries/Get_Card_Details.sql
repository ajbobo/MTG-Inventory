SELECT CollectorNumber,
    Name,
    TypeLine,
    FrontText
FROM cards
WHERE CollectorNumber = @CollectorNumber