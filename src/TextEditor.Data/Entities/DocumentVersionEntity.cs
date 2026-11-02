using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Data.Entities;


public sealed class DocumentVersionEntity
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public int ByteSize { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }

    public DocumentRegistryEntity Document { get; set; } = null!;
}