using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;

namespace TextEditor.Core.Repositories;

public interface IDocumentRepository
{
    Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Document> AddAsync(Document document, byte[] content, CancellationToken ct = default);
    Task UpdateAsync(Document document, CancellationToken ct = default);
    Task UpdateContentAsync(Guid id, byte[] content, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetRecentAsync(Guid userId, int count, CancellationToken ct = default);

    Task<byte[]?> GetContentAsync(Guid id, CancellationToken ct = default);

    // Версионирование (ADR-005)
    Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken ct = default);
    Task AddVersionAsync(Guid documentId, byte[] content, Guid userId, CancellationToken ct = default);
    Task TrimVersionsAsync(Guid documentId, int keep, CancellationToken ct = default);
}