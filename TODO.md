# TODO

- ~Create README.md file with project overview and instructions~ DONE
- Complete docker-compose
- Set up CI/CD pipeline
- Write unit tests for all modules
- Implement logging and error handling
- Implement clear list
  - ~API endpoint~ DONE
  - Frontend button
- Implement configurable target libraries: Define paths like TvLibraryPath and MoviesLibraryPath in AppSettings. Update RenameProposal to include full target path (copy/move file there on approve). Use placeholders e.g. {SeriesTitle} ({Year})/Season {SeasonNumber}/{EpisodeName}.
- Implement separate transcoding optimizer: Standalone service/profiles for H264-to-HEVC etc. via FFmpeg. Scan library independently, queue jobs, optional integration with proposals (e.g. "Optimize after rename")