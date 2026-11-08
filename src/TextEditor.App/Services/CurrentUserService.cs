using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;
using TextEditor.Core.Repositories;
using TextEditor.Core.Services;

namespace TextEditor.App.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private const string DefaultLogin = "local";
    private readonly IUserRepository _users;

    public Guid UserId { get; private set; }

    public CurrentUserService(IUserRepository users) => _users = users;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var existing = await _users.GetByLoginAsync(DefaultLogin, ct);
        if (existing is not null)
        {
            UserId = existing.Id;
            return;
        }

        var user = new User(
            Id: Guid.NewGuid(),
            Login: DefaultLogin,
            PasswordHash: "n/a", // локальный пользователь, пароль не используется
            CreatedAt: DateTimeOffset.UtcNow);

        var created = await _users.AddAsync(user, ct);
        UserId = created.Id;
    }
}