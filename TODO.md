# TODO

## Bugs
- [X] What happens if the file is removed after the proposal is created but before approval? (FileNotFoundException on move)
  - [X] Delete pending proposal if file not found during approval or during scan
- [X] Fix "wrong" template for TV series (currently "Season S01E01 EpisodeName", should be "Season - S01E01 - EpisodeName")
- [X] Handle System.IO.IOException: Target file already exists

## 🚨 Priority 1: Critical Bugs (Fix immediately)

- [X] **RenameService**: Add error handling for existing target files (File.Move throws exception on conflict)
- [X] **MediaWatcherService**: Add null-check for enriched MediaFile (crash when TMDB finds no metadata)
- [X] **API-Key Validation**: Validate TMDB API key at startup instead of runtime NullReferenceException
- [X] **MediaController.Scan**: Implement path validation (check if within WatchPath) - Deleted controller action

### 🔒 Security - Priority 1

- [ ] **RenameService.Execute**: Add path traversal validation (ensure target path stays within configured directories)
- [ ] **MediaController.Approve**: Sanitize error messages before returning to client (prevent path exposure)
- [ ] **ProposalDbContext**: Configure foreign key relationships for proper cascade delete behavior
- [ ] **RenameService**: Fix null check for `GetDirectoryName` (can return null for root paths)

## 🛠️ Priority 2: Core Functionality

- [ ] Write unit tests for all modules
- [ ] Implement logging and error handling
- [ ] Implement clear list
  - [X] API endpoint
  - [ ] Frontend button
- [ ] Missing directory structure for TV series
  - RenameService currently dumps all files flat in OutputPath
  - TV series need folder structure: SeriesName/Season 01/EpisodeFile.mkv
- [ ] Implement configurable target libraries: Define paths like TvLibraryPath and MoviesLibraryPath in AppSettings. Update RenameProposal to include full target path (copy/move file there on approve). Use placeholders e.g. {SeriesTitle} ({Year})/Season {SeasonNumber}/{EpisodeName}

### ⚠️ Issues Found in Code Review - Priority 2

- [ ] **MediaWatcherService**: Fix race condition - second `GetPending()` call may not reflect deleted items
- [ ] **MediaWatcherService**: Make file extensions configurable (currently hardcoded to .mkv and .mp4)
- [ ] **TmdbMetadataProvider**: Complete the incomplete `EnrichEpisode` method (truncated implementation)
- [ ] **TmdbMetadataProvider**: Add rate limiting to prevent API key blocking

## ⚡ Priority 3: Performance Optimizations

- [ ] **MediaWatcherService**: Remove `_processedFiles` ConcurrentDictionary (redundant, DB is single source of truth)
- [ ] **MediaWatcherService**: Optimize proposal query (currently loads all proposals on every scan)
- [ ] **Regex-Pattern**: Use `[GeneratedRegex]` or static Regex fields with `RegexOptions.Compiled` in MediaScanner and TmdbMetadataProvider
- [ ] **ProposalController.GetStats**: Inefficient - calls `GetAll()` three times, should use dedicated stats query

### 🔧 Performance - Priority 3

- [ ] **MediaScanner**: Add startup check if ffprobe is installed (fail gracefully)
- [ ] **MediaScanner**: Add configurable timeouts for large files
- [ ] **MediaScanner**: Improve error handling instead of empty string on ffprobe failure
- [ ] **TmdbMetadataProvider**: Implement retry logic for transient API errors (e.g. with Polly)

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

### 📝 Code Quality - Priority 4

- [ ] **Naming Consistency**: Standardize comments to English throughout codebase
- [ ] **Cleanup TODOs**: Address scattered TODO comments in code
- [ ] **Unit Tests**: Increase test coverage for edge cases

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

---

## 📋 Code Review Summary (2026-02-11)

### Architecture Strengths
- Clean separation of concerns (API, Core, Web)
- Proper dependency injection usage
- Pre-compiled Regex patterns for performance
- Good structured logging with Serilog
- Extension methods for string normalization

### Key Findings by Priority
1. **High**: Path traversal vulnerability, race condition, incomplete TMDB implementation
2. **Medium**: Hardcoded extensions, missing EF relationships, null handling
3. **Low**: Code style consistency, documentation improvements
