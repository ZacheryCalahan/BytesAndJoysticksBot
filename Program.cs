using BytesAndJoysticksBot.Services;
using BytesAndJoysticksBot.Services.HubServices;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace BytesAndJoysticksBot
{
    public class Program
    {
        private static AppSettings? settings;

        private static DiscordSocketClient _client;

        public static async Task Main()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;

            // Get app settings from JSON
            settings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(@"appsettings.json"));
            if (settings == null)
            {
                await Log(new LogMessage(LogSeverity.Critical, "", "Could not find \"appsettings.json\"."));
                return;
            }

            // Log bot into Discord
            await _client.LoginAsync(TokenType.Bot, settings.Token);
            await _client.StartAsync();
            await _client.SetActivityAsync(new Game("Creating a new me!"));

            // Bind events to handlers
            _client.Ready += ClientReady;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.UserVoiceStateUpdated += OnVoiceJoin;

            // Block the task until the program is closed.
            await Task.Delay(-1);
        }

        private static async Task OnVoiceJoin(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
        {
            // Get Voice channels
            SocketVoiceChannel channelLeft = state1.VoiceChannel;
            SocketVoiceChannel channelJoined = state2.VoiceChannel;

            // Safety check
            if (user == null)
            {
                await Log(new LogMessage(LogSeverity.Error, "OnVoiceJoin", "User is null"));
                return;
            }

            if (channelJoined != null)
            {
                // Only handle when channel joined is the Hub, else ignore.
                if (channelJoined.Name == "Hub (Join for Voice Channel)")
                {
                    await HubChannelHandler.OnHubJoined(user as SocketGuildUser, _client);
                }
            }

            if (channelLeft != null)
            {
                await HubChannelHandler.OnChannelLeave(channelLeft);
            }
        }

        private async static Task SlashCommandHandler(SocketSlashCommand command)
        {
            Console.WriteLine("Handling command: " + command.Data.Name);
            await CommandHandler.HandleCommand(command);
        }

        private static async Task HandleEchoCommand(SocketSlashCommand command)
        {
            string? echoString = command.Data.Options.First().Value.ToString();
            await command.RespondAsync(echoString, ephemeral: true);
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static async Task ClientReady()
        {
            // Get rid of old commands!
            await _client.BulkOverwriteGlobalApplicationCommandsAsync(new List<ApplicationCommandProperties>().ToArray());

            await CommandHandler.RegisterCommands(_client, settings);
            await Log(new LogMessage(LogSeverity.Info, "Client", "Client is ready"));
            return;
        }
    }
}