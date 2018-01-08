using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;

namespace Onely
{

    public class Player :  NotificationBase
    {
        private MediaPlayer player;
        public MediaPlayer MediaPlayer
        {
            get => this.player;
        }
        public Playlist Playlist { get; }

        private double playProgress;
        public double PlayProgress
        {
            get => this.playProgress;
            set
            {
                SetProperty(this.playProgress, value, () => this.playProgress = value);
            }
        }

        private PlaylistItem nowPlaying;
        public PlaylistItem NowPlaying
        {
            get => this.nowPlaying;
            set
            {
                SetProperty(this.nowPlaying, value, () => this.nowPlaying = value);
            }
        }

        private int targetIndex = -1;
        public int TargetIndex
        {
            get => this.targetIndex;
            set
            {
                SetProperty(this.targetIndex, value, () => this.targetIndex = value);
            }
        }

        private bool isPlaying = false;
        public bool IsPlaying
        {
            get => this.isPlaying;
            set
            {
                SetProperty(this.isPlaying, value, () => this.isPlaying = value);
            }
        }

        private bool isWaiting = false;
        public bool IsWaiting
        {
            get => this.isWaiting;
            set
            {
                SetProperty(this.isWaiting, value, () => this.isWaiting = value);
            }
        }

        private int repeatMode = 0;
        public int RepeatMode
        {
            get => this.repeatMode;
            set
            {
                SetProperty(this.repeatMode, value, () => this.repeatMode = value);
            }
        }

        public Player()
        {
            player = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.Media
            };
            player.MediaEnded += OnTrackEnd;
            player.PlaybackSession.PlaybackStateChanged += OnStateChange;
            player.PlaybackSession.PositionChanged += UpdateProgress;
            Playlist = new Playlist();
        }

        public void Play(int index)
        {
            if (index != Playlist.SelectedIndex)
            {
                player.Source = Playlist.SetCurrentPositionAndGetItem(index).Source;
            }
            Play();
        }

        public void Play()
        {
            player.Play();
        }

        public void Pause()
        {
            player.Pause();
        }

        public void FF()
        {
            if (!Advance())
            {
                Play(Playlist.Reset());
            }
        }

        private bool Advance()
        {
            var next = Playlist.GetNextIndex();
            if (next > -1)
            {
                Play(next);
                return true;
            }
            if (RepeatMode == 1)
            {
                Play(Playlist.Reset());
                return true;
            }
            return false;
        }

        public void RW()
        {
            var session = player.PlaybackSession;
            if ((session.Position.TotalSeconds > 2) || (Playlist.SelectedIndex == 0))
            {
                session.Position = TimeSpan.FromSeconds(0);
                return;
            }
            var prev = Playlist.GetPreviousIndex();
            if (prev > -1)
            {
                Play(prev);
            }
        }

        public void TogglePlayPause()
        {
            if (player.PlaybackSession.PlaybackState.Equals(MediaPlaybackState.Playing))
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void SeekFromRatio(double ratio)
        {
            var session = player.PlaybackSession;
            session.Position = TimeSpan.FromSeconds(ratio * session.NaturalDuration.TotalSeconds);
        }

        private void OnTrackEnd(MediaPlayer sender, object args)
        {
            if (RepeatMode != 2)
            {
                if (!Advance())
                {
                    Pause();
                }
            }
            else
            {
                RW();
                Play();
            }
        }

        private void OnStateChange(MediaPlaybackSession sender, object args)
        {
            switch (sender.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    IsPlaying = true;
                    IsWaiting = false;
                    TargetIndex = Playlist.SelectedIndex;
                    NowPlaying = Playlist.SelectedItem;
                    break;
                case MediaPlaybackState.Paused:
                    IsPlaying = false;
                    IsWaiting = false;
                    break;
                case MediaPlaybackState.Buffering:
                    IsWaiting = true;
                    IsPlaying = false;
                    break;
                case MediaPlaybackState.Opening:
                    IsWaiting = true;
                    IsPlaying = false;
                    break;
                default:
                    NowPlaying = null;
                    break;
            }
        }

        private void UpdateProgress(MediaPlaybackSession sender, object e)
        {
            if ((player.Source == null))
                return;
            var session = sender;
            if (session.PlaybackState.Equals(MediaPlaybackState.Playing))
            {
                var pos = session.Position.TotalSeconds;
                var dur = session.NaturalDuration.TotalSeconds;
                var percent = (pos / dur) * 100;

                PlayProgress = percent;
            }
        }

        public int ToggleRepeatMode()
        {
            RepeatMode = (RepeatMode + 1) % 3;
            return RepeatMode;
        }

        public void ToggleShuffle()
        {
            Playlist.Shuffle = !Playlist.Shuffle;
        }


        private void ClearPlayer()
        {
            Pause();
            player.Source = null;
            NowPlaying = null;
        }

        public void ClearPlaylist()
        {
            ClearPlayer();
            TargetIndex = -1;
            Playlist.Clear();
        }

        public void DeleteItem(int index)
        {
            if (index == Playlist.SelectedIndex)
            {
                ClearPlayer();
            }
            Playlist.RemoveAt(index);
            if ((player.Source == null) && (Playlist.SelectedItem != null))
            {
                player.Source = Playlist.SelectedItem.Source;
            }
            TargetIndex = Playlist.SelectedIndex;
        }

        public async Task LoadFilesToPlaylist(IEnumerable<StorageFile> audoFiles, IEnumerable<StorageFile> imageFiles = null)
        {
            if (audoFiles.Count() > 0)
            {
                bool loaded = await Playlist.LoadFiles(audoFiles, imageFiles);
                if (loaded && player.Source == null)
                {
                    NowPlaying = Playlist.Items[0];
                    player.Source = NowPlaying.Source;
                    TargetIndex = 0;
                }
            }
        }

        public async void LoadPlaylist(int id)
        {
            bool loaded = await Playlist.LoadNew(id);
            if (loaded)
            {
                UpdateOnPlaylistLoad();
            }
        }

        public async void LoadDefaultPlaylist()
        {
            bool loaded = await Playlist.LoadDefault();
            if (loaded)
            {
                UpdateOnPlaylistLoad();   
            }
        }

        private void UpdateOnPlaylistLoad()
        {
            if (player.Source != null)
                return;
            NowPlaying = Playlist.Items[0];
            TargetIndex = 0;
            if (NowPlaying != null)
            {
                player.Source = NowPlaying.Source;
            }
        }

        public int SavePlaylist(string name)
        {
            return Playlist.Save(name);
        }
    }
}
