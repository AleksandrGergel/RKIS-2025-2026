# TodoApp

Консольное приложение для работы со списком задач и профилями пользователей.

## Хранение данных

Приложение переключено с файлового хранения на реляционную базу данных SQLite через Entity Framework Core.

- База данных: `todos.db`
- Контекст: `Data/AppDbContext.cs`
- Модели: `Models/Profile.cs`, `Models/TodoItem.cs`
- Репозитории: `Services/ProfileRepository.cs`, `Services/TodoRepository.cs`
- Миграции: `Migrations/`

`FileManager.cs` больше не используется для хранения задач и удалён из проекта.

## EF Core

Используемые пакеты:

```powershell
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
```

Команды миграций:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Команды приложения

```text
help
profile [-o]
add "текст"
add -m
view [-i] [-s] [-d] [-a]
read <idx>
status <idx> <статус>
update <idx> "новый текст"
delete <idx>
search [флаги]
load <count> <size>
sync --pull
sync --push
undo
redo
exit
```

После перезапуска программы профили и задачи загружаются из `todos.db`.
