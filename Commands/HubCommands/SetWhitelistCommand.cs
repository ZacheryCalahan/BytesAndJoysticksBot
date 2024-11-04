using BytesAndJoysticksBot.Services.HubServices;
using Discord;
using Discord.WebSocket;

namespace BytesAndJoysticksBot.Commands.HubCommands
{
    public static class SetWhitelistCommand
    {
        public static SlashCommandProperties Command = new SlashCommandBuilder()
            .WithName("setwhitelist")
            .WithDescription("Sets channel to be whitelist only.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("iswhitelist")
                .WithDescription("Whitelist true/false")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Boolean))
            .Build();

        public static async Task ExecuteCommand(SocketSlashCommand command)
        {
            bool isWhitelist = (bool) command.Data.Options.First().Value;
            SocketGuildUser channelOwner = command.User as SocketGuildUser;
            SocketTextChannel commandChannel = command.Channel as SocketTextChannel;
            SocketGuild guild = commandChannel.Guild;

            // Check to see if the user 
            if (HubChannelHandler.TryGetOwnedHubChannel(channelOwner, out ulong channelId))
            {
                if (isWhitelist)
                {
                    // Set permissions so everyone but the channel owner can't see the channel.
                    List<Overwrite> permissions = new List<Overwrite>
                {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)),
                    new Overwrite(channelOwner.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow))
                };

                    await guild.GetChannel(channelId).ModifyAsync(x => x.PermissionOverwrites = permissions);
                    await command.RespondAsync("Channel set to whitelist only.");
                }
                else
                {
                    // Set permissions so everyone can see the channel (default state of the channel)
                    List<Overwrite> permissions = new List<Overwrite>
                {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow))
                };
                    await guild.GetChannel(channelId).ModifyAsync(x => x.PermissionOverwrites = permissions);
                    await command.RespondAsync("Channel set to public");
                }

            }
            else
            {
                await command.RespondAsync("You do not own any channels.");
            }
        }
    }
}