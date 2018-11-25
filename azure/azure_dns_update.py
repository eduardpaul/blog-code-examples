import requests

resource = "https://management.azure.com"
tenantId = "00000000-8f63-41df-988d-000000000000"
suscriptionId = "00000000-534b-456b-864b-000000000000"
appId = "00000000-beee-4c34-a999-000000000000"
appPassword = "0000000000006YUdAXkYKGA5aVwQzQEMy000000000000"
dnsZone = "yourdnszone.com"
valTTL = 3600

ip_rq = requests.get('https://api.ipify.org')
ip_rq.raise_for_status()
newIP = ip_rq.text

with open("dnsupdate_prev_ip", "a+") as f:
    f.seek(0, 0)
    line = f.readline()
    
    if (line == newIP):
        print 'Same value {0} {1}'.format(line, newIP)
        exit(0)
    else:
        print 'Diff value {0} {1}'.format(line, newIP)
        f.seek(0, 0)
        f.write(newIP)

auth_url = "https://login.microsoftonline.com/{0}/oauth2/token".format(tenantId)
auth_body = {
    "grant_type": "client_credentials",
    "client_id": appId,
    "client_secret": appPassword,
    "resource": resource
}

auth_rq = requests.post(auth_url, data = auth_body)
auth_rq.raise_for_status()
authorization = "{0} {1}".format(auth_rq.json()["token_type"], auth_rq.json()["access_token"])

chARecord_url = "https://management.azure.com/subscriptions/{0}/resourceGroups/personal/providers/Microsoft.Network/dnsZones/{1}/A/home?api-version=2018-03-01-preview".format(suscriptionId, dnsZone)
chARecord_body = {
    "properties": {
        "TTL": valTTL,
        "ARecords": [{
            "ipv4Address": newIP
        }]
    }
}

chARecord_headers = {
    'Authorization': authorization
}

chARecord_rq = requests.put(chARecord_url, headers = chARecord_headers, json = chARecord_body)