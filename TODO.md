# TODO

## 🚨 Priority 1: Critical Bugs (Fix immediately)

- [X] **RenameService**: Add error handling for existing target files (File.Move throws exception on conflict)
- [X] **MediaWatcherService**: Add null-check for enriched MediaFile (crash when TMDB finds no metadata)
- [X] **API-Key Validation**: Validate TMDB API key at startup instead of runtime NullReferenceException
- [X] **MediaController.Scan**: Implement path validation (check if within WatchPath) - Deleted controller action

## 🛠️ Priority 2: Core Functionality

- [ ] Write unit tests for all modules
- [ ] Implement logging and error handling
- [] Implement clear list
  - ~~API endpoint~~ DONE
  - Frontend button
- [ ] Missing directory structure for TV series
  - RenameService currently dumps all files flat in OutputPath
  - TV series need folder structure: SeriesName/Season 01/EpisodeFile.mkv
- [ ] Implement configurable target libraries: Define paths like TvLibraryPath and MoviesLibraryPath in AppSettings. Update RenameProposal to include full target path (copy/move file there on approve). Use placeholders e.g. {SeriesTitle} ({Year})/Season {SeasonNumber}/{EpisodeName}

## ⚡ Priority 3: Performance Optimizations

- [ ] **MediaWatcherService**: Remove `_processedFiles` ConcurrentDictionary (redundant, DB is single source of truth)
- [ ] **MediaWatcherService**: Optimize proposal query (currently loads all proposals on every scan)
- [ ] **Regex-Pattern**: Use `[GeneratedRegex]` or static Regex fields with `RegexOptions.Compiled` in MediaScanner and TmdbMetadataProvider
- [ ] **ProposalController.GetStats**: Inefficient - calls `GetAll()` three times, should use dedicated stats query

## 🔒 Priority 4: Security & Code Quality

- [ ] **ProposalController.GetProposals**: Whitelist pattern for sortBy parameter (SQL injection prevention)
- [ ] **MediaController.Approve**: Log statement should always execute (not behind IsEnabled check)
- [ ] **MediaScanner**: Startup check if ffprobe is installed
- [ ] **MediaScanner**: Configurable timeouts for large files
- [ ] **MediaScanner**: Better error handling instead of empty string on ffprobe failure
- [ ] **TmdbMetadataProvider**: Retry logic for transient API errors (e.g. with Polly)
- [ ] **Docker**: Add healthchecks for API and Web containers
- [ ] Integration tests for API endpoints
- [ ] Tests for race conditions in MediaWatcherService

## 🎯 Priority 5: Advanced Features

- [ ] Implement separate transcoding optimizer: Standalone service/profiles for H264-to-HEVC etc. via FFmpeg. Scan library independently, queue jobs, optional integration with proposals (e.g. "Optimize after rename")
- [ ] **RenameService**: Configurable naming schema templates instead of hardcoded format
- [ ] **MediaController**: Batch-approval endpoint for multiple IDs simultaneously
- [ ] Implement user authentication and authorization
- [ ] API key handling TMDB
  - Create TMDB Provider dynamically based on API key presence in AppSettings
  - If no API key, use basic file renaming without metadata fetching
  - Add support for multiple providers (e.g. TVDB) in the future

## ✅ Completed

- ~~Create README.md file with project overview and instructions~~ DONE
- ~~Complete docker-compose~~ DONE
- ~~Set up CI/CD pipeline~~ DONE
- ~~Implement clear list - API endpoint~~ DONE
