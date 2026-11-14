using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;

namespace TextEditor.Core.Repositories;

public interface ISettingsRepository
{
    Task<UserSettings?> GetAsync(Guid userId, CancellationToken ct = default);
    Task UpsertAsync(UserSettings settings, CancellationToken ct = default);
}