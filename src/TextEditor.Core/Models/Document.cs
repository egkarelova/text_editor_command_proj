using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;

public sealed record Document(
    Guid Id,
    Guid UserId,
    string Title,
    DocumentFormat Format,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastOpenedAt,
    bool IsFavorite,
    bool IsShared);