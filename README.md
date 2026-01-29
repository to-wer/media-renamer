# MediaRenamer

MediaRenamer is an automated media file renaming tool designed for organizing movie and TV series collections. It scans files, fetches metadata from TMDB, generates rename proposals, and provides a web UI for approval via a .NET-based API and Blazor frontend.

## Key Features

- Automatic detection of media files (MKV, MP4) with resolution and codec extraction using MediaInfo.
- TMDB integration for accurate German-language titles, episode details, and movie info.
- Background file watcher service for real-time scanning of watch directories.
- Web dashboard with live proposals, one-click approve/reject, and stats.
- Docker Compose support for easy deployment of API and web services.
  
## Quick Start

- Clone the repo and set TMDB_API_KEY environment variable (get from themoviedb.org).
- Configure paths in appsettings.json: WatchPath (input) and OutputPath (sorted).
- Run with Docker: docker-compose up.
- Access web UI at http://localhost:8081 to review/approve proposals.

### Tailwind CSS Integration

```bash
.\tools\tailwindcss-windows-x64.exe -c tailwind.config.cjs -i Styles/app.input.css -o wwwroot/app.css --watch
```

## Configuration

- appsettings.json (Api): Set Media:WatchPath and Media:OutputPath.
- docker-compose.yml: Maps volumes for data/watch and data/output; customize TMDBApiKey.
