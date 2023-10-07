param(
    [Parameter(Mandatory)]
    [string]$SetCode
)

$sets = Invoke-WebRequest "https://mtg-inventory.azurewebsites.net/api/Collection/$SetCode" 
    | ConvertFrom-Json 

$sets | Where-Object ctcs -ne $null
    | select-object -ExpandProperty Card -Property CTCs
    | select-object  -Property CollectorNumber,Name -ExpandProperty CTCs
    | select-object CollectorNumber,Name,Cardtype,Count