param(
    [Parameter(Mandatory)]
    [string]$fileName # This file needs to be in .csv format
)

$csvData = Import-Csv $fileName | ForEach-Object {
    [PSCustomObject]@{
        name = $_.Name;
        count = $_.Count;
    }
}

$deck = [PSCustomObject]@{
    name = $fileName.Substring(2,$fileName.Length - 6);
    cards = $csvData;
}

$deck | ConvertTo-Json