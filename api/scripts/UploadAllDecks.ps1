$files = Get-Item '.\Deck JSONs\*.json'

$files | ForEach-Object {
    $text = Get-Content $_
    $url = "https://localhost:7269/api/Decks"
    Invoke-WebRequest -Uri $url -Method POST -Body $text -ContentType "application/json"
}