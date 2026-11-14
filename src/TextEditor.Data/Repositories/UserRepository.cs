using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TextEditor.Core.Models;
using TextEditor.Core.Repositories;
using TextEditor.Data.Entities;

namespace TextEditor.Data.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByLoginAsync(string login, CancellationToken ct = default)
    {
        var entity = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Login == login, ct);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        var entity = ToEntity(user);
        _db.Users.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToDomain(entity);
    }

    public async Task<User?> GetGuestAsync(CancellationToken ct = default)
    {
        var entity = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsGuest, ct);
        return entity is null ? null : ToDomain(entity);
    }

    // --- Связывание ---
    private static User ToDomain(UserEntity e) => new(
        Id: e.Id,
        Login: e.Login,
        PasswordHash: e.PasswordHash,
        CreatedAt: ParseDate(e.CreatedAt),
        IsGuest: e.IsGuest);

    private static UserEntity ToEntity(User u) => new()
    {
        Id = u.Id,
        Login = u.Login,
        PasswordHash = u.PasswordHash,
        CreatedAt = FormatDate(u.CreatedAt),
        IsGuest = u.IsGuest
    };

    private static DateTimeOffset ParseDate(string s)
    => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private static string FormatDate(DateTimeOffset d)
        => d.ToString("O", CultureInfo.InvariantCulture);
}
