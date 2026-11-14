using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Core.Models;

public sealed record User(
    Guid Id,
    string Login,
    string PasswordHash,
    DateTimeOffset CreatedAt,
    bool IsGuest);
