using TagLibUWP;
using Windows.Media.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Onely
{
    public class PlaylistItem
    {
        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public string Genre { get; }
        public uint Track { get; }
        public BitmapImage Cover { get; set; }
        private AlbumCover mainCover;
        public AlbumCover MainCover {
            get => this.mainCover;
            set
            {
                this.SetAlbumCover(ref value);
            }
        }
        public MediaSource Source { get; }
        public string Path { get; }

        public PlaylistItem(string p, MediaSource s, Tag t, BitmapImage c)
        {
            Path = p;
            Source = s;
            Title = t.Title;
            Artist = t.Artist;
            Album = t.Album;
            Track = t.Track;
            Genre = t.Genre;
            Cover = c;
        }

        public void SetAlbumCover(ref AlbumCover a)
        {
            mainCover = a;
            if (Cover == null)
            {
                Cover = mainCover.Cover;
            }
        }
    }
}
