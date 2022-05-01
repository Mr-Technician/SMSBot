
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SMSBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private readonly string discordToken;
        private readonly string twilioSid;
        private readonly string twilioToken;

        /// <summary>
        /// The id of the channel messages will be sent from
        /// </summary>
        private const long channelID = 969974768629592164;
        private const long roleID = 970012640338399242;

        public Program()
        {
            var file = File.ReadAllText("public.json");
            var config = JsonConvert.DeserializeObject<dynamic>(file);
            discordToken = config.discordToken;
            twilioSid = config.twilioSid;
            twilioToken = config.twilioToken;

            var discConfig = new DiscordSocketConfig();
            discConfig.GatewayIntents |= GatewayIntents.GuildMembers;
            discConfig.GatewayIntents |= GatewayIntents.GuildPresences;
            _client = new DiscordSocketClient(discConfig);
        }

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            _client.Log += Log;

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            _client.MessageUpdated += MessageUpdated;
            _client.MessageReceived += MessageRecieved;
            _client.UserJoined += UserJoined;
            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            await user.AddRoleAsync(roleID);
        }

        private async Task MessageRecieved(SocketMessage message)
        {
            IGuild guild = _client.GetGuild(967216808845262878);
            IGuildUser user = await guild.GetUserAsync(248895007849709569);
            Console.WriteLine(user.Status);

            if (message.Channel.Id == 967216810120347711)
                Console.WriteLine(message);

            if (message.Channel.Id == channelID && !message.Author.IsBot && !message.Author.IsWebhook)
            {
                TwilioClient.Init(twilioSid, twilioToken);

                await MessageResource.CreateAsync(
                body: ((message.Author as SocketGuildUser).Nickname ?? message.Author.Username) + ":\n" + message.CleanContent,
                from: new Twilio.Types.PhoneNumber("+18649713170"),
                to: new Twilio.Types.PhoneNumber("+16513033247")
            );
            }
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }

}

