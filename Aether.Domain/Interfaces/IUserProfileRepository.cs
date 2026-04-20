using Aether.Domain.Entities;

namespace Aether.Domain.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task UpsertAsync(UserProfile profile, CancellationToken ct = default);
}
