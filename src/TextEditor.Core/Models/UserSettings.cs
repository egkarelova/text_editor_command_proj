using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Core.Models;

public sealed record UserSettings(
    Guid UserId,
    string Theme,
    string DefaultFont,
    int DefaultFontSize,
    bool AutoSaveEnabled,
    int AutoSaveMinutes);