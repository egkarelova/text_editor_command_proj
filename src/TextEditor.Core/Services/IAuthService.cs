using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TextEditor.Core.Models;

namespace TextEditor.Core.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string login, string password, CancellationToken ct = default);
    Task<AuthResult> RegisterAsync(string login, string password, CancellationToken ct = default);
    Task<AuthResult> EnterAsGuestAsync(CancellationToken ct = default);
}