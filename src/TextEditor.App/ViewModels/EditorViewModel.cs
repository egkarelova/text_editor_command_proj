using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

using TextEditor.Core.Models;
using TextEditor.App.Services;

namespace TextEditor.App.ViewModels;

public partial class EditorViewModel : ViewModelBase
{
    private bool _isLoading;

    [ObservableProperty]
    private Document? _currentDocument;

    [ObservableProperty]
    private string _documentTitle = "Без названия";

    [ObservableProperty]
    private byte[]? _documentContent;

    [ObservableProperty]
    private bool _isDirty;

    public Guid? CurrentDocumentId => CurrentDocument?.Id;

    partial void OnDocumentContentChanged(byte[]? value)
    {
        if (_isLoading) return;
        IsDirty = true;
    }

    partial void OnDocumentTitleChanged(string value)
    {
        if (_isLoading) return;
        IsDirty = true;
    }

    public void LoadDocument(Document doc, byte[] content)
    {
        _isLoading = true;
        try
        {
            CurrentDocument = doc;
            DocumentTitle = doc.Title;
            DocumentContent = content;
            IsDirty = false;
        }
        finally { _isLoading = false; }
    }

    public void MarkSaved(Document doc)
    {
        CurrentDocument = doc;
        IsDirty = false;
    }

    public bool ContainsImages()
    {
        if (DocumentContent is null or { Length: 0 }) return false;

        var doc = new FlowDocument();
        var range = new TextRange(doc.ContentStart, doc.ContentEnd);
        using var stream = new MemoryStream(DocumentContent);
        range.Load(stream, DataFormats.XamlPackage);
        return doc.ContainsImages();
    }

    public Document BuildDocumentForSave(Guid userId)
    {
        var baseDoc = CurrentDocument;
        if (baseDoc is null)
        {
            return new Document(
                Id: Guid.NewGuid(),
                UserId: userId,
                Title: DocumentTitle,
                Format: DocumentFormat.Txt,
                CreatedAt: DateTimeOffset.UtcNow,
                UpdatedAt: DateTimeOffset.UtcNow,
                LastOpenedAt: DateTimeOffset.UtcNow,
                IsFavorite: false,
                IsShared: false);
        }
        return baseDoc with { Title = DocumentTitle, UpdatedAt = DateTimeOffset.UtcNow };
    }
}