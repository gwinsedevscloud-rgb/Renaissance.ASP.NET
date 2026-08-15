using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;
using Renaissance.Infrastructure.Persistence;

namespace Renaissance.Infrastructure.Services;

public class ClientNumberGenerator : IClientNumberGenerator
{
    private const string Prefix = "ACH";
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
        return Format(next);
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 8;

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
                return Format(sequence.LastSequence);
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

    private static string Format(long sequence) => $"{Prefix}{sequence:D4}";
}
