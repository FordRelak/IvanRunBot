using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace DB_2._0.Service
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private MusicService _musicService;
        private LavaNode _lavaNode;

        public Music(MusicService musicService, LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            _musicService = musicService;

        }

        [Command("Join")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Уже");
                return;
            }

            var voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Зайди сам сначала");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Ага {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Leave")]
        public async Task LeaveAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("Я не в голосовом");
                return;
            }

            var voiceChannel = (Context.User as IVoiceState).VoiceChannel ?? player.VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyAsync("Not sure which voice channel to disconnect from.");
                return;
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceChannel);
                await ReplyAsync($"Ну вышел из {voiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Play")]
        public async Task PlayAsync([Remainder] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var searchResponse = await _lavaNode.SearchAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    foreach (var track in searchResponse.Tracks)
                    {
                        player.Queue.Enqueue(track);
                    }

                    await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    var track = searchResponse.Tracks[0];
                    player.Queue.Enqueue(track);
                    await ReplyAsync($"Enqueued: {track.Title}");
                }
            }
            else
            {
                var track = searchResponse.Tracks[0];

                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    for (var i = 0; i < searchResponse.Tracks.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(track);
                            await ReplyAsync($"Now Playing: {track.Title}");
                        }
                        else
                        {
                            player.Queue.Enqueue(searchResponse.Tracks[i]);
                        }
                    }

                    await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    await player.PlayAsync(track);
                    await ReplyAsync($"Now Playing: {track.Title}");
                }
            }
        }

        [Command("Pause")]
        public async Task PauseAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("I cannot pause when I'm not playing anything!");
                return;
            }

            try
            {
                await player.PauseAsync();
                await ReplyAsync($"Paused: {player.Track.Title}");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Resume")]
        public async Task ResumeAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                await ReplyAsync("I cannot resume when I'm not playing anything!");
                return;
            }

            try
            {
                await player.ResumeAsync();
                await ReplyAsync($"Resumed: {player.Track.Title}");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Stop")]
        public async Task StopAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("Woaaah there, I can't stop the stopped forced.");
                return;
            }

            try
            {
                await player.StopAsync();
                await ReplyAsync("No longer playing anything.");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Skip")]
        public async Task SkipAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Woaaah there, I can't skip when nothing is playing.");
                return;
            }

            //var voiceChannelUsers = (player.VoiceChannel as SocketVoiceChannel).Users.Where(x => !x.IsBot).ToArray();
            //if (_musicService.VoteQueue.Contains(Context.User.Id))
            //{
            //    await ReplyAsync("You can't vote again.");
            //    return;
            //}

            //_musicService.VoteQueue.Add(Context.User.Id);
            //var percentage = _musicService.VoteQueue.Count / voiceChannelUsers.Length * 100;
            //if (percentage < 85)
            //{
            //    await ReplyAsync("You need more than 85% votes to skip this song.");
            //    return;
            //}
            try
            {
                var oldTrack = player.Track;
                var currenTrack = await player.SkipAsync();
                await ReplyAsync($"Skipped: {oldTrack.Title}\nNow Playing: {currenTrack.Title}");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Seek")]
        public async Task SeekAsync(string time)
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Woaaah there, I can't seek when nothing is playing.");
                return;
            }

            var str = time.Split('.');
            str = str.Reverse().ToArray();
            TimeSpan timeSpan = TimeSpan.Zero;
            switch (str.Length)
            {
                case 1:
                    timeSpan = TimeSpan.FromSeconds(Convert.ToDouble(Convert.ToInt32(str[0])));
                    break;
                case 2:
                    timeSpan = TimeSpan.FromSeconds(Convert.ToDouble(Convert.ToInt32(str[0]) + Convert.ToInt32(str[1]) * 60));
                    break;
                case 3:
                    timeSpan = TimeSpan.FromSeconds(Convert.ToDouble(Convert.ToInt32(str[0]) + Convert.ToInt32(str[1]) * 60) + Convert.ToInt32(str[2]) * 360);
                    break;
                default:
                    break;
            }



            try
            {
                await player.SeekAsync(timeSpan);
                await ReplyAsync($"I've seeked `{player.Track.Title}` to {timeSpan}.");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Volume")]
        public async Task VolumeAsync(ushort volume)
        {
            if (Context.User != Context.Guild.Owner)
            {
                await ReplyAsync($"У {Context.User} нет прав!");
                return;
            }

            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            try
            {
                await player.UpdateVolumeAsync(volume);
                await ReplyAsync($"I've changed the player volume to {volume}.");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

    }
}
