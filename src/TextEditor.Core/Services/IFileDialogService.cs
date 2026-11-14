using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TextEditor.Core.Models;

namespace TextEditor.Core.Services;

public interface IFileDialogService
{
    string? PickSavePath(string suggestedFileName, bool includeTxt);
    string? PickOpenPath();
    Guid? PickDocument(IReadOnlyList<Document> documents);
}
