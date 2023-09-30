param(
    [string]
    $name,

    [string]
    $code
)

Write-Host "Uploading Set $name ($code)"

$json = [PSCustomObject]@{
    SetCode = $code
    SetName = $name
} | ConvertTo-Json

Invoke-WebRequest 'https://localhost:7269/api/Sets' -Method POST -Body $json -ContentType 'application/json' 
    | Select-Object StatusCode, Content