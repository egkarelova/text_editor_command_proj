using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Core.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    bool IsGuest { get; }
    bool IsAuthenticated { get; }
    void SetCurrent(Guid userId, bool isGuest);
    void Clear();
}