# Project Structure

Target solution structure:

```text
HOI4Benchmark.sln

src/
├── HOI4Benchmark.Domain/
│   ├── Benchmarks/
│   ├── Game/
│   ├── SystemInfo/
│   ├── Settings/
│   ├── Statistics/
│   ├── Common/
│   └── Errors/
│
├── HOI4Benchmark.Application/
│   ├── Abstractions/
│   │   ├── Benchmarking/
│   │   ├── Export/
│   │   ├── FileSystem/
│   │   ├── Game/
│   │   ├── Persistence/
│   │   ├── SystemInfo/
│   │   └── Time/
│   ├── Benchmarking/
│   ├── Results/
│   ├── Export/
│   ├── Settings/
│   ├── DTOs/
│   └── Common/
│
├── HOI4Benchmark.Infrastructure/
│   ├── FileSystem/
│   ├── Game/
│   ├── Export/
│   ├── Persistence/
│   ├── SystemInfo/
│   ├── Time/
│   ├── Logging/
│   ├── Serialization/
│   └── DependencyInjection/
│
└── HOI4Benchmark.App/
    ├── Views/
    ├── ViewModels/
    ├── Commands/
    ├── Navigation/
    ├── Dialogs/
    ├── Themes/
    ├── Resources/
    ├── Converters/
    ├── Behaviors/
    └── DependencyInjection/

tests/
└── HOI4Benchmark.Tests/
    ├── Domain/
    ├── Application/
    ├── Infrastructure/
    └── TestDoubles/
```

## Notes

- `HOI4Benchmark.Tests` is intentionally a single test project at the start to reduce overhead.
- If the test suite grows significantly, it can be split into:
  - `HOI4Benchmark.Domain.Tests`;
  - `HOI4Benchmark.Application.Tests`;
  - `HOI4Benchmark.Infrastructure.Tests`.
- A future CLI can be added as `src/HOI4Benchmark.Cli/` without changing Domain/Application.
