using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Data.Entities;

public sealed class UserSettingsEntity
{
    public Guid UserId { get; set; }
    public string Theme { get; set; } = "Light";
    public string DefaultFont { get; set; } = "Segoe UI";
    public int DefaultFontSize { get; set; } = 14;
    public bool AutoSaveEnabled { get; set; } = true;
    public int AutoSaveMinutes { get; set; } = 5;

    public UserEntity User { get; set; } = null!;
}