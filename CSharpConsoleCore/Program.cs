using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace CSharpConsoleCore
{
    class Program
    {
        private const string clientId = "FILL-IN-HERE";
        private const string clientSecret = "FILL-IN-HERE";
        private const string oauthEndpoint = "FILL-IN-HERE";
        private const string domain = "FILL-IN-HERE";
        private const string account = "FILL-IN-HERE";

        private static async Task CreateDirAsyncREST(string storageAccountName, string tokenStr)
        {

            // Construct the URI. This will look like this:
            //   https://myaccount.blob.core.windows.net/resource
            Guid guid = Guid.NewGuid();
            String uri = $"https://{storageAccountName}.dfs.preprod.core.windows.net/gen1/test1{guid.ToString()}?resource=directory";

            // Set this to whatever payload you desire. Ours is null because 
            //   we're not passing anything in.
            Byte[] requestPayload = null;

            //Instantiate the request message with a null payload.
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            { Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload) })
            {

                // Add the request headers for x-ms-date and x-ms-version.
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", "2018-06-17");
                // If you need any additional headers, add them here before creating
                //   the authorization header. 

                // Add the authorization header.
                httpRequestMessage.Headers.Add("Authorization", tokenStr);

                // Send the request.
                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage, CancellationToken.None))
                {
                    // If successful (status code = 200), 
                    //   parse the XML response for the container names.
                    if (httpResponseMessage.StatusCode == HttpStatusCode.Created)
                    {
                        Console.WriteLine("Successfully created directory");
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                // Acquire token and create client using client secret and client id
                var creds = new ClientCredential(clientId, clientSecret);
                var serviceSettings = ActiveDirectoryServiceSettings.Azure;
                serviceSettings.TokenAudience = new Uri("https://datalake.azure.net/");
                serviceSettings.AuthenticationEndpoint = new Uri(oauthEndpoint);
                ServiceClientCredentials clientCreds2 = ApplicationTokenProvider.LoginSilentAsync(domain, creds).GetAwaiter().GetResult();
                HttpRequestMessage request = new HttpRequestMessage();
                clientCreds2.ProcessHttpRequestAsync(request, CancellationToken.None).Wait();
                string tokenStr = request.Headers.Authorization.ToString();

                CreateDirAsyncREST(account, tokenStr).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
