# Архитектура Text Editor

Документ описывает устройство приложения: слои, зависимости, схему БД, ключевые
интерфейсы и основные сценарии. Обоснование решений — в [`decisions.md`](decisions.md).

## 1. Контекст и цели

Настольный текстовый редактор на WPF (.NET 8) с хранением документов и настроек в
локальной SQLite. Цель MVP — минимально функциональный редактор с возможностью
расширения до базового Word (таблицы, списки, стили).

## 2. Стек и ключевые решения

- **UI:** WPF, MVVM через CommunityToolkit.Mvvm (ADR-001, ADR-009)
- **DI:** Microsoft.Extensions.Hosting, Generic Host (ADR-008)
- **Данные:** EF Core 8 + SQLite, миграции (ADR-002)
- **Внутренний формат документа:** XamlPackage (ADR-003)
- **Экспорт/импорт:** `.rtf`, `.txt` через `TextRange`
- **Темы:** ResourceDictionary в `App`, имя активной темы — в БД
- **Тесты:** xUnit
- **CI:** GitHub Actions (`dotnet restore/build/test`)

## 3. Слои и зависимости
┌───────────────────────────────────────────────┐
│ TextEditor.App (WPF) │
│ Views (XAML), ViewModels, Themes, DI-старт, │
│ WPF-зависимые сервисы (ThemeService, │
│ DocumentFormatService, ImageInserter) │
└──────────────────┬────────────────────────────┘
│ использует
┌──────────────────▼────────────────────────────┐
│ TextEditor.Core (домен + контракты) │
│ Модели: Document, User, UserSettings, │
│ Enums (DocumentFormat) │
│ Интерфейсы: IDocumentService, │
│ ISettingsService, IThemeService, │
│ IDocumentFormatService, │
│ IDocumentRepository, IUserRepository, │
│ ISettingsRepository │
└──────────────────┬────────────────────────────┘
│ использует контракты
┌──────────────────▼────────────────────────────┐
│ TextEditor.Data (EF Core + SQLite) │
│ AppDbContext, Entities, Repositories, │
│ Migrations, DesignTimeDbContextFactory │
└───────────────────────────────────────────────┘

**Правила:**
- `Core` **не зависит** от WPF и EF Core.
- `App` **не работает с `AppDbContext` напрямую** — только через сервисы `Core`.
- `Data` **не знает про WPF**.
- EF-сущности (`Data/Entities`) отделены от доменных моделей (`Core/Models`).

