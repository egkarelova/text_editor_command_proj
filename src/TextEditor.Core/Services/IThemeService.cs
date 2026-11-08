using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Core.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    IReadOnlyList<string> AvailableThemes { get; }
    event EventHandler<string>? ThemeChanged;
    Task ApplyAsync(string themeName, CancellationToken ct = default);
}