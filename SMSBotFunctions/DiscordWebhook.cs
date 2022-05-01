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
using System.Linq;

namespace SMSBotFunctions
{
    public static class DiscordWebhook
    {
        [FunctionName("DiscordWebhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SMS Recieved");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            var query = HttpUtility.ParseQueryString(requestBody).ToDictionary();

            var AccountSid = query["AccountSid"];
            var MessagingServiceSid = query["MessagingServiceSid"];
            var From = query["From"];
            var ValidNumbers = Environment.GetEnvironmentVariable("ValidPhoneNumbers").Split(',');

            if (!(AccountSid == Environment.GetEnvironmentVariable("TwilioAccountSid")
                && MessagingServiceSid == Environment.GetEnvironmentVariable("TwilioMessagingServiceSid")
                && ValidNumbers.Contains(From)))
            {
                return new OkObjectResult(null); //if any of the above do not match do nothing but return Ok so Twilio doesn't complain
            }

            var name = Environment.GetEnvironmentVariable(From);

            var SuccessWebHook = new
            {
                username = name,
                content = query["Body"]
            };

            using var client = new HttpClient();
            string endpoint = string.Format("https://discordapp.com/api/webhooks/{0}", Environment.GetEnvironmentVariable(query["From"]));
            var content = new StringContent(JsonConvert.SerializeObject(SuccessWebHook), Encoding.UTF8, "application/json");
            await client.PostAsync(endpoint, content);

            return new OkObjectResult(null);
        }
    }
}
