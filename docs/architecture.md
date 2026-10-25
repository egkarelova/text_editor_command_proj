\##B.1 Контекст и цели

Настольный текстовый редактор на WPF (.NET 8), хранящий документы и настройки в локальной SQLite через EF Core. Цель — MVP с возможностью расширения до функциональности, близкой к базовому Word (таблицы, списки, стили).



\##B.2 Стек и ключевые решения

UI: WPF, MVVM через CommunityToolkit.Mvvm



DI: Microsoft.Extensions.DependencyInjection в App.xaml.cs



Данные: EF Core 8 + SQLite, миграции



Хранение документа: XamlPackage (BLOB) — надёжно сохраняет форматирование и изображения



Импорт/экспорт: .rtf, .txt (через TextRange.Load/Save)



Тесты: xUnit



Темы: ResourceDictionary в App, имя активной темы — в БД



\##B.3 Слои и зависимости

┌───────────────────────────────────────────────┐

│  TextEditor.App (WPF)                         │

│  Views (XAML), ViewModels, Themes, DI-старт,  │

│  WPF-зависимые сервисы (FormatConverter,      │

│  ImageInserter, ThemeService)                 │

└──────────────────┬────────────────────────────┘

&#x20;                  │ использует

┌──────────────────▼────────────────────────────┐

│  TextEditor.Core (домен + сервисы)            │

│  Модели: Document, AppSetting                 │

│  Интерфейсы: IDocumentService,                │

│  ISettingsService, IDocumentRepository,       │

│  ISettingsRepository, IThemeService,          │

│  IImageService                                │

└──────────────────┬────────────────────────────┘

&#x20;                  │ использует интерфейсы

┌──────────────────▼────────────────────────────┐

│  TextEditor.Data (EF Core + SQLite)           │

│  AppDbContext, DocumentRepository,            │

│  SettingsRepository, миграции                 │

└───────────────────────────────────────────────┘

Правило: App не обращается к AppDbContext напрямую — только через сервисы Core. Data не знает про WPF.



\##B.4 Структура решения

\##B.5 Схема БД v1



Поле	Тип	Ограничения	Комментарий

Id	TEXT (GUID)	PK	

Title	TEXT	NOT NULL, max 200	Имя документа

Content	BLOB	NULL	XamlPackage

CreatedAt	TEXT (ISO)	NOT NULL	

ModifiedAt	TEXT (ISO)	NOT NULL	

LastOpenedAt	TEXT (ISO)	NULL	Для Recent

IsFavorite	INTEGER	NOT NULL, default 0	

AppSettings



Поле	Тип	Ограничения

Key	TEXT	PK

Value	TEXT	NOT NULL

Ключи настроек: theme, default.font, default.fontSize, lastDocumentId.



\##B.6 Ключевые интерфейсы (сигнатуры)

csharp

// TextEditor.Core/Models

public sealed record Document(

&#x20;   Guid Id,

&#x20;   string Title,

&#x20;   byte\[]? Content,

&#x20;   DateTimeOffset CreatedAt,

&#x20;   DateTimeOffset ModifiedAt,

&#x20;   DateTimeOffset? LastOpenedAt,

&#x20;   bool IsFavorite);



public enum ExportFormat { Rtf, Txt }



// TextEditor.Core/Services/IDocumentService.cs

public interface IDocumentService

{

&#x20;   Task<Document> CreateNewAsync(string title, CancellationToken ct = default);

&#x20;   Task<Document> OpenAsync(Guid documentId, CancellationToken ct = default);

&#x20;   Task SaveAsync(Document document, CancellationToken ct = default);

&#x20;   Task DeleteAsync(Guid documentId, CancellationToken ct = default);

&#x20;   Task<IReadOnlyList<Document>> GetRecentAsync(int count, CancellationToken ct = default);

}



// TextEditor.Core/Services/ISettingsService.cs

public interface ISettingsService

{

&#x20;   Task<string?> GetAsync(string key, CancellationToken ct = default);

&#x20;   Task<string> GetAsync(string key, string defaultValue, CancellationToken ct = default);

&#x20;   Task SetAsync(string key, string value, CancellationToken ct = default);

}



// TextEditor.Core/Services/IThemeService.cs

public interface IThemeService

{

&#x20;   string CurrentTheme { get; }

&#x20;   IReadOnlyList<string> AvailableThemes { get; }

&#x20;   event EventHandler<string>? ThemeChanged;

&#x20;   Task ApplyAsync(string themeName, CancellationToken ct = default);

}



// TextEditor.Core/Repositories/IDocumentRepository.cs

public interface IDocumentRepository

{

&#x20;   Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default);

&#x20;   Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);

&#x20;   Task<Document> AddAsync(Document document, CancellationToken ct = default);

&#x20;   Task UpdateAsync(Document document, CancellationToken ct = default);

&#x20;   Task DeleteAsync(Guid id, CancellationToken ct = default);

&#x20;   Task<IReadOnlyList<Document>> GetRecentAsync(int count, CancellationToken ct = default);

}



// TextEditor.Core/Repositories/ISettingsRepository.cs

public interface ISettingsRepository

{

&#x20;   Task<string?> GetAsync(string key, CancellationToken ct = default);

&#x20;   Task SetAsync(string key, string value, CancellationToken ct = default);

}



// TextEditor.App/Services/IDocumentFormatService.cs (WPF-зависимая, работает через TextRange)

public interface IDocumentFormatService

{

&#x20;   // читает .rtf/.txt и возвращает XamlPackage bytes для хранения в БД

&#x20;   Task<byte\[]> ImportAsync(string filePath, CancellationToken ct = default);

&#x20;   // сохраняет XamlPackage bytes наружу в указанный формат

&#x20;   Task ExportAsync(byte\[] xamlPackage, string filePath, ExportFormat format, CancellationToken ct = default);

}

Осознанно не вводим IImageService на MVP: вставка картинки — это операция над FlowDocument, реализуется в ImageInserter внутри App; в БД картинки попадают как часть XamlPackage. Если позже понадобится отдельное хранение — расширим.



\##B.7 Основные сценарии (data flow)

Сохранение документа



text

MainViewModel → IDocumentService.SaveAsync(doc)

&#x20;  → IDocumentRepository.UpdateAsync(doc)

&#x20;  → AppDbContext.SaveChangesAsync

Content в doc заполняется EditorViewModel через TextRange.Save(stream, DataFormats.XamlPackage).



Открытие документа



text

MainViewModel → IDocumentService.OpenAsync(id)  → Document (bytes)

&#x20;  → EditorViewModel: stream = new MemoryStream(bytes)

&#x20;  → TextRange.Load(stream, DataFormats.XamlPackage)

&#x20;  → обновить LastOpenedAt через репозиторий

Применение темы



text

View (Settings) → IThemeService.ApplyAsync("Dark")

&#x20;  → Application.Current.Resources.MergedDictionaries заменить на DarkTheme.xaml

&#x20;  → событие ThemeChanged → SettingsService.SetAsync("theme", "Dark")

\##B.8 Правила и договорённости

App не обращается к AppDbContext напрямую.



Data не ссылается на WPF.



Все публичные сервисы — через интерфейсы, регистрируются в DI в App.xaml.cs.



Асинхронность: async/await до конца, CancellationToken в сигнатурах сервисов.



Модели Core — record/record class; EF-сущности — отдельные классы в Data с маппингом.



Логирование — Microsoft.Extensions.Logging (минимум для MVP, но заранее заложено).

