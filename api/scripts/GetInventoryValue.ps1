$setUrl = 'https://mtg-inventory.azurewebsites.net/api/Sets'
$setList = (Invoke-WebRequest $setUrl).Content | ConvertFrom-Json

# https://mtg-inventory.azurewebsites.net/api/Collection/dom?count=>=1&price=>=1
$collectionUrl = 'https://mtg-inventory.azurewebsites.net/api/Collection/<set>?count=%3E%3D1&price=%3E%3D1'

$fullList = $setList | ForEach-Object {
    $setCode = $_.code
    $setName = $_.name

    Write-Host "Checking set $setCode - $setName"

    $url = $collectionUrl.Replace("<set>", $setCode)
    # Write-Host $url

    $val = (Invoke-WebRequest $url).Content | ConvertFrom-Json

    $val | Select-Object -ExpandProperty card -Property totalCount 
        | Select-Object -Property setCode,collectorNumber,name,price,priceFoil,totalCount
}

if ($fullList.Length -gt 0) {
    $path = "Results.csv"
    $fullList | Export-Csv -Path $path
    Write-Host "Wrote file: $path"
}