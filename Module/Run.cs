using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DB_2._0.Module
{
    public class RunModule : ModuleBase<SocketCommandContext>
    {
        // ~run -> [UserName] заставляет 🏃Ваню🏃 бежат
        [Command("Run")]
        public async Task RunAsync([Summary("Ivan run")] SocketUser user = null)
        {
            var userInfo = user ?? Context.User;
            var emoji = new Emoji("\uD83C\uDFC3");
            await ReplyAsync($"{userInfo.Username} заставляет {emoji}Ваню{emoji} бежать");
        }
    }
}
