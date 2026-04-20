using Aether.Domain.Entities;
using Aether.Domain.Interfaces;
using Aether.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aether.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AetherDbContext _context;

    public UserProfileRepository(AetherDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserProfiles.FindAsync(new object[] { userId }, ct);
    }

    public async Task UpsertAsync(UserProfile profile, CancellationToken ct = default)
    {
        var existing = await _context.UserProfiles.FindAsync(new object[] { profile.UserId }, ct);
        if (existing == null)
            _context.UserProfiles.Add(profile);
        else
            _context.Entry(existing).CurrentValues.SetValues(profile);

        await _context.SaveChangesAsync(ct);
    }
}
