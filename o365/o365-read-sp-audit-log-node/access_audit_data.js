// api documentation: https://docs.microsoft.com/en-us/office/office-365-management-api/
// .net version: https://www.anupams.net/o365-management-api-automate-sharepoint/
// power bi formulas version: https://prathy.com/2018/02/powerbi-audit-log-using-office365-management-api/

// gotchas
// - three permissions currently used for the Office 365 Management Activity API: Read activity data for your organization, Read service health information for your organization, Read Data Loss Prevention (DLP) policy events, including detected sensitive information (only if using DLP)
// - When a subscription is created, it can take up to 12 hours for the first content blobs to become available for that subscription. 
// - Content blobs are available 7 days after the notification of the content blob’s availability.
// - Actions and events contained in the content blobs will not necessarily appear in the order in which they occurred.
// - Each vendor coding against the API has a dedicated quota for request throttling at 60K per minute. Microsoft cannot guarantee a response rate.
// - The webhook must be ready to immediately respond to a validation request after the start operation is executed.
// - Webhooks are being de-emphasized by Microsoft because of the difficulty in debugging and troubleshooting.

const AuthenticationContext = require('adal-node').AuthenticationContext;
const fetch = require('node-fetch');

//remember to create web API app in AAD.
const authorityHostUrl = 'https://login.windows.net';
const tenant = 'tenant_name.onmicrosoft.com'; // AAD Tenant name.
const tenant_id = "b534befa-1e16-47e1-a324-000000000000"; // AAD Tenant id.
const authorityUrl = authorityHostUrl + '/' + tenant;
const applicationId = 'b368fe50-179f-4a39-ad0c-000000000000'; // Application Id of app registered under AAD.
const clientSecret = 'uzLQRYqjQtnQBuPcbDV4iz5+MY9JhMT000000000000='; // Secret generated for app. Read this environment variable.
const resource = 'https://manage.office.com'; // URI that identifies the resource for which the token is valid.
//remember to visit URL for grant access: https://login.windows.net/common/oauth2/authorize?response_type=code&resource=https%3A%2F%2Fmanage.office.com&client_id=<<CLIENT ID>>&redirect_uri=https://localhost

var context = new AuthenticationContext(authorityUrl);
context.acquireTokenWithClientCredentials(resource, applicationId, clientSecret, (err, tokenResponse) => {
    if (err) { console.error(err); return; }

    var authHeaders = { 'Authorization': tokenResponse.tokenType + " " + tokenResponse.accessToken };

    //only once to enable the audit log (body with callback/webhook info is optional). Following calls will return an error. 
    var urlStartAudit = "https://manage.office.com/api/v1.0/" + tenant_id + "/activity/feed/subscriptions/start?contentType=Audit.SharePoint&PublisherIdentifier=" + tenant_id;
    fetch(urlStartAudit, { method: 'post', headers: authHeaders })
        .then(res => res.json())
        .then(j => console.log(j));

    //check if the audit is enabled.
    var urlListSubscriptions = "https://manage.office.com/api/v1.0/" + tenant_id + "/activity/feed/subscriptions/list?PublisherIdentifier=" + tenant_id;
    fetch(urlListSubscriptions, { method: 'get', headers: authHeaders })
        .then(res => res.json())
        .then(j => console.log(j));

    //get content blobs (list of blobs of audit log).
    var urlGetAudit = "https://manage.office.com/api/v1.0/" + tenant_id + "/activity/feed/subscriptions/content?contentType=Audit.SharePoint&PublisherIdentifier=" + tenant_id;
    fetch(urlGetAudit, { method: 'get', headers: authHeaders })
        .then(res => res.json())
        .then(contentBlobs => {
            //to get events, each of content blobs must be downloaded separately. Use PublisherIdentifier to address throttling issue (60k req x minute)
            var blobsWait = [];
            contentBlobs.forEach(blob => {
                blobsWait.push(fetch(blob.contentUri + "?PublisherIdentifier=" + tenant_id, { method: 'get', headers: authHeaders }).then(res => res.json()));
            });
            return Promise.all(blobsWait);
        })
        .then(allEvents => {
            //execute aggregation, filtering, sorting.  
            return allEvents
                .reduce((flat, toFlatten) => { return flat.concat(toFlatten); })
                .filter((e) => { return e.Operation == "FileAccessed" || e.Operation == "FileDownloaded" || e.Operation == "PageViewed"; })
                .sort((e1, e2) => { return e1.CreationTime.localeCompare(e2.CreationTime) })
        })
        .then(events => console.log(events));
});