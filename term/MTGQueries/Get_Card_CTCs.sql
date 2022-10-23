SELECT cds.Name,
    inv.Attrs,
    inv.Count
FROM cards cds
    LEFT JOIN user_inventory inv 
        ON cds.CollectorNumber = inv.CollectorNumber
WHERE cds.CollectorNumber = @CollectorNumber
ORDER BY Attrs