# Services and Interfaces

This document lists the initial service boundaries for **HOI4 Benchmark**.

## Application abstractions

Interfaces required by use cases live in `HOI4Benchmark.Application`. Concrete implementations live in `HOI4Benchmark.Infrastructure` or `HOI4Benchmark.App` for UI-only services.

## Benchmarking

### IBenchmarkRunner

Coordinates the benchmark process:

- start benchmark;
- stop benchmark;
- observe autosave updates;
- parse game dates;
- measure elapsed time;
- publish progress;
- produce final result.

### IBenchmarkSessionService

Manages benchmark session state:

- create session;
- add measurements;
- complete session;
- cancel session;
- fail session;
- build final result.

### IBenchmarkProgressReporter

Reports progress to UI or other clients.

Progress events may include:

- status changes;
- detected autosave;
- detected game date;
- completed monthly measurement;
- warning;
- error;
- completion summary.

## File system

### IFileSystem

Abstracts file system operations needed by application logic.

### IFileWatcher

Abstracts watching a file or directory.

For HOI4 autosave monitoring, implementation must handle:

- duplicated file system events;
- file locking;
- partially written files;
- delete/create/change sequences;
- timeout and cancellation.

## Game integration

### IGameSaveParser

Parses a HOI4 save file and extracts relevant information.

### IGameDateExtractor

Extracts a `GameDate` from a save file stream/content.

Possible future strategies:

- plain text save extractor;
- compressed save extractor;
- binary save extractor;
- fallback extractor.

### IGameInstallationDetector

Detects HOI4 installation and common paths.

### IGameVersionProvider

Detects game version/checksum if available.

### IGameModsProvider

Detects enabled mods if available.

## Statistics and scoring

### IStatisticsCalculator

Calculates benchmark statistics from measurements.

This may live as a domain service if it has no external dependencies.

### IBenchmarkScoreCalculator

Calculates benchmark score and records formula version.

Score formula must be explicit and versioned.

## Persistence and export

### IBenchmarkResultRepository

Internal application storage for benchmark results.

Initial implementation: JSON files in the application data directory.

### ISettingsRepository

Loads and saves user settings.

Initial implementation: JSON settings file.

### IBenchmarkExporter

Exports benchmark results for users.

Initial formats:

- JSON;
- CSV summary;
- CSV detailed.

Repository and exporter are intentionally separate concepts.

## System information

### ISystemInfoProvider

Builds a `SystemSnapshot`.

Implementation may use Windows APIs, WMI, registry, DXGI, or other adapters.

Failure to detect non-critical information should usually be a warning, not a fatal error.

## Time

### IClock

Abstracts current time and elapsed time measurement for testability.

## UI-only services

These live in `HOI4Benchmark.App`.

### INavigationService

Handles navigation between WPF screens/view models.

### IDialogService

Handles file/folder picking and user messages.

### IThemeService

Optional service for theme switching.
