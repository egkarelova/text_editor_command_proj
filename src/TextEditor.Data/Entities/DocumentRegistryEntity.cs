using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Data.Entities;

public sealed class DocumentRegistryEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Format { get; set; } = "txt"; // "txt" | "rtf"
    public bool IsFavorite { get; set; }
    public string? LastOpenedAt { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

    public UserEntity User { get; set; } = null!;
    public DocumentContentEntity? Content { get; set; }
    public ICollection<DocumentVersionEntity> Versions { get; set; } = new List<DocumentVersionEntity>();
}