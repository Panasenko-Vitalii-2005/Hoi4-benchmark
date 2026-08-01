using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Domain.Common;

namespace HOI4Benchmark.Application.Implementations;

public sealed class BenchmarkSessionService : IBenchmarkSessionService
{
    private readonly IRepository<BenchmarkSession> _repository;

    public BenchmarkSessionService(
        IRepository<BenchmarkSession> repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<BenchmarkSession> StartBenchmarkAsync(
        string name,
        int targetMeasuredMonths,
        int warmupMonths = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(
                "Benchmark session name is required.");
        }

        BenchmarkSession? activeSession =
            await GetCurrentSessionAsync();

        if (activeSession is not null)
        {
            throw new DomainException(
                "Another benchmark session is already active.");
        }

        var session = new BenchmarkSession(
            Guid.NewGuid(),
            name,
            DateTimeOffset.UtcNow,
            targetMeasuredMonths,
            warmupMonths);

        session.MarkWaitingForAutosave();

        await _repository.AddAsync(session);

        return session;
    }

    public async Task<BenchmarkSession> StopBenchmarkAsync(
        Guid sessionId)
    {
        BenchmarkSession? session =
            await _repository.GetByIdAsync(sessionId);

        if (session is null)
{
    throw new DomainException(
        "Benchmark session was not found.");
}

        if (session.Status is
            BenchmarkStatus.Completed or
            BenchmarkStatus.Cancelled or
            BenchmarkStatus.Failed)
        {
            throw new DomainException(
                "Benchmark session is already finished.");
        }

        session.Cancel(DateTimeOffset.UtcNow);

        await _repository.UpdateAsync(session);

        return session;
    }

    public async Task<BenchmarkSession?> GetCurrentSessionAsync()
    {
        IEnumerable<BenchmarkSession> sessions =
            await _repository.GetAllAsync();

        return sessions
            .Where(session =>
                session.Status is
                    BenchmarkStatus.WaitingForAutosave or
                    BenchmarkStatus.Running)
            .OrderByDescending(session => session.StartedAtUtc)
            .FirstOrDefault();
    }
}