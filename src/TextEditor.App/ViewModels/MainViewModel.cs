using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TextEditor.Core.Models;
using TextEditor.Core.Services;

namespace TextEditor.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDocumentService _documents;
    private readonly IFileDialogService _dialogs;
    private readonly IDocumentFormatService _format;
    private readonly IThemeService _themes;
    private readonly ICurrentUserService _currentUser;

    public EditorViewModel Editor { get; }

    public MainViewModel(
        IDocumentService documents,
        IFileDialogService dialogs,
        IDocumentFormatService format,
        IThemeService themes,
        ICurrentUserService currentUser,
        EditorViewModel editor)
    {
        _documents = documents;
        _dialogs = dialogs;
        _format = format;
        _themes = themes;
        _currentUser = currentUser;
        Editor = editor;
    }

    [ObservableProperty]
    private string _statusText = "Готово";

    // ================= File =================

    [RelayCommand]
    private async Task NewAsync()
    {
        if (!await ConfirmDiscardChangesAsync()) return;

        var doc = await _documents.CreateNewAsync(
            _currentUser.UserId,
            title: "Без названия",
            format: DocumentFormat.Txt);

        Editor.LoadDocument(doc, Array.Empty<byte>());
        StatusText = $"Создан: {doc.Title}";
    }

    [RelayCommand]
    private async Task OpenAsync()
    {
        if (!await ConfirmDiscardChangesAsync()) return;

        var accessible = await _documents.GetAccessibleAsync(_currentUser.UserId);
        if (accessible.Count == 0)
        {
            MessageBox.Show(
                "Нет доступных документов.",
                "Открыть", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var pickedId = _dialogs.PickDocument(accessible);
        if (pickedId is null) return;

        var doc = await _documents.OpenAsync(pickedId.Value);
        if (doc is null)
        {
            MessageBox.Show(
                "Документ не найден.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var content = await _documents.GetContentAsync(doc.Id) ?? Array.Empty<byte>();
        Editor.LoadDocument(doc, content);
        StatusText = $"Открыт: {doc.Title}";
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (Editor.CurrentDocumentId is null)
        {
            await SaveAsAsync();
            return;
        }

        await PersistCurrentAsync();
        StatusText = $"Сохранено: {Editor.DocumentTitle}";
    }

    private bool CanSave() => Editor.IsDirty && Editor.CurrentDocumentId is not null;

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        var hasImages = Editor.ContainsImages();
        var path = _dialogs.PickSavePath(Editor.DocumentTitle, includeTxt: !hasImages);
        if (path is null) return;

        var format = DetectFormat(path);
        var content = Editor.DocumentContent ?? Array.Empty<byte>();

        // 1. Экспорт наружу (.rtf/.txt)
        await _format.ExportAsync(content, path, format);

        // 2. Если документ ещё не в БД — создаём запись
        if (Editor.CurrentDocumentId is null)
        {
            var title = Path.GetFileNameWithoutExtension(path);
            var created = await _documents.CreateNewAsync(_currentUser.UserId, title, format);
            Editor.LoadDocument(created, content);
        }

        // 3. Сохраняем содержимое в БД
        await PersistCurrentAsync();
        Editor.MarkSaved(Editor.CurrentDocument!);
        StatusText = $"Сохранено как: {Path.GetFileName(path)}";
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        if (!await ConfirmDiscardChangesAsync()) return;
        Application.Current.Shutdown();
    }

    // ================= View =================

    [RelayCommand]
    private Task ApplyLightThemeAsync() => _themes.ApplyAsync("Light");

    [RelayCommand]
    private Task ApplyDarkThemeAsync() => _themes.ApplyAsync("Dark");

    // ================= Help =================

    [RelayCommand]
    private void ShowAbout()
    {
        var w = new Views.AboutWindow { Owner = Application.Current.MainWindow };
        w.ShowDialog();
    }

    // ================= Internals =================

    private async Task PersistCurrentAsync()
    {
        var doc = Editor.BuildDocumentForSave(_currentUser.UserId);
        var content = Editor.DocumentContent ?? Array.Empty<byte>();
        await _documents.SaveAsync(doc, content, _currentUser.UserId);
        Editor.MarkSaved(doc);
    }

    private async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (!Editor.IsDirty) return true;

        var result = MessageBox.Show(
            "В текущем документе есть несохранённые изменения.\n\n" +
            "Сохранить перед продолжением?",
            "Несохранённые изменения",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Cancel) return false;
        if (result == MessageBoxResult.Yes)
        {
            await SaveAsync();
            return !Editor.IsDirty;   // если Save отменили — не продолжаем
        }
        return true;
    }

    private static DocumentFormat DetectFormat(string path)
        => Path.GetExtension(path).Equals(".rtf", StringComparison.OrdinalIgnoreCase)
            ? DocumentFormat.Rtf
            : DocumentFormat.Txt;
}