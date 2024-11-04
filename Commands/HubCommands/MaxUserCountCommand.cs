using BytesAndJoysticksBot.Services.HubServices;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytesAndJoysticksBot.Commands.HubCommands
{
    public class MaxUserCountCommand
    {
        public static SlashCommandProperties Command = new SlashCommandBuilder()
            .WithName("setusercount")
            .WithDescription("Sets the channels max participants.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("maxusers")
                .WithDescription("The maximum amount of users")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Integer))
            .Build();

        public static async Task ExecuteCommand(SocketSlashCommand command)
        {
            SocketGuildUser channelOwner = command.User as SocketGuildUser;
            SocketTextChannel commandChannel = command.Channel as SocketTextChannel;
            SocketGuild guild = commandChannel.Guild;

            // Check to see if the user 
            if (HubChannelHandler.TryGetOwnedHubChannel(channelOwner, out ulong channelId))
            {
                SocketVoiceChannel voiceChannel = (SocketVoiceChannel) guild.GetChannel(channelId);
                int maxUsers = Convert.ToInt32(command.Data.Options.First().Value);
                await voiceChannel.ModifyAsync(x => x.UserLimit = maxUsers);

                await command.RespondAsync("Done. Max users set to: " + maxUsers);

            }
            else
            {
                await command.RespondAsync("You do not own any channels.");
            }
        }
    }
}
