using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;


namespace DB_2._0.Service
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly IServiceProvider _service;

        public CommandHandler(DiscordSocketClient client, CommandService command, IServiceProvider service)
        {
            _client = client;
            _command = command;
            _service = service;
        }

        public async Task InstallAsync()
        {
            await _command.AddModulesAsync(Assembly.GetExecutingAssembly(), _service);

            _client.MessageReceived += HandleMessageAsync;
            _client.UserJoined += _client_UserJoined;
        }

        private Task _client_UserJoined(SocketGuildUser arg)
        {
            throw new NotImplementedException();
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            int argPos = 0;
            var msg = arg as SocketUserMessage;
            if (msg.Author.IsBot) return;

            if (!msg.HasCharPrefix('!', ref argPos)) return;

            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, msg);

            var x = await _command.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _service
                );

            if (!x.IsSuccess) Console.WriteLine("ERROR COMMAND");

            await msg.DeleteAsync();
        }
    }
}
