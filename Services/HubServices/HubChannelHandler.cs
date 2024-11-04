using Discord;
using Discord.WebSocket;

namespace BytesAndJoysticksBot.Services.HubServices
{
    public static class HubChannelHandler
    {
        // List of created channels
        private static Dictionary<ulong, ulong> customChannelPairs = new Dictionary<ulong, ulong>(); // VoiceChannelId, CommandChannelId
        public static Dictionary<SocketGuildUser, ulong> UserOwnedChannels = new Dictionary<SocketGuildUser, ulong>();

        public static async Task OnHubJoined(SocketGuildUser user, DiscordSocketClient client)
        {
            // Get Guild
            SocketGuild guild = user.Guild;

            // Create a name for the channel, and find the categoryId for where it belongs
            string channelName = user.DisplayName + "'s Voice Channel";
            string commandChannelName = user.DisplayName + "-command-channel";
            var categoryId = guild.CategoryChannels.FirstOrDefault(category => category.Name.Equals("Hub Channels"))?.Id;

            if (categoryId == null)
            {
                Console.WriteLine("Error, no hub category. (Fix to a log)");
                return;
            }

            // Create new voice channel
            var voiceChannel = await guild.CreateVoiceChannelAsync(channelName, prop => prop.CategoryId = categoryId);
            ulong VoiceChannelId = voiceChannel.Id;

            // Create text channel for commands, and set it to only be visible for the user.
            List<Overwrite> permissions = new List<Overwrite>
            {
                new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)),
                new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow))
            };
            var commandChannel = await guild.CreateTextChannelAsync(commandChannelName, prop =>
            {
                prop.CategoryId = categoryId;
                prop.PermissionOverwrites = permissions;
            });
            ulong CommandChannelId = commandChannel.Id;

            // Tie both channels together, so when voice channel is empty both channels are deleted.
            customChannelPairs.Add(VoiceChannelId, CommandChannelId);
            UserOwnedChannels.Add(user, VoiceChannelId);

            // Connect user to created channel
            await user.ModifyAsync(x => x.Channel = Optional.Create(voiceChannel as IVoiceChannel));

            await Task.CompletedTask;

            // Put configuration message in text channel
            await commandChannel.SendMessageAsync("Use the included commands to change things about the channel");
        }

        public static async Task OnChannelLeave(SocketVoiceChannel channelLeft)
        {
            // Check to see if channel is of a created one, and all users have left
            if (customChannelPairs.ContainsKey(channelLeft.Id))
            {
                if (channelLeft.ConnectedUsers.Count == 0)
                {
                    // Get the corresponding text channel
                    ulong textChannelId;
                    if (!customChannelPairs.TryGetValue(channelLeft.Id, out textChannelId))
                    {
                        Console.WriteLine("Error, no corresponding text channel found.");
                        return;
                    }

                    var textChannel = channelLeft.Guild.GetChannel(textChannelId);

                    // Delete channels
                    await channelLeft.DeleteAsync();
                    await textChannel.DeleteAsync();
                }
            }
        }

        public static bool TryGetOwnedHubChannel(SocketGuildUser user, out ulong voiceChannelId)
        {
            if (UserOwnedChannels.TryGetValue(user, out ulong channelId))
            {
                voiceChannelId = channelId;
                return true;
            }
            voiceChannelId = 0;
            return false;
        }
    }
}