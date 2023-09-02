$sets = Invoke-WebRequest 'https://api.scryfall.com/sets' 
    | Select-Object -ExpandProperty Content 
    | ConvertFrom-Json 
    | Select-Object -ExpandProperty data
Write-Host "All sets: $($sets.Length)"

$filtered = $sets |
    Where-Object set_type -In ('core', 'expansion', 'masterpiece', 'masters', 'commander', 'draft_innovation', 'funny')
Write-Host "Filtered Sets: $($filtered.Length)"

$collectable = $filtered | Where-Object {
    ($_.set_type -ne 'funny' -and $_.parent_set_code -ne 'sld') -or ($_.block_code -eq $null -and $_.parent_set_code -eq $null) 
}
Write-Host "Collectable sets: $($collectable.Length)"

$collectable | Select-Object name, code