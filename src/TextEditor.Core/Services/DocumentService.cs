using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TextEditor.Core.Models;
using TextEditor.Core.Repositories;

namespace TextEditor.Core.Services;

public sealed class DocumentService : IDocumentService
{
    private const int KeepVersions = 5;

    private readonly IDocumentRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public DocumentService(IDocumentRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Document> CreateNewAsync(
        Guid userId, string title, DocumentFormat format, CancellationToken ct = default)
    {
        var doc = new Document(
            Id: Guid.NewGuid(),
            UserId: userId,
            Title: string.IsNullOrWhiteSpace(title) ? "Без названия" : title,
            Format: format,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow,
            LastOpenedAt: null,
            IsFavorite: false,
            IsShared: _currentUser.IsGuest);   // гостевые — общие (видны всем)

        return await _repository.AddAsync(doc, Array.Empty<byte>(), ct);
    }

    public async Task<Document?> OpenAsync(Guid documentId, CancellationToken ct = default)
    {
        var doc = await _repository.GetByIdAsync(documentId, ct);
        if (doc is null) return null;

        var updated = doc with { LastOpenedAt = DateTimeOffset.UtcNow };
        await _repository.UpdateAsync(updated, ct);
        return updated;
    }

    public Task<byte[]?> GetContentAsync(Guid documentId, CancellationToken ct = default)
        => _repository.GetContentAsync(documentId, ct);

    public async Task SaveAsync(
        Document document, byte[] content, Guid userId, CancellationToken ct = default)
    {
        // Сохраняем предыдущее содержимое как версию (если оно было непустым)
        var existing = await _repository.GetContentAsync(document.Id, ct);
        if (existing is { Length: > 0 })
        {
            await _repository.AddVersionAsync(document.Id, existing, userId, ct);
            await _repository.TrimVersionsAsync(document.Id, keep: KeepVersions, ct);
        }

        await _repository.UpdateContentAsync(document.Id, content, ct);
        await _repository.UpdateAsync(
            document with { UpdatedAt = DateTimeOffset.UtcNow }, ct);
    }

    public Task<IReadOnlyList<Document>> GetRecentAsync(
        Guid userId, int count, CancellationToken ct = default)
        => _repository.GetRecentAsync(userId, count, ct);

    public Task<IReadOnlyList<Document>> GetAccessibleAsync(
        Guid userId, CancellationToken ct = default)
        => _repository.GetAccessibleAsync(userId, ct);

    public Task DeleteAsync(Guid documentId, CancellationToken ct = default)
        => _repository.DeleteAsync(documentId, ct);
}
