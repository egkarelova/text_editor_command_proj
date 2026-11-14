using System;
using TextEditor.Core.Models;
using TextEditor.Data.Entities;
using TextEditor.Data.Repositories;
using TextEditor.Tests.TestHelpers;
using Xunit;

namespace TextEditor.Tests.Repositories;

public sealed class SettingsRepositoryTests
{
    private static async Task<Guid> SeedUserAsync(SqliteInMemoryFixture f)
    {
        var userId = Guid.NewGuid();
        f.Context.Users.Add(new UserEntity
        {
            Id = userId,
            Login = $"user-{userId:N}",
            PasswordHash = "h",
            CreatedAt = DateTimeOffset.UtcNow.ToString("O")
        });
        await f.Context.SaveChangesAsync();
        return userId;
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenSettingsDoNotExist()
    {
        using var f = new SqliteInMemoryFixture();
        var repo = new SettingsRepository(f.Context);

        var result = await repo.GetAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertAsync_Inserts_WhenNotExists()
    {
        using var f = new SqliteInMemoryFixture();
        var userId = await SeedUserAsync(f);
        var repo = new SettingsRepository(f.Context);
        var settings = new UserSettings(userId, "Dark", "Consolas", 16, true, 10);

        await repo.UpsertAsync(settings);

        var stored = await repo.GetAsync(userId);
        Assert.NotNull(stored);
        Assert.Equal("Dark", stored.Theme);
        Assert.Equal("Consolas", stored.DefaultFont);
        Assert.Equal(16, stored.DefaultFontSize);
        Assert.True(stored.AutoSaveEnabled);
        Assert.Equal(10, stored.AutoSaveMinutes);
    }

    [Fact]
    public async Task UpsertAsync_Updates_WhenExists()
    {
        using var f = new SqliteInMemoryFixture();
        var userId = await SeedUserAsync(f);
        var repo = new SettingsRepository(f.Context);

        await repo.UpsertAsync(new UserSettings(userId, "Light", "Segoe UI", 14, true, 5));
        await repo.UpsertAsync(new UserSettings(userId, "Dark", "Consolas", 18, false, 15));

        var stored = await repo.GetAsync(userId);
        Assert.NotNull(stored);
        Assert.Equal("Dark", stored.Theme);
        Assert.Equal("Consolas", stored.DefaultFont);
        Assert.Equal(18, stored.DefaultFontSize);
        Assert.False(stored.AutoSaveEnabled);
        Assert.Equal(15, stored.AutoSaveMinutes);
    }

    [Fact]
    public async Task UpsertAsync_IsVisibleFromAnotherContext()
    {
        using var f = new SqliteInMemoryFixture();
        var userId = await SeedUserAsync(f);

        await using (var ctx1 = f.CreateNewContext())
        {
            var repo = new SettingsRepository(ctx1);
            await repo.UpsertAsync(new UserSettings(userId, "Dark", "Arial", 12, false, 3));
        }

        await using (var ctx2 = f.CreateNewContext())
        {
            var repo = new SettingsRepository(ctx2);
            var stored = await repo.GetAsync(userId);
            Assert.NotNull(stored);
            Assert.Equal("Dark", stored.Theme);
        }
    }

    [Fact]
    public async Task GetAsync_ReturnsCorrectRow_WhenMultipleUsersHaveSettings()
    {
        using var f = new SqliteInMemoryFixture();
        var userA = await SeedUserAsync(f);
        var userB = await SeedUserAsync(f);
        var repo = new SettingsRepository(f.Context);

        await repo.UpsertAsync(new UserSettings(userA, "Light", "Segoe UI", 14, true, 5));
        await repo.UpsertAsync(new UserSettings(userB, "Dark", "Consolas", 16, false, 10));

        var a = await repo.GetAsync(userA);
        var b = await repo.GetAsync(userB);

        Assert.NotNull(a);
        Assert.NotNull(b);
        Assert.Equal("Light", a.Theme);
        Assert.Equal("Dark", b.Theme);
        Assert.Equal(userA, a.UserId);
        Assert.Equal(userB, b.UserId);
    }
}