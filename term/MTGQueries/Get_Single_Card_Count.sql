SELECT ifnull(inv.Total || IFNULL(foil.Symbol, '') || IFNULL(other.Symbol, ''), 0) AS Cnt
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