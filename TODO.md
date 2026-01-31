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

## Review Results

### Kritische Bugs

- [ ] **RenameService**: Fehlerbehandlung für existierende Zieldateien hinzufügen (File.Move wirft Exception bei Konflikt)
- [ ] **MediaWatcherService**: Null-Check für enriched MediaFile hinzufügen (Crash wenn TMDB keine Metadaten findet)
- [ ] **API-Key Validation**: TMDB API-Key beim Startup validieren statt NullReferenceException zur Laufzeit
- [ ] **MediaController.Scan**: Path-Validierung implementieren (prüfen ob innerhalb WatchPath)

### Performance-Optimierungen

- [ ] **MediaWatcherService**: Entferne `_processedFiles` ConcurrentDictionary (überflüssig, DB ist bereits Single Source of Truth)
- [ ] **MediaWatcherService**: Optimiere Proposal-Abfrage (lädt bei jedem Scan alle Proposals)
- [ ] **Regex-Pattern**: Nutze `[GeneratedRegex]` oder statische Regex-Felder mit `RegexOptions.Compiled` in MediaScanner und TmdbMetadataProvider
- [ ] **ProposalController.GetStats**: Ineffizient - ruft `GetAll()` dreimal auf, sollte eine dedizierte Stats-Abfrage sein

### Verbesserungen

- [ ] **RenameService**: Directory-Struktur für TV-Serien implementieren (Serienname/Staffel XX/Episode.mkv)
- [ ] **RenameService**: Konfigurierbare Namensschema-Templates statt hardcoded Format
- [ ] **MediaController**: Batch-Approval Endpunkt für mehrere IDs gleichzeitig
- [ ] **TmdbMetadataProvider**: Retry-Logik für transiente API-Fehler (z.B. mit Polly)
- [ ] **MediaScanner**: Startup-Prüfung ob ffprobe installiert ist
- [ ] **MediaScanner**: Konfigurierbare Timeouts für große Dateien
- [ ] **MediaScanner**: Besseres Error-Handling statt leeren String bei ffprobe-Fehler
- [ ] **Docker**: Healthchecks für API und Web-Container hinzufügen
- [ ] **ProposalController.GetProposals**: Whitelist-Pattern für sortBy Parameter (SQL Injection Prävention)
- [ ] **MediaController.Approve**: Log-Statement sollte immer ausgeführt werden (nicht hinter IsEnabled-Check)

### Testing

- [ ] Unit Tests für Core-Services (MediaScanner, MetadataResolver, RenameService)
- [ ] Integration Tests für API-Endpoints
- [ ] Test für Race Conditions im MediaWatcherService
