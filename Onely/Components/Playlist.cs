using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Onely
{
    public class Playlist
    {
        public int Id = -1;
        public string Name = String.Empty;
        public ObservableCollection<PlaylistItem> Items { get; set; }
        private List<AlbumCover> AlbumCovers;
        public List<int> RandomIndexes { get; set; }
        public int SelectedIndex { get; set; }
        public int RandomIndex {get; set; }
        public PlaylistItem HeldItem;
        public PlaylistItem SelectedItem {
            get {
                if (SelectedIndex > -1) 
                    try {
                        return this.Items[SelectedIndex];
                    } catch(Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                return null;
            }
        }
        private bool shuffle;
        public bool Shuffle
        {
            get
            {
                return this.shuffle;
            }
            set
            {
                if (value && value != this.shuffle)
                {
                    this.GenerateRandomIndexes();
                }
                this.shuffle = value;
            }
        }

        public Playlist()
        {
            Items = new ObservableCollection<PlaylistItem>();
            SelectedIndex = 0;
            RandomIndex = 0;
            HeldItem = null;
            Shuffle = false;
            RandomIndexes = new List<int>();
            AlbumCovers = new List<AlbumCover>();
        }

        public Playlist(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public void PrepareForReorder()
        {
            HeldItem = SelectedItem;
        }

        public void SetSelectedIndexAfterReorder()
        {
            if (HeldItem != null)
            {
                SelectedIndex = Items.IndexOf(HeldItem);
                HeldItem = null;
            }
        }

        public void Add(PlaylistItem item)
        {
            Items.Add(item);
        }

        public void RemoveAt(int index)
        {
            var item = Items[index];
            item.Source.Dispose();
            Items.RemoveAt(index);
            if (SelectedIndex > index)
            {
                SelectedIndex--;
            }
            if (Shuffle)
            {
                RandomIndexes.RemoveAt(RandomIndexes.IndexOf(index));
                foreach (var ind in RandomIndexes.Select((value, i) => new { i, value }))
                {
                    if (ind.value > index)
                    {
                        RandomIndexes[ind.i] = ind.value - 1;
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var i in Items)
            {
                i.Source.Dispose();
            }
            Items.Clear();
            AlbumCovers.Clear();
        }

        public PlaylistItem SetCurrentPositionAndGetItem(int index)
        {
            try
            {
                var item = Items[index];
                SelectedIndex = index;
                return item;
            } catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
                return null;
            }
        }

        public int GetNextIndex()
        {
            int next;
            if (Shuffle)
            {
                next = RandomIndex + 1;
                if (next < RandomIndexes.Count)
                {
                    RandomIndex = next;
                    return RandomIndexes[next];
                }
                return -1;
            }
            next = SelectedIndex + 1;
            if (next < Items.Count())
                return next;
            return -1;
        }

        public int GetPreviousIndex()
        {
            int prev;
            if (Shuffle)
            {
                prev = RandomIndex - 1;
                if (prev > -1)
                {
                    RandomIndex = prev;
                    return RandomIndexes[prev];
                } 
                return -1;
            }
            prev = SelectedIndex - 1;
            if (prev > -1)
                return prev;
            return -1;
        }

        public int Reset()
        {
            RandomIndex = 0;
            if (Shuffle)
            {
                GenerateRandomIndexes();
                return RandomIndexes[0];
            }
            return 0;
        }

        private void GenerateRandomIndexes()
        {
            if (Items.Count() < 1)
                return;
            var list = new List<int>(Enumerable.Range(0, Items.Count() - 1));
            // This is a cheap and not totally random way to do this, but it works ok
            RandomIndexes = list.OrderBy(a => Guid.NewGuid()).ToList();
        }

        private AlbumCover GetExistingAlbumCover(string path)
        {
            foreach(var i in AlbumCovers)
            {
                if (i.CoverPath == path)
                    return i;
            }
            return null;
        }

        public async Task<bool> LoadDefault()
        {
            var id = PlaylistStatic.GetDefaultPlaylistId();
            return await ClearAndLoadNew(id);
        }

        public async Task<bool> LoadNew(int id)
        {
            var name = PlaylistStatic.RetrieveNameById(id);
            if (name == null)
                return false;
            Name = name;
            Id = id;
            List<PlaylistItem> items = await PlaylistItemStatic.RetrievePlaylistItemsByPlaylistId(id);
            if (items == null)
                return true;

            foreach (var item in items)
            {
                Add(item);
            }
            if (Shuffle)
            {
                GenerateRandomIndexes();
            }
            return true;
        }

        public async Task<bool> ClearAndLoadNew(int id)
        {
            Clear();
            return await LoadNew(id);
        }

        public async Task<bool> LoadFiles(IEnumerable<StorageFile> audioFiles, IEnumerable<StorageFile> imageFiles)
        {
            AlbumCover cover;
            StorageFile imageFile = imageFiles.FirstOrDefault();
            if (imageFile != null)
            {
                cover = GetExistingAlbumCover(imageFile.Path);
                if (cover == null)
                {
                    cover = await AlbumCover.FromStorageFile(imageFile);
                    if (cover != null)
                    {
                        AlbumCovers.Add(cover);
                        string faToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(imageFile);
                    }
                }
            } else
            {
                cover = null;
            }

            foreach (var file in audioFiles)
            {
                var item = await PlaylistItemStatic.LoadFromFile(file);

                Add(item);
                string faToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                if (cover != null)
                {
                    item.MainCover = cover;
                }
            }
            if (Shuffle)
            {
                GenerateRandomIndexes();
            }
            return true;
        }

        public int Save(string name)
        {
            return PlaylistStatic.Save(this, name);
        }

        public void SaveDefault()
        {
            var currentId = Id;
            var currentName = Name;
            Id = PlaylistStatic.GetDefaultPlaylistId();
            Save("Default");
            Id = currentId;
            currentName = Name;
        }
    }
}
