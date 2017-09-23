using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;

namespace FunctionApp2
{
    public static class Function3
    {
        [FunctionName("Function3")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var settingdata = ConfigurationSettings.AppSettings["name"];
            
            return req.CreateResponse(HttpStatusCode.OK, "Hello " + settingdata);
        }
    }
}
