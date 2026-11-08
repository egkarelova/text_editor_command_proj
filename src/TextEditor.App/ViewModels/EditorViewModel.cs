using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TextEditor.App.ViewModels;

public partial class EditorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _documentTitle = "Untitled";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private Guid? _currentDocumentId;

    [ObservableProperty]
    private byte[]? _documentContent;

    // TODO: Александр. Методы для сериализации/десериализации FlowDocument
    // через TextRange.Save/Load с DataFormats.XamlPackage, binding RichTextBox.Document.
}