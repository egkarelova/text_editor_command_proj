using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;
using TextEditor.Core.Services;

namespace TextEditor.App.Services;

// TODO: Айдана. Использовать TextRange.Load/Save с DataFormats.Rtf / DataFormats.Text
// для конвертации в/из XamlPackage.
public sealed class DocumentFormatService : IDocumentFormatService
{
    public Task<byte[]> ImportAsync(string filePath, CancellationToken ct = default)
        => throw new NotImplementedException("Sprint 1 — Dev A");

    public Task ExportAsync(byte[] xamlPackage, string filePath, DocumentFormat format, CancellationToken ct = default)
        => throw new NotImplementedException("Sprint 1 — Dev A");
}