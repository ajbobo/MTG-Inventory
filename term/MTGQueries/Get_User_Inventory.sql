SELECT SetCode,
    CollectorNumber,
    Name,
    Attrs,
    Count
FROM user_inventory
WHERE Count > 0
ORDER BY CollectorNumber,
    Attrs