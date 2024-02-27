$csvs = Get-Item ".\Deck CSVs\*.csv"

$csvs | ForEach-Object {
    $name = $_.FullName
    $start = $name.LastIndexOf('\')
    $shortName = $name.Substring($start + 1, $name.Length - $start - 5) + '.json';
    Write-Host "Converting $name to $shortName"
    .\ConvertDeckToJson.ps1 -fileName $_.FullName > ".\Deck JSONs\$shortName"
}