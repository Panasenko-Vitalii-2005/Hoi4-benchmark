# Contributing to HOI4 Benchmark

Thank you for your interest in contributing to HOI4 Benchmark.

Contributions of all sizes are welcome, including:

- bug reports;
- feature requests;
- documentation improvements;
- unit tests;
- performance improvements;
- UI improvements;
- pull requests.

---

# Development environment

Required software:

- .NET 9 SDK
- Windows 10 or Windows 11
- Git

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

Run tests:

```bash
dotnet test HOI4Benchmark.sln
```

---

# Project architecture

The project follows:

- Clean Architecture
- MVVM
- SOLID principles

Dependencies are one-directional:

```text
App
↓
Application
↓
Domain

Infrastructure
↓
Application
```

The Domain project must not depend on any other project.

---

# Coding guidelines

Please follow these conventions:

- use meaningful names;
- keep methods small;
- prefer dependency injection;
- avoid duplicated logic;
- write unit tests for new functionality;
- avoid unnecessary comments when code is self-explanatory.

Follow the existing project formatting.

---

# Pull requests

Before opening a pull request:

- build the solution successfully;
- ensure all tests pass;
- ensure GitHub Actions CI succeeds;
- keep commits focused on a single change;
- update documentation when appropriate.

Large changes should preferably be discussed in an issue before implementation.

---

# Commit messages

Recommended format:

```text
type: short description
```

Examples:

```text
feat: add benchmark export
fix: resolve autosave parser bug
docs: update README
test: improve repository tests
refactor: simplify benchmark session logic
ci: update release workflow
```

---

# Reporting bugs

When reporting a bug, please include:

- application version;
- Windows version;
- reproduction steps;
- expected behavior;
- actual behavior;
- screenshots if applicable;
- diagnostics bundle, if available.

---

# Feature requests

Feature requests should explain:

- the problem;
- the proposed solution;
- possible alternatives;
- expected user benefit.

---

# Code review

Every contribution should be:

- readable;
- maintainable;
- tested;
- documented when necessary.

Constructive review feedback is encouraged.

---

# License

By submitting a contribution, you agree that your contribution may be distributed under the project's future open-source license.
