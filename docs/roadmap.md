# Development Roadmap

## Phase 0 — Repository foundation

Status: completed.

Goal: prepare the repository for clean open-source development.

Tasks:

- define architecture documents;
- define project structure;
- add `.gitignore`;
- decide coding standards;
- decide package dependencies;
- prepare CI plan.

## Phase 1 — Solution skeleton

Status: completed.

Goal: create the .NET solution and project boundaries.

Tasks:

- create `HOI4Benchmark.sln`;
- create projects:
  - `HOI4Benchmark.Domain`;
  - `HOI4Benchmark.Application`;
  - `HOI4Benchmark.Infrastructure`;
  - `HOI4Benchmark.App`;
  - `HOI4Benchmark.Tests`;
- configure project references according to Clean Architecture;
- add initial test project;

## Phase 2 — Domain foundation

Status: in progress.

Goal: implement and test core benchmark concepts.

Tasks:

- implement `GameDate` — completed;
- implement `MonthlyMeasurement` — completed;
- implement `BenchmarkSession` — completed;
- implement `BenchmarkResult` — completed;
- implement statistics calculation — completed;
- implement score formula v1 — completed;
- unit-test domain rules thoroughly — in progress.

## Phase 3 — Application use cases

Goal: define executable benchmark workflow independent from WPF and infrastructure.

Tasks:

- define application interfaces — completed;
- implement benchmark start/stop use cases - completed (MVP);;
- implement result management use cases - completed;
- implement settings use cases - completed;
- implement export use cases - completed;
- add test doubles and application tests - completed.

## Phase 4 — Infrastructure MVP

Goal: support real benchmark data collection.

Tasks:

- implement autosave watcher with debounce/retry/stability checks - completed;
- implement initial save date parser - completed;
- implement JSON settings repository - completed;
- implement JSON result repository - completed;
- implement JSON exporter - completed;
- implement CSV exporter - completed;
- implement basic Windows system information provider - completed.

## Phase 5 — WPF MVP

Goal: provide usable desktop UI.

Tasks:

- add shell window and navigation - completed;
- implement Dashboard screen - completed;
- implement Benchmark screen - completed;
- implement Results screen - completed;
- implement Settings screen - completed;
- wire application use cases to view models - completed;
- add basic theme resources - completed;.

## Phase 6 — Compare and polish

Goal: make the app useful for public benchmark comparisons.

Tasks:

- implement Compare Results screen - completed (in test);
- add charts/tables - completed (in test);
- improve exports - completed (in test);
- add privacy options - completed (in test);
- improve error messages - completed (in test);
- add logging and diagnostics bundle - completed (in test).

## Phase 7 — Release engineering

Goal: prepare first public GitHub release.

Tasks:

- add GitHub Actions CI;
- add release build pipeline;
- add code signing plan if needed;
- add README usage guide;
- add contributing guide;
- add issue templates;
- create v0.1.0 release.

