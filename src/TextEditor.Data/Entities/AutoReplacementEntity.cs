using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Data.Entities;

public sealed class AutoReplacementEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Shortcut { get; set; } = string.Empty;
    public string Expansion { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}