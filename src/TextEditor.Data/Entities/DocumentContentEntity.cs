using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Data.Entities;

public sealed class DocumentContentEntity
{
    public Guid DocumentId { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public int ByteSize { get; set; }

    public DocumentRegistryEntity Document { get; set; } = null!;
}
