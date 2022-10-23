SELECT cds.CollectorNumber,
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