using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Web;
using System.Net.Http;
using System.Text;

namespace SMSBotFunctions
{
    public static class DiscordWebhook
    {
        [FunctionName("DiscordWebhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var WebHookId = Environment.GetEnvironmentVariable("WebhookID");
            var WebHookToken = Environment.GetEnvironmentVariable("WebhookToken");

            log.LogInformation("SMS Recieved");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            var query = HttpUtility.ParseQueryString(requestBody).ToDictionary();

            var SuccessWebHook = new
            {
                username = query["From"],
                content = query["Body"]
            };

            using var client = new HttpClient();
            string endpoint = string.Format("https://discordapp.com/api/webhooks/{0}/{1}", WebHookId, WebHookToken);
            var content = new StringContent(JsonConvert.SerializeObject(SuccessWebHook), Encoding.UTF8, "application/json");
            await client.PostAsync(endpoint, content);

            return new OkObjectResult(null);
        }
    }
}
