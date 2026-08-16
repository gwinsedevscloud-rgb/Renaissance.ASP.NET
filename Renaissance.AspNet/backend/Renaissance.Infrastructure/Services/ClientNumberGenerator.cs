using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;
using Renaissance.Infrastructure.Persistence;

namespace Renaissance.Infrastructure.Services;

public class ClientNumberGenerator : IClientNumberGenerator
{
    private readonly RenaissanceDbContext _db;

    public ClientNumberGenerator(RenaissanceDbContext db)
    {
        _db = db;
    }

    public async Task<string> PeekNextAsync(CancellationToken cancellationToken = default)
    {
        var sequence = await _db.ClientNumberSequences
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == 1, cancellationToken);

        var next = (sequence?.LastSequence ?? 0) + 1;
        var prefix = await GetPrefixAsync(cancellationToken);
        return Format(prefix, next);
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 8;
        var prefix = await GetPrefixAsync(cancellationToken);

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var sequence = await _db.ClientNumberSequences
                    .FirstOrDefaultAsync(x => x.Id == 1, cancellationToken);

                if (sequence is null)
                {
                    sequence = new ClientNumberSequence
                    {
                        Id = 1,
                        LastSequence = 0,
                        Version = 0
                    };
                    _db.ClientNumberSequences.Add(sequence);
                }

                sequence.LastSequence += 1;
                sequence.Version += 1;
                await _db.SaveChangesAsync(cancellationToken);
                return Format(prefix, sequence.LastSequence);
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in _db.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }

                if (attempt == maxAttempts - 1)
                {
                    throw;
                }
            }
        }

        throw new InvalidOperationException("Unable to generate client number after concurrency retries.");
    }

    private async Task<string> GetPrefixAsync(CancellationToken cancellationToken)
    {
        var settings = await _db.HospitalSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == HospitalSettings.SingletonId, cancellationToken);

        var prefix = settings?.ClientNumberPrefix?.Trim().ToUpperInvariant();
        return string.IsNullOrWhiteSpace(prefix) ? "ACH" : prefix;
    }

    private static string Format(string prefix, long sequence) => $"{prefix}{sequence:D4}";
}
