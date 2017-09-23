using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Xrm.Sdk;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System;
using System.Configuration;

namespace FunctionApp2
{
    public static class Function1
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, 
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            
            var clientId = ConfigurationSettings.AppSettings["D365ClientId"];
            var clientSecret = ConfigurationSettings.AppSettings["D365ClientSecret"];
            var resourceUrl = ConfigurationSettings.AppSettings["D365ResourceUrl"];

            var tenantId = ConfigurationSettings.AppSettings["D365TenantId"];

            var azureAdConnect = new AzureAdConnect(
                clientId,
                clientSecret,
                resourceUrl,
                tenantId
                );

            var token = azureAdConnect.GetAccessToken();

            token.Wait();
            
            return req.CreateResponse(HttpStatusCode.Accepted, $"Get Token {token.Result.access_token}");
        }
    }


    /// <summary>
    /// AzureADからTokenを取得するためのClass
    /// client_credentials Type
    /// </summary>
    public class AzureAdConnect
    {
        private string _clientId { get; set; }
        private string _clientSecret { get; set; }
        private string _recourceUrl { get; set; }
        private string _tenantId { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AzureAdConnect(string clientId, string clientSecret, string recourceUrl, string tenantId)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _recourceUrl = recourceUrl;
            _tenantId = tenantId;
        }

        /// <summary>
        /// grant_type=passwordでTokenを取得
        /// </summary>
        /// <returns></returns>
        public async Task<AzureAccessToken> GetAccessToken()
        {
            var token = new AzureAccessToken();

            string oauthUrl = string.Format("https://login.microsoftonline.com/{0}/oauth2/token",
                _tenantId);

            string reqBody = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&resource={2}",
                Uri.EscapeDataString(_clientId),
                Uri.EscapeDataString(_clientSecret),
                Uri.EscapeDataString(this._recourceUrl));

            var client = new HttpClient();
            var content = new StringContent(reqBody);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            using (HttpResponseMessage response = await client.PostAsync(oauthUrl, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    var serializer = new DataContractJsonSerializer(typeof(AzureAccessToken));
                    var json = await response.Content.ReadAsStreamAsync();
                    token = (AzureAccessToken)serializer.ReadObject(json);
                }
            }

            return token;
        }
    }

    /// <summary>
    /// AccessToken等の格納クラス
    /// </summary>
    [DataContract]
    public class AzureAccessToken
    {
        [DataMember]
        public string access_token { get; set; }

        [DataMember]
        public string token_type { get; set; }

        [DataMember]
        public string expires_in { get; set; }

        [DataMember]
        public string expires_on { get; set; }

        [DataMember]
        public string resource { get; set; }
    }
}
