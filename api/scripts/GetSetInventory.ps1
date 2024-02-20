param(
    [Parameter(Mandatory)]
    [string]$SetCode
)

$sets = Invoke-WebRequest "https://mtg-inventory.azurewebsites.net/api/Collection/$SetCode" 
    | ConvertFrom-Json 

$sets | Where-Object ctcs -NE $null
    | Select-Object -ExpandProperty Card -Property CTCs
    | Select-Object -Property CollectorNumber, Name -ExpandProperty CTCs
    | Select-Object CollectorNumber, Name, Cardtype, Count