using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class CertPinningByAuthorityHttpClientHander(Dictionary<string, string> authorityToPublicKeyMap, bool pinnedOnly = false)
{
    public HttpClientHandler CreateHandler()
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (HttpRequestMessage hrqMessage,
                X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) =>
            {
                // If there is something wrong, don't trust it.
                if (sslPolicyErrors != SslPolicyErrors.None || cert == null)
                {
                    return false;
                }

                // if the authority is in the dictionary (pinned), check the public key
                var authorityIsPinned = authorityToPublicKeyMap.TryGetValue(hrqMessage.RequestUri?.Authority ?? "", out var expectedPublicKey);
                if (authorityIsPinned)
                {
                    return cert?.GetPublicKeyString().Equals(expectedPublicKey, StringComparison.InvariantCultureIgnoreCase) ?? false;
                }

                return !pinnedOnly; // certificate is valid and not pinned (other public ws)
            }
        };
    }
}