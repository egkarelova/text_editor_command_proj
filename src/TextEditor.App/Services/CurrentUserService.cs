using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TextEditor.Core.Services;

namespace TextEditor.App.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    public Guid UserId { get; private set; } = Guid.Empty;
    public bool IsGuest { get; private set; }
    public bool IsAuthenticated => UserId != Guid.Empty && !IsGuest;

    public void SetCurrent(Guid userId, bool isGuest)
    {
        UserId = userId;
        IsGuest = isGuest;
    }

    public void Clear()
    {
        UserId = Guid.Empty;
        IsGuest = false;
    }
}