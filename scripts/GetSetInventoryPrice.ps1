param(
    [Parameter(Mandatory)]
    [string]$SetCode
)

$sets = Invoke-WebRequest "https://mtg-inventory.azurewebsites.net/api/Collection/$SetCode" 
    | ConvertFrom-Json 

$sets | Where-Object ctcs -NE $null
    | Select-Object -ExpandProperty Card -Property TotalCount
    | Select-Object CollectorNumber, Name, Price, TotalCount
    | Where-Object Price -gt 1.00