using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

using TextEditor.Core.Models;
using TextEditor.Core.Services;

namespace TextEditor.App.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string? PickSavePath(string suggestedFileName, bool includeTxt)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить документ",
            FileName = suggestedFileName,
            Filter = includeTxt
                ? "Rich Text (*.rtf)|*.rtf|Текстовый файл (*.txt)|*.txt"
                : "Rich Text (*.rtf)|*.rtf",
            DefaultExt = ".rtf"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PickOpenPath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Открыть файл",
            Filter = "Rich Text (*.rtf)|*.rtf|Текстовый файл (*.txt)|*.txt|Все файлы (*.*)|*.*"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }


    public Guid? PickDocument(IReadOnlyList<Document> documents)
    => documents.Count > 0 ? documents[0].Id : null;
    //public Guid? PickDocument(IReadOnlyList<Document> documents)
    //{
    //    var picker = new Views.DocumentPickerWindow(documents);
    //    return picker.ShowDialog() == true ? picker.SelectedDocumentId : null;
    //}
}
