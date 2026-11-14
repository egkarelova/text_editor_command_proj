using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Data.Entities;

public sealed class ForbiddenWordEntity
{
    public Guid Id { get; set; }
    public string Word { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}