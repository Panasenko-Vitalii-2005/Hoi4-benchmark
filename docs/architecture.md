# Architecture

Project: **HOI4 Benchmark**

Status: initial architecture decision.

## Chosen approach

The project uses **Clean Architecture + MVVM**.

Layers:

```text
HOI4Benchmark.App
  -> HOI4Benchmark.Application
      -> HOI4Benchmark.Domain

HOI4Benchmark.Infrastructure
  -> HOI4Benchmark.Application
  -> HOI4Benchmark.Domain
```

Dependency direction must always point inward. The domain layer must not depend on WPF, file system APIs, JSON, CSV, Windows APIs, dependency injection, or logging frameworks.

## Projects

```text
src/
├── HOI4Benchmark.Domain/
├── HOI4Benchmark.Application/
├── HOI4Benchmark.Infrastructure/
└── HOI4Benchmark.App/

tests/
└── HOI4Benchmark.Tests/
```

A separate CLI project may be added later without changing the domain or application layers.

## Project responsibilities

### HOI4Benchmark.Domain

Contains the core domain model and business rules:

- benchmark session and benchmark result models;
- game date value object;
- monthly measurements;
- statistics and score calculation concepts;
- system snapshot models;
- game version and mod models;
- domain exceptions and validation rules.

Must not contain:

- WPF types;
- file system access;
- JSON/CSV serialization;
- Windows-specific code;
- dependency injection setup;
- concrete logging implementation.

### HOI4Benchmark.Application

Contains use cases and application-level abstractions:

- start/stop benchmark orchestration;
- result loading, saving, deletion, comparison;
- export use cases;
- settings use cases;
- interfaces for infrastructure services;
- DTOs for crossing layer boundaries;
- validation of user/application input.

Application depends on Domain only.

### HOI4Benchmark.Infrastructure

Contains implementations of application abstractions:

- physical file system access;
- autosave file watcher;
- HOI4 save parsing;
- JSON result storage;
- JSON and CSV exporters;
- Windows system information providers;
- game installation/version/mod detection;
- settings persistence;
- logging implementation;
- clock implementation.

Infrastructure depends on Application and Domain.

### HOI4Benchmark.App

Contains WPF UI and composition root:

- views;
- view models;
- navigation;
- dialogs;
- commands;
- themes and resources;
- dependency injection configuration;
- application startup.

The WPF layer must not implement benchmark logic directly.

## Architectural rules

1. Domain has no dependencies on other project layers.
2. Application depends only on Domain.
3. Infrastructure implements interfaces declared by Application.
4. App composes Application + Infrastructure and contains WPF-specific code.
5. UI calls use cases or application services, not infrastructure directly.
6. File parsing, system information, and export formats are replaceable strategies.
7. Benchmark score formula must be versioned.
8. Export schema must be versioned.
9. Expected operational failures should be represented as result values where practical.
10. Unexpected failures may use exceptions and logging.

## Key patterns

- Clean Architecture;
- MVVM;
- Dependency Injection;
- Repository;
- Strategy;
- Adapter;
- Factory;
- Observer/Event-based progress reporting;
- Result pattern for expected failures.

## First implementation milestone

The first implementation milestone should create only the solution skeleton and domain foundation. UI and infrastructure details should be added after the core model is stable.
