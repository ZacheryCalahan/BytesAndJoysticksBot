using BytesAndJoysticksBot.Services.HubServices;
using Discord;
using Discord.WebSocket;

namespace BytesAndJoysticksBot.Commands.HubCommands
{
    public class WhitelistCommand
    {
        public static SlashCommandProperties Command = new SlashCommandBuilder()
            .WithName("whitelist")
            .WithDescription("Sets a single user to a whitelist for the voice channel.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to whitelist")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.User))
            .Build();

        public static async Task ExecuteCommand(SocketSlashCommand command)
        {
            // Get needed context
            SocketGuildUser whitelistedUser = (SocketGuildUser) command.Data.Options.First().Value;
            SocketGuildUser channelOwner = command.User as SocketGuildUser;
            SocketTextChannel commandChannel = command.Channel as SocketTextChannel;
            SocketGuild guild = commandChannel.Guild;


            // Check if this is executed by a channel owner
            if (HubChannelHandler.TryGetOwnedHubChannel(channelOwner, out ulong channelId))
            {
                // 1024 == ViewChannel (I assume there is an easier way to do this, but I can't find it. This works though!)
                await guild.GetChannel(channelId).AddPermissionOverwriteAsync(whitelistedUser, new OverwritePermissions(1024ul, 0));
                await command.RespondAsync("Done, " + whitelistedUser.DisplayName + " is allowed.");
            }
            else
            {
                await command.RespondAsync("You're not a channel owner!");
            }

            await Task.CompletedTask;
        }
    }
}