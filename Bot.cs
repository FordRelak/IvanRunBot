using DB_2._0.Service;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Victoria;

namespace DB_2._0
{
    public class Bot
    {
        private DiscordSocketClient _client;
        private CommandService _command;

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Debug                
            });

            _command = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });


            var serviceProvider = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_command)
                .AddSingleton<LoggingService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<MusicService>()
                .AddSingleton<LavaConfig>()
                .AddSingleton<LavaNode>()
                //
                .BuildServiceProvider();

            serviceProvider.GetRequiredService<LoggingService>();
            serviceProvider.GetRequiredService<MusicService>();
            serviceProvider.GetRequiredService<MusicService>().Install();
            await serviceProvider.GetRequiredService<CommandHandler>().InstallAsync();

            await _client.LoginAsync(TokenType.Bot, System.IO.File.ReadAllText("token.txt"));
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
