using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Core.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Task InitializeAsync(CancellationToken ct = default);
}