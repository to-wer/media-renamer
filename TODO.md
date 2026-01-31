# TODO

- ~Create README.md file with project overview and instructions~ DONE
- ~Complete docker-compose~ DONE
- ~Set up CI/CD pipeline~ DONE
- Write unit tests for all modules
- Implement logging and error handling
- Implement clear list
  - ~API endpoint~ DONE
  - Frontend button
- Implement separate transcoding optimizer: Standalone service/profiles for H264-to-HEVC etc. via FFmpeg. Scan library independently, queue jobs, optional integration with proposals (e.g. "Optimize after rename")
- Implement configurable target libraries: Define paths like TvLibraryPath and MoviesLibraryPath in AppSettings. Update RenameProposal to include full target path (copy/move file there on approve). Use placeholders e.g. {SeriesTitle} ({Year})/Season {SeasonNumber}/{EpisodeName}.
- Fehlende Directory-Struktur für Serien
  - Der RenameService legt alle Dateien flach im OutputPath ab. 
  - Für TV-Serien wäre eine Ordnerstruktur sinnvoll: Serienname/Staffel 01/Episodendatei.mkv.
- Implement user authentication and authorization
- API key handling TMDB
  - Create TMDB Provider dynamically based on API key presence in AppSettings
  - If no API key, use basic file renaming without metadata fetching
  - Add support for multiple providers (e.g. TVDB) in the future