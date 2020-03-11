using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace DB_2._0.Service
{
    public class MusicService
    {
        private readonly LavaNode _lavaNode;
        private readonly DiscordSocketClient _client;

        public readonly HashSet<ulong> VoteQueue;

        public MusicService(LavaNode lavaNode, DiscordSocketClient client)
        {
            _client = client;
            _lavaNode = lavaNode;

            VoteQueue = new HashSet<ulong>();
        }

        public void Install()
        {
            _client.Ready += OnReady;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        }

        //Если ты останешься один пусть играет трек
        private async Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (arg1.IsBot)
            {
                return;
            }

            if (arg2.VoiceChannel == arg3.VoiceChannel)
            {
                return;
            }

            var oldChannel = arg2.VoiceChannel.Users;
            var newChannel = arg3.VoiceChannel.Users;

            //Бот на новом канале
            var botNewChannel = from user in newChannel
                                where user.IsBot
                                select user;
            //Бот на старом канале
            var botOldChannel = from user in oldChannel
                                where user.IsBot
                                select user;

            //Бот на новом
            if (botNewChannel.Count() > 0)
            {
                if (_lavaNode.TryGetPlayer(botNewChannel.FirstOrDefault().Guild, out var playerNew))
                {
                    if (newChannel.Count > 2)
                    {
                        if (playerNew.PlayerState == PlayerState.Playing)
                        {
                            await playerNew.TextChannel.SendMessageAsync($"{arg1.Username} зашёл. Трек на паузу");
                            await playerNew.PauseAsync();
                            return;
                        }
                    }
                    if (newChannel.Count == 2)
                    {
                        if (playerNew.PlayerState == PlayerState.Paused)
                        {
                            await playerNew.TextChannel.SendMessageAsync($"Возобновляю  {playerNew.Track.Title}");
                            await playerNew.ResumeAsync();
                            return;
                        }
                    }
                }
            }
            else if (botOldChannel.Count() > 0)
            {
                if (_lavaNode.TryGetPlayer(botOldChannel.FirstOrDefault().Guild, out var playerOld))
                {
                    if (oldChannel.Count == 1)
                    {
                        if (playerOld.PlayerState == PlayerState.Playing)
                        {
                            await playerOld.TextChannel.SendMessageAsync($"Ставлю на паузу {playerOld.Track.Title}");
                            await playerOld.PauseAsync();
                            return;
                        }
                    }
                    if (oldChannel.Count == 2)
                    {
                        if (playerOld.PlayerState == PlayerState.Paused)
                        {
                            await playerOld.TextChannel.SendMessageAsync($"Возобновляю  {playerOld.Track.Title}");
                            await playerOld.ResumeAsync();
                            return;
                        }
                    }
                    else return;
                }
            }

            return;
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
                return;

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("No more tracks to play.");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }
            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                $"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }

        private async Task OnReady()
        {
            await _lavaNode.ConnectAsync();
        }
    }
}