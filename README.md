# ChapterInjector

**ChapterInjector** is a Jellyfin plugin that enables support for external chapter files (OGM text or Matroska XML) alongside your media files. It allows you to manage chapters independently of the media container, which is useful for situations where re-muxing is not desirable (e.g., seeding torrents).

In addition, it automatically injects a client-side script into the Jellyfin Web UI to fetch and utilize these chapters during playback.

## Features

- **External Chapter Support**: Reads chapters from `.txt` (OGM format) and `.xml` (Matroska format) files.
- **Flexible Naming**: Supports both movie-style and episode-style naming conventions.
- **Client-Side Injection**: Automatically injects a script into the Web Client to fetch chapters on playback.
- **API Endpoint**: Exposes a `GET /ExternalChapters/{ItemId}` endpoint for custom integrations.

## Installation

1.  Download the latest release ZIP.
2.  Extract the contents into your Jellyfin `plugins` directory.
    *   Linux: `/var/lib/jellyfin/plugins/ChapterInjector/`
    *   Windows: `C:\ProgramData\Jellyfin\Server\plugins\ChapterInjector\`
3.  Restart Jellyfin.

## Usage

Simply place your chapter files in the same directory as your video files.

### Naming Conventions

The plugin checks for chapter files in the following priority order:

#### Movies / Generic Videos
For a video file named `MyMovie.mkv`:
1.  `MyMovie.chapters.xml` (Preferred)
2.  `MyMovie.chapters.txt`
3.  `chapters.xml`
4.  `chapters.txt`
5.  `chapter.xml`
6.  `chapter.txt`

#### TV Episodes
For an episode with index `5` (e.g., S01E05):
1.  `5_chapters.xml` (Preferred for Episode 5)
2.  `5_chapters.txt`
3.  (Then falls back to Generic patterns above)

### File Formats

**OGM Text (.txt)**
```ini
CHAPTER01=00:00:00.000
CHAPTER01NAME=Intro
CHAPTER02=00:05:00.000
CHAPTER02NAME=The Heist
```

**Matroska XML (.xml)**
Standard `matroska_chapters.xml` format.

## Building from Source

Requirements:
- .NET 9.0 SDK

```bash
# Clone the repository
git clone https://github.com/BetaZay/ChapterInjector.git
cd ChapterInjector

# Build
dotnet build ChapterInjector.sln
```

## How It Works

1.  **Server-Side**: The `ExternalChaptersController` (API) looks for files next to the media item when requested.
2.  **Client-Side**: A `StartupService` modifies `index.html` to load a small script (`client.js`). This script monitors playback events and fetches chapters from the API.

## License

GPL-3.0
