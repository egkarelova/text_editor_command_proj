using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;

namespace TextEditor.Core.Services;

public interface ISettingsService
{
    Task<UserSettings> GetAsync(Guid userId, CancellationToken ct = default);
    Task SaveAsync(UserSettings settings, CancellationToken ct = default);
}