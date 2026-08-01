# HOI4 Benchmark

**HOI4 Benchmark** is an open-source desktop application for automated performance benchmarking of _Hearts of Iron IV_.

The goal is to measure real simulation speed by monitoring HOI4 autosaves, extracting in-game dates, recording month-to-month elapsed time, calculating benchmark statistics, and exporting results to JSON/CSV.

Project phase: **release preparation**.

The application already includes a WPF desktop interface, benchmark session management, persistent results, result comparison, charts, JSON/CSV export, privacy controls, logging, and diagnostics bundle generation.

The project uses Clean Architecture and MVVM to keep the application maintainable and testable. The current implementation includes the domain model, application services, infrastructure persistence, WPF presentation layer, automated tests, export tooling, logging, and diagnostics.

![CI](https://github.com/<user>/Hoi4-benchmark/actions/workflows/ci.yml/badge.svg)
![Release](https://github.com/<user>/Hoi4-benchmark/actions/workflows/release.yml/badge.svg)

## Technology stack

- C#
- .NET 9
- WPF
- MVVM
- JSON
- CSV
- Git

## Requirements

Before running the application, ensure the following software is installed:

- Windows 10 or Windows 11 (64-bit)
- Hearts of Iron IV
- .NET 9 SDK (development only)
- Git (optional)

For end users downloading a published release, no .NET installation is required because release builds are distributed as self-contained Windows x64 packages.

---

## Installation

Clone the repository:

```bash
git clone https://github.com/<your-account>/Hoi4-benchmark.git
cd Hoi4-benchmark
```

Restore dependencies:

```bash
dotnet restore HOI4Benchmark.sln
```

Build the solution:

```bash
dotnet build HOI4Benchmark.sln
```

Run the application:

```bash
dotnet run --project src/HOI4Benchmark.App
```

---

## Running a benchmark

1. Launch Hearts of Iron IV.
2. Enable monthly autosaves.
3. Load the benchmark save.
4. Start the benchmark in the application.
5. Let the game run at maximum speed.
6. Wait until the configured end date is reached.
7. Stop the benchmark.
8. Review statistics and charts.
9. Export results if desired.

---

## Exporting results

Benchmark results can be exported as:

- JSON
- CSV

Privacy options allow:

- anonymizing benchmark names;
- removing exact timestamps;
- excluding warning messages.

---

## Diagnostics bundle

If something goes wrong:

1. Open **Settings**.
2. Click **Create diagnostics bundle**.
3. A ZIP archive containing logs and diagnostic information will be created.

This bundle can be attached to a GitHub issue.

---

## Logs

Application logs are stored in:

```text
%LOCALAPPDATA%\HOI4Benchmark\logs
```

Current log file:

```text
app.log
```

---

## Automated quality checks

Every push to GitHub automatically:

- restores dependencies;
- builds the solution;
- executes unit tests.

Tagged releases additionally:

- publish a self-contained Windows x64 build;
- generate SHA-256 checksum files;
- upload release artifacts.

---

## Project status

Current development stage:

**Phase 7 — Release engineering**

The project is under active development.
Breaking changes may occur before version 1.0.

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
- [`docs/code-signing.md`](docs/code-signing.md) — Windows release signing and certificate security plan.
- [`CONTRIBUTING.md`](CONTRIBUTING.md) — contribution guidelines for developers.

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

This project is licensed under the [MIT License](LICENSE).