## 4. Структура решения
text-editor/
│
├── .editorconfig                     # Единый стиль кода для всей команды (C# + XAML)
├── .gitignore                        # bin/, obj/, *.db, .vs/
├── Directory.Build.props             # Общие настройки: TargetFramework, LangVersion, Nullable, TreatWarningsAsErrors
├── README.md                         # Краткое описание, стек, инструкция запуска, состав команды
├── TextEditor.sln                    # Solution-файл
│
├── .github/
│   ├── PULL_REQUEST_TEMPLATE.md      # Шаблон PR (что сделано, как проверить, скрин/видео)
│   ├── ISSUE_TEMPLATE/
│   │   ├── task.md                   # Шаблон задачи (assignee, reviewer, DoD)
│   │   └── bug.md                    # Шаблон бага
│   └── workflows/
│       └── ci.yml                    # dotnet restore → build → test на push в main и PR
│
├── docs/                             # Вся проектная документация (не код)
│   ├── architecture.md               # Архитектура: слои, зависимости, ключевые решения (заполнен, см. ниже)
│   ├── decisions.md                  # Журнал решений ADR-lite (заполнен, см. ниже)
│   ├── requirements.md               # User stories, MVP vs stretch
│   ├── database-schema.md            # Описание схемы БД + DDL (см. раздел 3)
│   ├── user-guide.md                 # Руководство пользователя (Dev E, Sprint 2+)
│   ├── dev-guide.md                  # Как собрать, тестировать, мигрировать БД (Dev E, Sprint 1)
│   └── test-plan.md                  # Чек-лист ручного QA (Dev E)
│
├── src/
│   ├── TextEditor.App/               # WPF-приложение (View + ViewModel + WPF-сервисы)
│   │   ├── App.xaml                  # Без StartupUri, ресурсы, темы
│   │   ├── App.xaml.cs               # Generic Host + DI + миграции при старте
│   │   ├── AssemblyInfo.cs
│   │   ├── GlobalUsings.cs           # Глобальные using для App
│   │   │
│   │   ├── Views/                    # Только XAML + code-behind (минимум логики)
│   │   │   ├── MainWindow.xaml
│   │   │   ├── MainWindow.xaml.cs
│   │   │   └── AboutWindow.xaml
│   │   │
│   │   ├── ViewModels/               # ViewModel (CommunityToolkit.Mvvm, без code-behind-логики)
│   │   │   ├── ViewModelBase.cs      # ObservableObject + IsBusy/ErrorMessage
│   │   │   ├── MainViewModel.cs      # Шелл: меню, команды, навигация
│   │   │   └── EditorViewModel.cs    # Состояние редактора, форматирование
│   │   │
│   │   ├── Services/                 # WPF-зависимые сервисы (работают с FlowDocument/TextRange)
│   │   │   ├── ThemeService.cs       # IThemeService: ResourceDictionary, событие ThemeChanged
│   │   │   ├── DocumentFormatService.cs # IDocumentFormatService: import/export .rtf/.txt
│   │   │   └── ImageInserter.cs      # Вставка изображений в FlowDocument
│   │   │
│   │   ├── Behaviors/                # Attached behaviors (RichTextBox binding)
│   │   ├── Converters/               # IValueConverter
│   │   └── Themes/
│   │       ├── LightTheme.xaml
│   │       └── DarkTheme.xaml
│   │
│   ├── TextEditor.Core/              # Домен + интерфейсы сервисов (НЕ зависит от WPF и EF)
│   │   ├── Models/
│   │   │   ├── Document.cs           # record: Id, Title, Content, Format, ...
│   │   │   ├── User.cs               # record: Id, Login, PasswordHash
│   │   │   ├── UserSettings.cs       # record: Theme, Font, AutoSave...
│   │   │   └── Enums.cs              # DocumentFormat { Txt, Rtf }
│   │   │
│   │   ├── Services/                 # Только интерфейсы (реализации — в App или Data)
│   │   │   ├── IDocumentService.cs
│   │   │   ├── ISettingsService.cs
│   │   │   ├── IThemeService.cs
│   │   │   └── IDocumentFormatService.cs
│   │   │
│   │   ├── Repositories/             # Интерфейсы репозиториев (реализации — в Data)
│   │   │   ├── IDocumentRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   └── ISettingsRepository.cs
│   │   │
│   │   └── TextEditor.Core.csproj
│   │
│   └── TextEditor.Data/              # EF Core + SQLite (НЕ зависит от WPF)
│       ├── AppDbContext.cs           # DbSet'ы, OnModelCreating (конфигурации)
│       ├── DesignTimeDbContextFactory.cs # IDesignTimeDbContextFactory для миграций
│       ├── Entities/                 # EF-сущности (отдельно от доменных моделей Core)
│       │   ├── UserEntity.cs
│       │   ├── DocumentEntity.cs
│       │   ├── DocumentRegistryEntity.cs
│       │   ├── UserSettingsEntity.cs
│       │   ├── AutoReplaceEntity.cs
│       │   ├── ForbiddenWordEntity.cs
│       │   └── DocumentVersionEntity.cs
│       │
│       ├── Configurations/           # IEntityTypeConfiguration<T> для каждой сущности
│       ├── Repositories/             # Реализации интерфейсов из Core
│       ├── Migrations/               # Автогенерируемые миграции (не править руками)
│       └── TextEditor.Data.csproj
│
└── tests/
    └── TextEditor.Tests/             # xUnit
        ├── Services/
        │   ├── DocumentServiceTests.cs
        │   └── SettingsServiceTests.cs
        ├── Repositories/
        │   └── DocumentRepositoryTests.cs # in-memory SQLite
        └── TestHelpers/
            └── SqliteInMemoryFixture.cs


## 5. Схема БД

Обоснование разделения `DocumentRegistry` / `DocumentContents` — ADR-004.
Версионирование — ADR-005. `Format` — ADR-006.
DDL (SQLite) в в [`database-schema.md`](database-schema.md).


## 6. Основные сценарии (data flow)

#### 6.1. Сохранение документа
MainViewModel → EditorViewModel.SerializeFlowDocument()
   → byte[] (XamlPackage)
   → IDocumentService.SaveAsync(doc, bytes, userId)
   → IDocumentRepository: AddVersion (старое) → UpdateContent (новое) → Trim(5)
   → AppDbContext.SaveChangesAsync

#### 6.2. Открытие документа
MainViewModel → IDocumentService.OpenAsync(id) → Document
              → IDocumentService.GetContentAsync(id) → byte[]
   → EditorViewModel.LoadDocument(bytes)
   → TextRange.Load(stream, DataFormats.XamlPackage)
   → обновить LastOpenedAt

#### 6.3. Применение темы
View → IThemeService.ApplyAsync("Dark")
   → заменить MergedDictionaries на DarkTheme.xaml
   → событие ThemeChanged
   → ISettingsService.SaveAsync (Theme = "Dark")
