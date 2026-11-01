using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.App.Services;
/// Проверка FlowDocument на наличие изображений (ADR-007).
/// Используется перед сохранением в .txt — если есть картинки, формат запрещён.

public static class FlowDocumentExtensions
{
    public static bool ContainsImages(this FlowDocument document)
        => FindImages(document).Any();

    private static IEnumerable<Image> FindImages(FlowDocument document)
    {
        foreach (var block in document.Blocks)
            foreach (var image in FindImagesInBlock(block))
                yield return image;
    }

    private static IEnumerable<Image> FindImagesInBlock(Block block)
    {
        if (block is Paragraph paragraph)
        {
            foreach (var inline in paragraph.Inlines)
                foreach (var image in FindImagesInInline(inline))
                    yield return image;
        }
        else if (block is Table table)
        {
            foreach (var rowGroup in table.RowGroups)
                foreach (var row in rowGroup.Rows)
                    foreach (var cell in row.Cells)
                        foreach (var cellBlock in cell.Blocks)
                            foreach (var image in FindImagesInBlock(cellBlock))
                                yield return image;
        }
    }

    private static IEnumerable<Image> FindImagesInInline(Inline inline)
    {
        switch (inline)
        {
            case InlineUIContainer { Child: Image img }:
                yield return img;
                break;
            case Span span:
                foreach (var child in span.Inlines)
                    foreach (var image in FindImagesInInline(child))
                        yield return image;
                break;
        }
    }
}