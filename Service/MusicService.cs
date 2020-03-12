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
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _lavaNode.OnTrackEnded += OnTrackEnded;
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

            //Бот на новом
            if (!(arg3.VoiceChannel is null))
            {
                var newChannel = arg3.VoiceChannel.Users;
                //Бот на новом канале
                var botNewChannel = from user in newChannel
                                    where user.IsBot
                                    select user;

                if (botNewChannel.Count() > 0)
                {
                    if (_lavaNode.TryGetPlayer(botNewChannel.FirstOrDefault().Guild, out var playerNew))
                    {
                        //if (newChannel.Count > 2)
                        //{
                        //    if (playerNew.PlayerState == PlayerState.Playing)
                        //    {
                        //        await playerNew.TextChannel.SendMessageAsync($"{arg1.Username} зашёл. Трек на паузу");
                        //        await playerNew.PauseAsync();
                        //        return;
                        //    }
                        //}
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
            }
            //Бот на старом
            else if (!(arg2.VoiceChannel is null))
            {
                var oldChannel = arg2.VoiceChannel.Users;
                //Бот на старом канале
                var botOldChannel = from user in oldChannel
                                    where user.IsBot
                                    select user;
                if (botOldChannel.Count() > 0)
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
                await player.TextChannel.SendMessageAsync("Нечего играть");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("Следующий трек это не трек");
                return;
            }

            #region Embed
            var embed = new EmbedBuilder()
                .WithAuthor("Играет")
                .WithTitle(player.Track.Title)
                .WithUrl(player.Track.Url)
                .WithColor(Color.Orange)
                .WithCurrentTimestamp()
                .WithImageUrl(getYouTubeThumbnail(player.Track.Url))
                //
                .Build();
            #endregion

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                $"{args.Reason}: {args.Track.Title}\nСейчас играет: {track.Title}");
        }

        private async Task OnReady()
        {
            await _lavaNode.ConnectAsync();
        }

        private string getYouTubeThumbnail(string YoutubeUrl)
        {
            string youTubeThumb = string.Empty;
            if (YoutubeUrl == "")
                return "";

            if (YoutubeUrl.IndexOf("=") > 0)
            {
                youTubeThumb = YoutubeUrl.Split('=')[1];
            }
            else if (YoutubeUrl.IndexOf("/v/") > 0)
            {
                string strVideoCode = YoutubeUrl.Substring(YoutubeUrl.IndexOf("/v/") + 3);
                int ind = strVideoCode.IndexOf("?");
                youTubeThumb = strVideoCode.Substring(0, ind == -1 ? strVideoCode.Length : ind);
            }
            else if (YoutubeUrl.IndexOf('/') < 6)
            {
                youTubeThumb = YoutubeUrl.Split('/')[3];
            }
            else if (YoutubeUrl.IndexOf('/') > 6)
            {
                youTubeThumb = YoutubeUrl.Split('/')[1];
            }

            return "http://img.youtube.com/vi/" + youTubeThumb + "/mqdefault.jpg";
        }

    }
}