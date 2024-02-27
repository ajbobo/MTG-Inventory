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

$start = $fileName.LastIndexOf('\');
$name = $fileName.Substring($start + 1, $fileName.Length - $start - 5);

$deck = [PSCustomObject]@{
    name = $name
    cards = $csvData;
}

$deck | ConvertTo-Json