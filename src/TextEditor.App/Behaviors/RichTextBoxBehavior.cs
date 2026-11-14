using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TextEditor.App.Behaviors;

/// Использование в XAML:
///   <RichTextBox local:RichTextBoxBehavior.Document="{Binding DocumentContent, Mode=TwoWay}" />
///
/// В ViewModel:
///   [ObservableProperty] private byte[]? _documentContent;

public static class RichTextBoxBehavior
{
    /// <summary>Защита от рекурсии: TextChanged → SetValue → OnDocumentChanged → Load → TextChanged.</summary>
    [ThreadStatic]
    private static bool _isUpdating;

    public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.RegisterAttached(
            "Document",
            typeof(byte[]),
            typeof(RichTextBoxBehavior),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDocumentChanged));

    public static byte[]? GetDocument(DependencyObject obj)
        => (byte[]?)obj.GetValue(DocumentProperty);

    public static void SetDocument(DependencyObject obj, byte[]? value)
        => obj.SetValue(DocumentProperty, value);

    private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RichTextBox richTextBox) return;
        if (_isUpdating) return;

        _isUpdating = true;
        try
        {
            // 1. Загружаем содержимое из ViewModel в RichTextBox
            if (e.NewValue is byte[] { Length: > 0 } data)
            {
                using var stream = new MemoryStream(data);
                var doc = new FlowDocument();
                var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                range.Load(stream, DataFormats.XamlPackage);
                richTextBox.Document = doc;
            }
            else
            {
                richTextBox.Document = new FlowDocument();
            }
        }
        finally
        {
            _isUpdating = false;
        }

        // 2. Подписываемся на изменения (один раз на контрол)
        richTextBox.TextChanged -= OnRichTextBoxTextChanged;
        richTextBox.TextChanged += OnRichTextBoxTextChanged;
    }

    private static void OnRichTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        if (sender is not RichTextBox richTextBox) return;

        _isUpdating = true;
        try
        {
            // 3. Сериализуем текущее содержимое RichTextBox в byte[] и отдаём во ViewModel
            using var stream = new MemoryStream();
            var range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            range.Save(stream, DataFormats.XamlPackage);
            SetDocument(richTextBox, stream.ToArray());
        }
        finally
        {
            _isUpdating = false;
        }
    }
}