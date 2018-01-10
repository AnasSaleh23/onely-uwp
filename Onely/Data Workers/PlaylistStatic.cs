using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace Onely
{
    public static class PlaylistStatic
    {
        public static PlaylistReferenceCollection<PlaylistReference> GetSavedPlaylists()
        {
            using (SqliteConnection db = OnelyDB.Open())
            {

                PlaylistReferenceCollection<PlaylistReference> playlists = new PlaylistReferenceCollection<PlaylistReference>(); 
                SqliteCommand command = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "SELECT id, name FROM playlists WHERE name != 'Default' ORDER BY name"
                };
                var res = OnelyDB.ExecuteReader(command);

                while (res.Read())
                {
                    playlists.Add(new PlaylistReference(res.GetInt32(0), res.GetString(1)));
                }

                db.Close();

                return playlists;
            }
        }

        public static void DeletePlaylistById(int id)
        {
            using (SqliteConnection db = OnelyDB.Open())
            {
                SqliteCommand command = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "DELETE FROM playlist_items WHERE playlist_id=@ID"
                };
                command.Parameters.AddWithValue("@ID", id);
                OnelyDB.ExecuteReader(command);

                PlaylistItemStatic.DeleteBasedOnPlaylistId(id, db);

                OnelyDB.Close(db);
            }
        }

        public static string RetrieveNameById(int id)
        {
            using (SqliteConnection db = OnelyDB.Open())
            {
                SqliteCommand command = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "SELECT name FROM playlists WHERE id=@ID"
                };
                command.Parameters.AddWithValue("@ID", id);
                var res = command.ExecuteReader();

                if (!res.HasRows)
                {
                    OnelyDB.Close(db);
                    return null;
                }
                
                var name = String.Empty;
                while (res.Read())
                {
                    name = res.GetString(0);
                }
                return name;
            }
        }

        public static int Save(Playlist playlist, string name)
        {
            using (SqliteConnection db = OnelyDB.Open())
            {
                SqliteCommand command;
                if (playlist.Id == -1)
                {
                    command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "INSERT INTO playlists(name) VALUES (@Name)"
                    };
                    command.Parameters.AddWithValue("@Name", name);
                    try
                    {
                        command.ExecuteReader();
                    } catch(Exception e)
                    {
                        return -1;
                    }

                    command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "SELECT last_insert_rowid()"
                    };
                    playlist.Id = (int)(long)command.ExecuteScalar();
                }
                else
                {
                    command = new SqliteCommand
                    {
                        Connection = db,
                        CommandText = "UPDATE playlists SET name=@Name WHERE id=@ID"
                    };
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@ID", playlist.Id);
                    OnelyDB.ExecuteReader(command);

                    PlaylistItemStatic.DeleteBasedOnPlaylistId(playlist.Id, db);
                }
                int count = 0;
                foreach (var item in playlist.Items)
                {
                    PlaylistItemStatic.Save(item, playlist.Id, count, db);
                    count++;
                }
                OnelyDB.Close(db);
                return playlist.Id;
            }
        }

        public static int GetDefaultPlaylistId()
        {
            using (SqliteConnection db = OnelyDB.Open())
            {
                int id = -1;
                SqliteCommand command = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "SELECT id FROM playlists WHERE name='Default'"
                };
                var res = OnelyDB.ExecuteReader(command);
                while (res.Read())
                {
                    id = res.GetInt32(0);
                }
                return id;
            }
        }
    }
}
