using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace DB_2._0.Service
{
    public class NewUserJoinService
    {
        private DiscordSocketClient _client;
        public NewUserJoinService(DiscordSocketClient client)
        {
            _client = client;
            _client.UserJoined += UserJoined;
        }
        /// <summary>
        /// Когда новый пользователь зайдет на сервер, ему присвоится роль "Need_Some_Help_Here"
        /// </summary>
        /// <param name="user">Новый пользователь</param>
        /// <returns></returns>
        private async Task UserJoined(SocketGuildUser user)
        {
            if (user.Roles.Count == 0)
            {
                var selectedRole = from role in user.Guild.Roles
                                   where role.Name == "Need_Some_Help_Here"
                                   select role;
                if (selectedRole.Count() > 0)
                {
                    await user.AddRoleAsync(selectedRole as IRole);
                }                
            }
        }
    }
}
