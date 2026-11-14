-- 1. Пользователи
CREATE TABLE Users (
    Id            TEXT PRIMARY KEY,           -- GUID
    Login         TEXT NOT NULL UNIQUE,
    PasswordHash  TEXT NOT NULL,              -- PBKDF2 / BCrypt
    CreatedAt     TEXT NOT NULL
);

-- 2. Настройки пользователя (1:1)
CREATE TABLE UserSettings (
    UserId          TEXT PRIMARY KEY REFERENCES Users(Id) ON DELETE CASCADE,
    Theme           TEXT NOT NULL DEFAULT 'Light',
    DefaultFont     TEXT NOT NULL DEFAULT 'Segoe UI',
    DefaultFontSize INTEGER NOT NULL DEFAULT 14,
    AutoSaveEnabled INTEGER NOT NULL DEFAULT 1,
    AutoSaveMinutes INTEGER NOT NULL DEFAULT 5
);

-- 3. Метаданные документа / реестр (лёгкая таблица для списков)
CREATE TABLE DocumentRegistry (
    Id            TEXT PRIMARY KEY,           -- GUID
    UserId        TEXT NOT NULL REFERENCES Users(Id),
    Title         TEXT NOT NULL,
    Format        TEXT NOT NULL CHECK (Format IN ('txt','rtf')),
    IsFavorite    INTEGER NOT NULL DEFAULT 0,
    LastOpenedAt  TEXT,
    CreatedAt     TEXT NOT NULL,
    UpdatedAt     TEXT NOT NULL
);
CREATE INDEX IX_DocumentRegistry_User_Updated
    ON DocumentRegistry(UserId, UpdatedAt DESC);

-- 4. Содержимое документа (тяжёлый BLOB, грузится только при открытии)
CREATE TABLE DocumentContents (
    DocumentId  TEXT PRIMARY KEY REFERENCES DocumentRegistry(Id) ON DELETE CASCADE,
    Content     BLOB NOT NULL,                -- XamlPackage (рекомендуется) или RTF bytes
    ByteSize    INTEGER NOT NULL,             -- для проверки лимита 1 МБ
    CHECK (ByteSize <= 1048576)
);

-- 5. Версии документа (последние N версий; N=5 задаётся логикой приложения)
CREATE TABLE DocumentVersions (
    Id            TEXT PRIMARY KEY,
    DocumentId    TEXT NOT NULL REFERENCES DocumentRegistry(Id) ON DELETE CASCADE,
    VersionNumber INTEGER NOT NULL,
    Content       BLOB NOT NULL,              -- снимок содержимого
    ByteSize      INTEGER NOT NULL,
    CreatedAt     TEXT NOT NULL,
    CreatedBy     TEXT NOT NULL REFERENCES Users(Id),
    UNIQUE (DocumentId, VersionNumber)
);
CREATE INDEX IX_DocumentVersions_Document_Version
    ON DocumentVersions(DocumentId, VersionNumber DESC);

-- 6. Автозамены (словарь пользователя)
CREATE TABLE AutoReplacements (
    Id          TEXT PRIMARY KEY,
    UserId      TEXT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Shortcut    TEXT NOT NULL,                -- например, "LOREM"
    Expansion   TEXT NOT NULL,                -- полный текст
    IsEnabled   INTEGER NOT NULL DEFAULT 1,
    UNIQUE (UserId, Shortcut)
);

-- 7. Запрещённые слова (общие для всех или для пользователя)
CREATE TABLE ForbiddenWords (
    Id        TEXT PRIMARY KEY,
    Word      TEXT NOT NULL UNIQUE,           -- если общий список
    CreatedAt TEXT NOT NULL
);
