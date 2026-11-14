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

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _db;

    public SettingsRepository(AppDbContext db) => _db = db;

    public async Task<UserSettings?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var entity = await _db.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task UpsertAsync(UserSettings settings, CancellationToken ct = default)
    {
        var entity = await _db.UserSettings
            .FirstOrDefaultAsync(x => x.UserId == settings.UserId, ct);

        if (entity is null)
        {
            _db.UserSettings.Add(ToEntity(settings));
        }
        else
        {
            entity.Theme = settings.Theme;
            entity.DefaultFont = settings.DefaultFont;
            entity.DefaultFontSize = settings.DefaultFontSize;
            entity.AutoSaveEnabled = settings.AutoSaveEnabled;
            entity.AutoSaveMinutes = settings.AutoSaveMinutes;
        }

        await _db.SaveChangesAsync(ct);
    }

    // --- Связывание ---
    private static UserSettings ToDomain(UserSettingsEntity e) => new(
        UserId: e.UserId,
        Theme: e.Theme,
        DefaultFont: e.DefaultFont,
        DefaultFontSize: e.DefaultFontSize,
        AutoSaveEnabled: e.AutoSaveEnabled,
        AutoSaveMinutes: e.AutoSaveMinutes);

    private static UserSettingsEntity ToEntity(UserSettings s) => new()
    {
        UserId = s.UserId,
        Theme = s.Theme,
        DefaultFont = s.DefaultFont,
        DefaultFontSize = s.DefaultFontSize,
        AutoSaveEnabled = s.AutoSaveEnabled,
        AutoSaveMinutes = s.AutoSaveMinutes
    };
}