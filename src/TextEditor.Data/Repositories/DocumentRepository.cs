using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;
using TextEditor.Core.Repositories;

namespace TextEditor.Data.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _db;
    public DocumentRepository(AppDbContext db) => _db = db;

    public Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task<Document> AddAsync(Document document, byte[] content, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task UpdateAsync(Document document, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task UpdateContentAsync(Guid id, byte[] content, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task<IReadOnlyList<Document>> GetRecentAsync(Guid userId, int count, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task<byte[]?> GetContentAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task AddVersionAsync(Guid documentId, byte[] content, Guid userId, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");

    public Task TrimVersionsAsync(Guid documentId, int keep, CancellationToken ct = default)
        => throw new NotImplementedException("Егор");
}