using System;
using Microsoft.EntityFrameworkCore;
using TextEditor.Core.Models;
using TextEditor.Data.Repositories;
using TextEditor.Tests.TestHelpers;
using Xunit;

namespace TextEditor.Tests.Repositories;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsUser_AndReturnsIt()
    {
        using var f = new SqliteInMemoryFixture();
        var repo = new UserRepository(f.Context);
        var user = new User(Guid.NewGuid(), "alice", "hash", DateTimeOffset.UtcNow);

        var created = await repo.AddAsync(user);

        Assert.Equal(user.Id, created.Id);
        Assert.Equal("alice", created.Login);
        Assert.Equal("hash", created.PasswordHash);
    }

    [Fact]
    public async Task AddAsync_IsVisibleFromAnotherContext()
    {
        using var f = new SqliteInMemoryFixture();
        await using (var ctx1 = f.CreateNewContext())
        {
            var repo = new UserRepository(ctx1);
            await repo.AddAsync(new User(Guid.NewGuid(), "bob", "h", DateTimeOffset.UtcNow));
        }

        await using (var ctx2 = f.CreateNewContext())
        {
            var repo = new UserRepository(ctx2);
            var stored = await repo.GetByLoginAsync("bob");
            Assert.NotNull(stored);
            Assert.Equal("bob", stored.Login);
        }
    }

    [Fact]
    public async Task GetByLoginAsync_ReturnsNull_WhenUserNotFound()
    {
        using var f = new SqliteInMemoryFixture();
        var repo = new UserRepository(f.Context);

        var result = await repo.GetByLoginAsync("nobody");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenExists()
    {
        using var f = new SqliteInMemoryFixture();
        var repo = new UserRepository(f.Context);
        var user = new User(Guid.NewGuid(), "carol", "h", DateTimeOffset.UtcNow);
        await repo.AddAsync(user);

        var result = await repo.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("carol", result.Login);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var f = new SqliteInMemoryFixture();
        var repo = new UserRepository(f.Context);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_Throws_OnDuplicateLogin()
    {
        using var f = new SqliteInMemoryFixture();
        var repo = new UserRepository(f.Context);
        await repo.AddAsync(new User(Guid.NewGuid(), "dup", "h1", DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repo.AddAsync(new User(Guid.NewGuid(), "dup", "h2", DateTimeOffset.UtcNow)));
    }

    [Fact]
    public async Task CreatedAt_RoundTrips_PreservingOffset()
    {
        using var f = new SqliteInMemoryFixture();
        var repo = new UserRepository(f.Context);
        var original = new DateTimeOffset(2025, 11, 8, 14, 30, 0, TimeSpan.FromHours(3));
        var user = new User(Guid.NewGuid(), "dave", "h", original);
        await repo.AddAsync(user);

        var stored = await repo.GetByIdAsync(user.Id);

        Assert.NotNull(stored);
        Assert.Equal(original, stored.CreatedAt);
        Assert.Equal(TimeSpan.FromHours(3), stored.CreatedAt.Offset);
    }
}