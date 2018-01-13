# Onely UWP

A [Universal Windows Platform](https://docs.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide) version of the [Onely](https://github.com/mphonic/onely) music player, written in C#. Like the original Onely, this is a light, fast, and friendly music playlist player.

### A Playlist Player

The Onely app plays playlists of audio files. You can create a playlist by opening file(s), a folder with audio files in it, or by dragging audio files onto the app window. Playlist order can be changed by dragging items. A playlist can be saved for later, and existing playlists can be added onto the currently active playlist. Typical seek, shuffle, and repeat modes are available. Controls can be activated via keyboard shortcuts (i.e., spacebar to toggle play/pause, shift-right to skip to the next track, etc.).

### Build It

You'll need [Visual Studio](https://www.visualstudio.com/) to build this project. Open Onely.sln, select the appropriate build targets, and build. You may have to install extensions and update NuGet packages. Some understanding of VS is assumed.

### Because

I wanted to learn C# and get some familiarity with the UWP platform. And I like having a clean, efficient, non-intrusive music player that doesn't mess with my files or connect me to online services. And playlist players cover a decent amount of basic ground in app development -- I/O, asynchronous functions, event handling, user input and interaction, DB and / or JSON read / write (Onely uses SQLite), notifications (there's some Toast here), etc.
