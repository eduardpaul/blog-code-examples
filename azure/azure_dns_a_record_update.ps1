#docs: 
# https://docs.microsoft.com/en-us/rest/api/dns/recordsets/createorupdate
# https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/invoke-restmethod?view=powershell-6
# https://docs.microsoft.com/es-es/azure/active-directory/develop/v1-oauth2-client-creds-grant-flow#request-an-access-token

$tenantId = "00000000-8f63-41df-988d-000000000000"
$appId = "00000000-beee-4c34-a999-000000000000"
$appPassword = "0000000000006YUdAXkYKGA5aVwQzQEMy000000000000"
$resource = "https://management.azure.com"
$suscriptionId = "00000000-534b-456b-864b-000000000000"
$dnsZone = "yourdnszone.com"
$recordType = "A"
$newIP = "131.0.0.1"
$TTL = 3600

#Auth: create app and give management permissions create secret password.
$Url = "https://login.microsoftonline.com/$($tenantId)/oauth2/token"
$OauthBody = @{
    grant_type = "client_credentials"
    client_id = $appId
    client_secret = $appPassword
    resource = $resource 
}
$access = Invoke-RestMethod -Method 'Post' -Uri $url -Body $OauthBody | Select-Object token_type,access_token 
$authorization = "$($access.token_type) $($access.access_token)"

#Change DNS record: Give contribution permission to app. 
$Url = "https://management.azure.com/subscriptions/$($suscriptionId)/resourceGroups/personal/providers/Microsoft.Network/dnsZones/$($dnsZone)/$($recordType)/home?api-version=2018-03-01-preview"

$headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
$headers.Add("Authorization", $authorization)

$json = @{
  properties= @{
    TTL = $TTL
    ARecords= @(
      @{
        ipv4Address= $newIP 
      }
    )
  }
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Method 'Put' -Uri $url -Body $json -Headers $headers -ContentType "application/json"