using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBot
{
    internal class BotConfig
    {
        public string DiscordToken { get; init; }
        public string TwilioSid { get; init; }
        public string TwilioToken { get; init; }
        public string TwilioNumber { get; init; }
        public ulong GuildId { get; init; }
        public ulong SMSChannelId { get; init; }
        public ulong JoinRoleId { get; init; }
        public List<SMSLink> SMSLinks { get; init; }

        public static BotConfig Initialize(string filePath)
        {
            var file = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<BotConfig>(file);
        }        
    }

    internal class SMSLink
    {
        public string Number { get; init; }
        public ulong DiscordWebhookID { get; init; }
        public ulong DiscordID { get; init; }
        public bool Enabled { get; init; }
    }
}
