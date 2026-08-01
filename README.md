# HOI4 Benchmark

**HOI4 Benchmark** is an open-source desktop application for automated performance benchmarking of _Hearts of Iron IV_.

The goal is to measure real simulation speed by monitoring HOI4 autosaves, extracting in-game dates, recording month-to-month elapsed time, calculating benchmark statistics, and exporting results to JSON/CSV.

Project phase: **release preparation**.

The application already includes a WPF desktop interface, benchmark session management, persistent results, result comparison, charts, JSON/CSV export, privacy controls, logging, and diagnostics bundle generation.

The project uses Clean Architecture and MVVM to keep the application maintainable and testable. The current implementation includes the domain model, application services, infrastructure persistence, WPF presentation layer, automated tests, export tooling, logging, and diagnostics.

## Technology stack

- C#
- .NET 9
- WPF
- MVVM
- JSON
- CSV
- Git

## Architecture

Chosen architecture: **Clean Architecture + MVVM**.
Projects:

```text
src/
├── HOI4Benchmark.Domain
├── HOI4Benchmark.Application
├── HOI4Benchmark.Infrastructure
└── HOI4Benchmark.App

tests/
└── HOI4Benchmark.Tests
```

Layer responsibilities and architectural rules are documented in [`docs/architecture.md`](docs/architecture.md).

## Documentation

- [`docs/architecture.md`](docs/architecture.md) — chosen architecture and dependency rules.
- [`docs/project-structure.md`](docs/project-structure.md) — planned solution and folder structure.
- [`docs/domain-model.md`](docs/domain-model.md) — initial domain model.
- [`docs/services.md`](docs/services.md) — planned services and interfaces.
- [`docs/roadmap.md`](docs/roadmap.md) — development phases.

## Benchmark concept

The user runs a prepared benchmark save in Hearts of Iron IV with:

- maximum game speed;
- monthly autosave enabled.

The application monitors `autosave_temp.hoi4`. After each autosave, it:

1. extracts the in-game date;
2. measures real elapsed time between monthly autosaves;
3. stores monthly measurements;
4. calculates summary statistics;
5. captures system/game metadata;
6. exports results to JSON and CSV.

Calculated statistics:

- per-month time;
- average;
- median;
- minimum;
- maximum;
- standard deviation;
- estimated year time;
- benchmark score.

## Current features

- WPF desktop interface;
- benchmark session start and stop;
- HOI4 autosave monitoring;
- in-game date parsing;
- monthly performance measurements;
- benchmark statistics and score;
- persistent result storage;
- result history;
- comparison of two benchmark runs;
- performance charts;
- JSON and CSV export;
- export privacy controls;
- file logging;
- diagnostics ZIP bundle.

## Current limitations

- Windows x64 only;
- HOI4 text-format saves are currently required;
- compressed/binary saves are not yet supported;
- public installer is not available yet;
- release builds are currently unsigned.

## License

License is not selected yet.

For an open-source desktop benchmark tool, likely candidates are MIT or GPL-3.0. This decision should be made before the first public release.
