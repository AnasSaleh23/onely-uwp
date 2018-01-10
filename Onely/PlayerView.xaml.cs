using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Storage.Pickers;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.Foundation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Onely
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    public sealed partial class PlayerView : Page
    {
        private Player player;
        
        private bool ReorderingInitiated = false;
        
        private string[] allowedAudioFileTypes = { ".flac", ".mp3", ".m4a", ".aac", ".wav", ".ogg", ".aif", ".aiff" };
        private string[] allowedImageFileTypes = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };

        private PlaylistReferenceCollection<PlaylistReference> SavedPlaylists;

        public PlayerView()
        {
            this.InitializeComponent();
            player = new Player();
            player.LoadDefaultPlaylist();
            mediaPlayerElement.SetMediaPlayer(player.MediaPlayer);
            SavedPlaylists = PlaylistStatic.GetSavedPlaylists();
            Windows.ApplicationModel.Core.CoreApplication.Suspending += (ss, ee) =>
            {
                player.Playlist.SaveDefault();
            };
        }

        private void Play(int index) => player.Play(index);

        private void Play() => player.Play();

        private void Pause() => player.Pause();

        private void FF() => player.FF();

        private void RW() => player.RW();

        private void TogglePlayPause() => player.TogglePlayPause();

        private void ToggleShuffle() => player.ToggleShuffle();
        
        private void ClearPlaylist() => player.ClearPlaylist();

        private void DeleteItem(int index) => player.DeleteItem(index);

        private void ToggleRepeatMode() => player.ToggleRepeatMode();

        private void ProgressBarSeek(object sender, PointerRoutedEventArgs e)
        {
            var bar = (UIElement)sender;
            PointerPoint pointer = e.GetCurrentPoint(bar);
            Point position = pointer.Position;
            var size = bar.RenderSize;
            var ratio = (position.X / size.Width);
            if (!player.IsPlaying)
                player.PlayProgress = ratio * 100;
            player.SeekFromRatio(ratio);
        }

        private void CursorShowHand()
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void CursorShowArrow()
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void AddFileFilters(ref FileOpenPicker picker)
        {
            foreach(var t in allowedAudioFileTypes)
            {
                picker.FileTypeFilter.Add(t);
            }
            foreach(var t in allowedImageFileTypes)
            {
                picker.FileTypeFilter.Add(t);
            }
        }

        private void AddFileFilters(ref FolderPicker picker)
        {
            foreach (var t in allowedAudioFileTypes)
            {
                picker.FileTypeFilter.Add(t);
            }
            foreach (var t in allowedImageFileTypes)
            {
                picker.FileTypeFilter.Add(t);
            }
        }

        private async void AddFiles()
        {
            var filePicker = new FileOpenPicker();
            AddFileFilters(ref filePicker);

            IReadOnlyList<StorageFile> files = await filePicker.PickMultipleFilesAsync();
            await LoadFiles(files);
        }

        private async void AddFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            AddFileFilters(ref folderPicker);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                await LoadFiles(files);
            }
        }

        private async void Playlist_Drop(object sender, DragEventArgs e)
        {
            if (ReorderingInitiated)
            {
                Playlist_DragItemsCompleted();
                return;
            }
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                IEnumerable<StorageFile> files = null;
                IEnumerable<StorageFile> tmpFiles = null;
                IEnumerable<StorageFolder> folders = items.OfType<StorageFolder>();
                if (folders.Count() > 0)
                {
                    foreach (var folder in folders)
                    {
                        tmpFiles = await folder.GetFilesAsync();
                        if (files == null)
                        {
                            files = tmpFiles;
                        }
                        else
                        {
                            files = files.Concat(tmpFiles);
                        }
                    }
                }
                tmpFiles = items.OfType<StorageFile>();
                if (files == null)
                {
                    files = tmpFiles;
                }
                else
                {
                    files = files.Concat(tmpFiles);
                }
                await LoadFiles(files);
            }
        }

        private async Task LoadFiles(IEnumerable<StorageFile> files)
        {
            if (files != null)
            {
                IEnumerable<StorageFile> audioFiles = files.Where(i => allowedAudioFileTypes.Contains(i.FileType));
                IEnumerable<StorageFile> imageFiles = AlbumCover.GetValidCoverImagesFromFiles(files);
                await player.LoadFilesToPlaylist(audioFiles, imageFiles);
            }
        }

        private void PlaylistItem_DoubleTapped()
        {
            Play(player.TargetIndex);
        }

        private void Playlist_DragOver(object sender, DragEventArgs e)
        {
            if (ReorderingInitiated)
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

        private void Playlist_DragItemsStart()
        {
            ReorderingInitiated = true;
            player.Playlist.PrepareForReorder();
        }

        private void Playlist_DragItemsCompleted()
        {
            ReorderingInitiated = false;
            player.Playlist.SetSelectedIndexAfterReorder();
        }

        private void Panel_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Enter:
                    if (player.TargetIndex > -1)
                    {
                        Play(player.TargetIndex);
                    }
                    break;

                case VirtualKey.Delete:
                    if(player.TargetIndex > -1)
                    {
                        DeleteItem(player.TargetIndex);
                    }
                    break;

                default:
                    break;
            }
        }

        // TODO: save / rename playlists
        private async void OpenSaveDialog()
        {
            await SavePlaylistDialog.ShowAsync();
        }

        private void SavePlaylist(string name)
        {
            if (player.SavePlaylist(name) > -1)
            {
                SavedPlaylists = PlaylistStatic.GetSavedPlaylists();
                return;
            }
            // name already exists
        }

        private void TogglePane()
        {
            FileCommand.IsPaneOpen = !FileCommand.IsPaneOpen;
        }

        private void ToggleShowPlaylists()
        {
            TogglePane();
        }

        private void SavePlaylist(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
