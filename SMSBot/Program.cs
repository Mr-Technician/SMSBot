
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SMSBot
{
    public class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly BotConfig _config;

        public Program()
        {
            _config = BotConfig.Initialize("public.json");

            var discConfig = new DiscordSocketConfig();
            discConfig.GatewayIntents |= GatewayIntents.GuildMembers;
            discConfig.GatewayIntents |= GatewayIntents.GuildPresences;
            _client = new DiscordSocketClient(discConfig);

            TwilioClient.Init(_config.TwilioSid, _config.TwilioToken); //create twilio client
        }

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
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
            await user.AddRoleAsync(_config.JoinRoleId);
        }

        private async Task MessageRecieved(SocketMessage message)
        {
            if (message.Channel.Id == _config.SMSChannelId && (!message.Author.IsBot || message.Author.IsWebhook)) //if this is general chat, not a bot, (unless webhook)
            {
                IGuild guild = _client.GetGuild(_config.GuildId); //get the currint guild
                foreach (var smsLink in _config.SMSLinks.Where(x => x.Enabled)) //loop through enabled links
                {
                    if (message.Author.Id == smsLink.DiscordWebhookID) break; //don't send message if the recipient is the same webhook

                    IGuildUser user = await guild.GetUserAsync(smsLink.DiscordID); //get the send user
                    if (user.Status == UserStatus.Online) break; //don't send message if the user is online

                    await MessageResource.CreateAsync( //send the message
                    body: ((message.Author as SocketGuildUser).Nickname ?? message.Author.Username) + ":\n" + message.CleanContent,
                    from: new Twilio.Types.PhoneNumber(_config.TwilioNumber),
                    to: new Twilio.Types.PhoneNumber(smsLink.Number)
                    );
                }
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

