using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Core.Models;

public sealed record AuthResult(
    bool Success,
    Guid UserId,
    bool IsGuest,
    string? ErrorMessage = null)
{
    public static AuthResult Ok(Guid userId, bool isGuest) => new(true, userId, isGuest);
    public static AuthResult Fail(string message) => new(false, Guid.Empty, false, message);
}
