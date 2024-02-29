using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using ServiceReference;

// Initialize the HttpClientHandler with the authority and public key to pin
var httpClientHander = new CertPinningByAuthorityHttpClientHander(
    // Authority & Publickey
    new Dictionary<string, string>{ {
        "www.dataaccess.com", 
        "3082018A02820181009B51111A2C1CE3FBC26F62D63AC87BD597E22F3A8080467993F278BBC72C3B9B6E325E63CA93480ADAB8D4903D5C039773E3E06C89996EA2080478A6632395DC23480EDE1127256EBA0C943824DF771984C591A7BC390EA79B908D81A261F3AA7835D9EE837A0C7BF3F7B3B7C7054BA643911BB59D857494A3E71ABA44BCE13FAB02361ADFBB277B77C4F1746B75960829CD04BEF6B6552185FED30623659DCDE757D207746A59A5C0D46AFC50FB4EC8DBA3C22E43D1A0AEBFCAB4EDBA699F1012D57913A281BBDADAADCFD0255BDA64114941AB8E5B7A29BF50699A7949915766967D41372804F37984CABA752F0456A85246954F310AEEC9C1385C3AC4AB747BA04BE83ECCE40AD8847A294BBA89B1F460ED4500D7C11A7128DEA5FE9C26131D20C26BD7D4F11702EF6724555C0C553A6C39254265D8E61912A37A649B5642DE997086FC65EBEF3EAE537421D751D532F4B5A479C3844FA5AE6221266F750EC54059D805F088C1B628FD39F715F3484940847CFE942739D9131A6FD6ADDF930203010001"
    } });

// dotnet host to manage dependencies and lifetime
using var host = Host.CreateDefaultBuilder()
.ConfigureServices(services =>
{
    services.AddHttpClient().ConfigureHttpClientDefaults(sp => sp.ConfigurePrimaryHttpMessageHandler(() => httpClientHander.CreateHandler()));
})
.UseConsoleLifetime().Build();

// later, use DI to get the IHttpMessageHandlerFactory
var httpMessageHandler = host.Services.GetRequiredService<IHttpMessageHandlerFactory>()!;

var wsClient = new NumberConversionSoapTypeClient(new NumberConversionSoapTypeClient.EndpointConfiguration() { });

// Add the endpoint behavior to the client
wsClient.Endpoint.EndpointBehaviors.Add(new CertificatePinningEndpointBehavior(httpMessageHandler));

// Call the service (verify that changing the public key will make the call fail)
var result = await wsClient.NumberToWordsAsync(1234);
Console.WriteLine(result.Body.NumberToWordsResult);

await host.RunAsync();