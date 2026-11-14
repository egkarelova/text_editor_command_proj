using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TextEditor.Core.Models;
using TextEditor.Core.Repositories;
using TextEditor.Data.Entities;

namespace TextEditor.Data.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private const int MaxContentBytes = 1024 * 1024;   // 1 МБ

    private readonly AppDbContext _db;
    public DocumentRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var entities = await _db.Documents
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Document>> GetSharedAsync(CancellationToken ct = default)
    {
        var entities = await _db.Documents
            .AsNoTracking()
            .Where(x => x.IsShared)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Document>> GetAccessibleAsync(Guid userId, CancellationToken ct = default)
    {
        var entities = await _db.Documents
            .AsNoTracking()
            .Where(x => x.UserId == userId || x.IsShared)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<Document> AddAsync(Document document, byte[] content, CancellationToken ct = default)
    {
        EnsureSizeLimit(content);

        var entity = new DocumentRegistryEntity
        {
            Id = document.Id,
            UserId = document.UserId,
            Title = document.Title,
            Format = FormatToString(document.Format),
            IsFavorite = document.IsFavorite,
            IsShared = document.IsShared,
            LastOpenedAt = document.LastOpenedAt?.ToString("O", CultureInfo.InvariantCulture),
            CreatedAt = document.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
            UpdatedAt = document.UpdatedAt.ToString("O", CultureInfo.InvariantCulture),
            Content = new DocumentContentEntity
            {
                DocumentId = document.Id,
                Content = content,
                ByteSize = content.Length
            }
        };

        _db.Documents.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToDomain(entity);
    }

    public async Task UpdateAsync(Document document, CancellationToken ct = default)
    {
        var entity = await _db.Documents.FirstOrDefaultAsync(x => x.Id == document.Id, ct)
            ?? throw new InvalidOperationException($"Документ {document.Id} не найден.");

        entity.Title = document.Title;
        entity.Format = FormatToString(document.Format);
        entity.IsFavorite = document.IsFavorite;
        entity.IsShared = document.IsShared;
        entity.LastOpenedAt = document.LastOpenedAt?.ToString("O", CultureInfo.InvariantCulture);
        entity.UpdatedAt = document.UpdatedAt.ToString("O", CultureInfo.InvariantCulture);

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateContentAsync(Guid id, byte[] content, CancellationToken ct = default)
    {
        EnsureSizeLimit(content);

        var contentEntity = await _db.DocumentContents
            .FirstOrDefaultAsync(x => x.DocumentId == id, ct);

        if (contentEntity is null)
        {
            _db.DocumentContents.Add(new DocumentContentEntity
            {
                DocumentId = id,
                Content = content,
                ByteSize = content.Length
            });
        }
        else
        {
            contentEntity.Content = content;
            contentEntity.ByteSize = content.Length;
        }

        var registry = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (registry is not null)
            registry.UpdatedAt = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return;
        _db.Documents.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> GetRecentAsync(
        Guid userId, int count, CancellationToken ct = default)
    {
        var entities = await _db.Documents
            .AsNoTracking()
            .Where(x => (x.UserId == userId || x.IsShared) && x.LastOpenedAt != null)
            .OrderByDescending(x => x.LastOpenedAt)
            .Take(count)
            .ToListAsync(ct);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<byte[]?> GetContentAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.DocumentContents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DocumentId == id, ct);
        return entity?.Content;
    }

    public async Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken ct = default)
    {
        var max = await _db.DocumentVersions
            .Where(x => x.DocumentId == documentId)
            .MaxAsync(x => (int?)x.VersionNumber, ct) ?? 0;
        return max + 1;
    }

    public async Task AddVersionAsync(
        Guid documentId, byte[] content, Guid userId, CancellationToken ct = default)
    {
        EnsureSizeLimit(content);

        var next = await GetNextVersionNumberAsync(documentId, ct);
        _db.DocumentVersions.Add(new DocumentVersionEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            VersionNumber = next,
            Content = content,
            ByteSize = content.Length,
            CreatedAt = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            CreatedBy = userId
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task TrimVersionsAsync(Guid documentId, int keep, CancellationToken ct = default)
    {
        var toDelete = await _db.DocumentVersions
            .Where(x => x.DocumentId == documentId)
            .OrderByDescending(x => x.VersionNumber)
            .Skip(keep)
            .ToListAsync(ct);

        if (toDelete.Count == 0) return;
        _db.DocumentVersions.RemoveRange(toDelete);
        await _db.SaveChangesAsync(ct);
    }

    // --- Mapping ---

    private static Document ToDomain(DocumentRegistryEntity e) => new(
        Id: e.Id,
        UserId: e.UserId,
        Title: e.Title,
        Format: FormatFromString(e.Format),
        CreatedAt: ParseDate(e.CreatedAt),
        UpdatedAt: ParseDate(e.UpdatedAt),
        LastOpenedAt: string.IsNullOrEmpty(e.LastOpenedAt) ? null : ParseDate(e.LastOpenedAt),
        IsFavorite: e.IsFavorite,
        IsShared: e.IsShared);

    private static string FormatToString(DocumentFormat f)
        => f == DocumentFormat.Rtf ? "rtf" : "txt";

    private static DocumentFormat FormatFromString(string s)
        => string.Equals(s, "rtf", StringComparison.OrdinalIgnoreCase)
            ? DocumentFormat.Rtf
            : DocumentFormat.Txt;

    private static DateTimeOffset ParseDate(string s)
        => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private static void EnsureSizeLimit(byte[] content)
    {
        if (content.Length > MaxContentBytes)
            throw new InvalidOperationException(
                $"Содержимое превышает лимит {MaxContentBytes} байт (фактически: {content.Length}).");
    }
}