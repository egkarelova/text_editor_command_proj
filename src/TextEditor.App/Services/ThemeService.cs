using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Services;

namespace TextEditor.App.Services;

// TODO: Наргиз с Александром. Реализовать замену MergedDictionaries.
public sealed class ThemeService : IThemeService
{
    public string CurrentTheme { get; private set; } = "Light";
    public IReadOnlyList<string> AvailableThemes { get; } = new[] { "Light", "Dark" };

    public event EventHandler<string>? ThemeChanged;

    public Task ApplyAsync(string themeName, CancellationToken ct = default)
    {
        if (!AvailableThemes.Contains(themeName))
            throw new ArgumentException($"Unknown theme: {themeName}", nameof(themeName));

        // TODO: заменить MergedDictionaries в Application.Current.Resources
        CurrentTheme = themeName;
        ThemeChanged?.Invoke(this, themeName);
        return Task.CompletedTask;
    }
}