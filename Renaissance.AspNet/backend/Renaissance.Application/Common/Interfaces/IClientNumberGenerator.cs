namespace Renaissance.Application.Common.Interfaces;

public interface IClientNumberGenerator
{
    /// <summary>Reserves and returns the next client number. Call only when persisting a new client.</summary>
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the next client number without advancing the sequence (for UI preview).</summary>
    Task<string> PeekNextAsync(CancellationToken cancellationToken = default);
}
