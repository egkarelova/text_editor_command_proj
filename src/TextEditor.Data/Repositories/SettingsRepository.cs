using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;
using TextEditor.Core.Repositories;

namespace TextEditor.Data.Repositories;

// TODO: Айдана.
public sealed class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _db;
    public SettingsRepository(AppDbContext db) => _db = db;

    public Task<UserSettings?> GetAsync(Guid userId, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task UpsertAsync(UserSettings settings, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");
}