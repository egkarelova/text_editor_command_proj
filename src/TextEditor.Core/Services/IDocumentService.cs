using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TextEditor.Core.Models;

namespace TextEditor.Core.Services;

public interface IDocumentService
{
    Task<Document> CreateNewAsync(Guid userId, string title, DocumentFormat format, CancellationToken ct = default);
    Task<Document?> OpenAsync(Guid documentId, CancellationToken ct = default);
    Task<byte[]?> GetContentAsync(Guid documentId, CancellationToken ct = default);
    Task SaveAsync(Document document, byte[] content, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetRecentAsync(Guid userId, int count, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetAccessibleAsync(Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid documentId, CancellationToken ct = default);
}