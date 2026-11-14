using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;

namespace TextEditor.Core.Services;


public interface IDocumentFormatService
{
    Task<byte[]> ImportAsync(string filePath, CancellationToken ct = default);

    Task ExportAsync(byte[] xamlPackage, string filePath, DocumentFormat format, CancellationToken ct = default);
}
