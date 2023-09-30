$data = Get-Content output.json | ConvertFrom-Json -Depth 10
$setList = $data | Get-Member -MemberType NoteProperty
Write-Host "$($setList.Count) Sets available"

$setList | ForEach-Object {
    $setName = $_.Name
    Write-Host "Current set: $setname"

    $set = $data | Select-Object -ExpandProperty $setName
    Write-Host "$($set.Count) Cards"
    $set | ForEach-Object {
        $card = $_
        $number = $card.CollectorNumber
        $CTCs = @()
        $card.Counts | Get-Member -MemberType NoteProperty | ForEach-Object {
            $type = $_.Name
            $count = ($card.Counts | Select-Object -ExpandProperty $type)
            $CTCs += [PSCustomObject]@{
                CardType = $_.Name
                Count = $count
            }
        }

        Write-Host "Sending Card `"$($card.Name)`""

        $url = "https://localhost:7269/api/Collection/$setName/card/$number"
        $body = [PSCustomObject]@{
            Name = $card.Name
            CTCs = $CTCs
        } | ConvertTo-Json

        $headers = @{
            'Accept'       = '*/*'
            'Content-Type' = 'application/json'
        }

        # Write-Host "Calling API: $url  with body $body"
        $res = Invoke-WebRequest -Method PUT -Uri $url -Body $body -Headers $headers
    }

    Start-Sleep -Seconds 2
    
}