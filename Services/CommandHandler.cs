using BytesAndJoysticksBot.Commands.HubCommands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace BytesAndJoysticksBot.Services
{
    public static class CommandHandler
    {
        // Registers commands to the Discord API
        public static async Task RegisterCommands(DiscordSocketClient client, AppSettings settings)
        {

            try
            {
                var whitelistCommand = await client.Rest.CreateGuildCommand(WhitelistCommand.Command, settings.GuildID);
                var setWhitelistCommand = await client.Rest.CreateGuildCommand(SetWhitelistCommand.Command, settings.GuildID);
                var maxUserCountCommand = await client.Rest.CreateGuildCommand(MaxUserCountCommand.Command, settings.GuildID);
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public static async Task HandleCommand(SocketSlashCommand command)
        {
            switch (command.CommandName)
            {
                case "whitelist":
                    await WhitelistCommand.ExecuteCommand(command);
                    break;
                case "setwhitelist":
                    await SetWhitelistCommand.ExecuteCommand(command);
                    break;
                case "setusercount":
                    await MaxUserCountCommand.ExecuteCommand(command);
                    break;

            }
        }


    }
}