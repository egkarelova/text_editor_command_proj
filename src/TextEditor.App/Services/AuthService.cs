using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TextEditor.Core.Models;
using TextEditor.Core.Repositories;
using TextEditor.Core.Services;

namespace TextEditor.App.Services;

public sealed class AuthService : IAuthService
{
    private const string GuestLogin = "guest";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentUserService _currentUser;
    private readonly ISettingsRepository _settings;

    public AuthService(
        IUserRepository users,
        IPasswordHasher hasher,
        ICurrentUserService currentUser,
        ISettingsRepository settings)
    {
        _users = users;
        _hasher = hasher;
        _currentUser = currentUser;
        _settings = settings;
    }

    public async Task<AuthResult> LoginAsync(string login, string password, CancellationToken ct = default)
    {
        login = login?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(login)) return AuthResult.Fail("Введите логин.");
        if (string.IsNullOrEmpty(password)) return AuthResult.Fail("Введите пароль.");

        var user = await _users.GetByLoginAsync(login, ct);
        if (user is null) return AuthResult.Fail("Пользователь не найден.");
        if (user.IsGuest) return AuthResult.Fail("Это гостевая учётная запись. Используйте «Войти как гость».");

        if (!_hasher.Verify(password, user.PasswordHash))
            return AuthResult.Fail("Неверный пароль.");

        _currentUser.SetCurrent(user.Id, isGuest: false);
        return AuthResult.Ok(user.Id, isGuest: false);
    }

    public async Task<AuthResult> RegisterAsync(string login, string password, CancellationToken ct = default)
    {
        login = login?.Trim() ?? string.Empty;
        if (login.Length < 3) return AuthResult.Fail("Логин должен быть не короче 3 символов.");
        if (password.Length < 6) return AuthResult.Fail("Пароль должен быть не короче 6 символов.");

        var existing = await _users.GetByLoginAsync(login, ct);
        if (existing is not null) return AuthResult.Fail("Логин уже занят.");

        var user = new User(
            Id: Guid.NewGuid(),
            Login: login,
            PasswordHash: _hasher.Hash(password),
            CreatedAt: DateTimeOffset.UtcNow,
            IsGuest: false);

        await _users.AddAsync(user, ct);

        // Настройки по умолчанию
        await _settings.UpsertAsync(new UserSettings(
            UserId: user.Id,
            Theme: "Light",
            DefaultFont: "Segoe UI",
            DefaultFontSize: 14,
            AutoSaveEnabled: true,
            AutoSaveMinutes: 5), ct);

        _currentUser.SetCurrent(user.Id, isGuest: false);
        return AuthResult.Ok(user.Id, isGuest: false);
    }

    public async Task<AuthResult> EnterAsGuestAsync(CancellationToken ct = default)
    {
        var guest = await _users.GetGuestAsync(ct);
        if (guest is not null)
        {
            _currentUser.SetCurrent(guest.Id, isGuest: true);
            return AuthResult.Ok(guest.Id, isGuest: true);
        }

        var newGuest = new User(
            Id: Guid.NewGuid(),
            Login: GuestLogin,
            PasswordHash: string.Empty,
            CreatedAt: DateTimeOffset.UtcNow,
            IsGuest: true);

        await _users.AddAsync(newGuest, ct);

        await _settings.UpsertAsync(new UserSettings(
            UserId: newGuest.Id,
            Theme: "Light",
            DefaultFont: "Segoe UI",
            DefaultFontSize: 14,
            AutoSaveEnabled: true,
            AutoSaveMinutes: 5), ct);

        _currentUser.SetCurrent(newGuest.Id, isGuest: true);
        return AuthResult.Ok(newGuest.Id, isGuest: true);
    }
}