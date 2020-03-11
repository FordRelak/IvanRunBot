using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DB_2._0.Module
{
    public class Clear : ModuleBase<SocketCommandContext>
    {
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [Command("Clear")]
        public async Task ClearAsync()
        {
            var msgs = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(msgs);
        }
    }
}
