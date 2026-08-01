# Domain Model

This document defines the initial domain model for **HOI4 Benchmark**.

## Benchmark aggregate

### BenchmarkSession

Represents a running benchmark session.

Responsibilities:

- hold benchmark identity;
- hold benchmark settings snapshot;
- track current status;
- collect monthly measurements;
- store system and game metadata captured at start;
- produce a final benchmark result.

Potential statuses:

- NotStarted;
- WaitingForAutosave;
- Running;
- Completed;
- Cancelled;
- Failed.

### BenchmarkResult

Represents a completed benchmark result.

Contains:

- benchmark id;
- benchmark name;
- schema version;
- application version;
- created/started/completed timestamps;
- benchmark settings snapshot;
- game information;
- system snapshot;
- measurements;
- statistics;
- score;
- warnings;
- optional user notes.

### MonthlyMeasurement

Represents one measured interval between autosaves.

Contains:

- month index;
- source game date;
- target game date;
- elapsed real time;
- start timestamp;
- end timestamp;
- warm-up flag;
- warnings.

## Value objects

### GameDate

A Hearts of Iron IV in-game date.

It must be modeled as a domain value object instead of `DateTime`.

Responsibilities:

- validate year/month/day;
- compare game dates;
- format date for display/export;
- calculate month differences;
- detect expected next month transition.

### BenchmarkScore

Represents the public benchmark score.

Contains:

- numeric score;
- formula version;
- baseline information;
- optional description.

The formula must be versioned because public benchmark results need long-term comparability.

## Statistics

### BenchmarkStatistics

Contains:

- measurement count;
- average month time;
- median month time;
- minimum month time;
- maximum month time;
- standard deviation;
- total measured time;
- estimated year time;
- benchmark score.

Warm-up measurements should be stored but excluded from the main statistics unless explicitly requested.

## Game metadata

### GameVersion

Contains:

- version;
- build/checksum if available;
- detection source.

### GameMod

Contains:

- name;
- version if available;
- enabled flag;
- workshop id if available;
- local path if available.

## System snapshot

### SystemSnapshot

Captured near benchmark start.

Contains:

- CPU information;
- GPU list;
- memory information;
- operating system information;
- timestamp;
- optional privacy-safe metadata.

Machine/user identifying information should not be exported by default.

### CpuInfo

Contains:

- name;
- vendor if available;
- physical core count;
- logical processor count;
- base/max clock if available;
- architecture.

### GpuInfo

Contains:

- name;
- vendor if available;
- VRAM if available;
- driver version if available.

### MemoryInfo

Contains:

- total physical memory;
- available memory at benchmark start if available;
- memory speed/type if available.

### OperatingSystemInfo

Contains:

- name;
- version;
- build;
- architecture.
