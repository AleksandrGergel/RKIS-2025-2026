using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TodoApp.Commands;
using TodoApp.Data;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp
{
    class Program
    {
        private static readonly ProfileRepository ProfileRepository = new();
        private static readonly TodoRepository TodoRepository = new();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();

            EnsureDatabaseReady();

            AppInfo.Profiles = ProfileRepository.GetAll();
            MainLoop();
        }

        private static void EnsureDatabaseReady()
        {
            using var context = new AppDbContext();
            context.Database.OpenConnection();

            try
            {
                bool profilesTableExists = TableExists(context, "Profiles");
                bool todosTableExists = TableExists(context, "Todos");

                if (profilesTableExists != todosTableExists)
                {
                    throw new DataStorageException("Структура базы данных повреждена: таблицы Profiles и Todos должны существовать вместе.");
                }

                if (profilesTableExists && todosTableExists)
                {
                    string currentMigrationId = "20260502235345_InitialCreate";
                    string productVersion = typeof(DbContext).Assembly
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                        .InformationalVersion
                        ?.Split('+')[0]
                        ?? "8.0.6";

                    context.Database.ExecuteSqlRaw(
                        @"CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                            ""MigrationId"" TEXT NOT NULL CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY,
                            ""ProductVersion"" TEXT NOT NULL
                        );");

                    context.Database.ExecuteSql(
                        $@"INSERT OR IGNORE INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                           VALUES ({currentMigrationId}, {productVersion});");
                }
            }
            finally
            {
                context.Database.CloseConnection();
            }

            context.Database.Migrate();
        }

        private static bool TableExists(AppDbContext context, string tableName)
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName;";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "$tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            object? result = command.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

        private static bool SelectOrCreateProfile()
        {
            Console.WriteLine("Войти в существующий профиль? [y/n]");
            Console.Write("> ");

            string choice = (Console.ReadLine() ?? string.Empty).Trim().ToLower();

            if (choice == "y")
            {
                return LoginProfile();
            }

            if (choice == "n")
            {
                return CreateProfile();
            }

            throw new InvalidArgumentException("Введите 'y' или 'n'.");
        }

        private static bool LoginProfile()
        {
            if (AppInfo.Profiles.Count == 0)
            {
                throw new ProfileNotFoundException("Нет сохранённых профилей. Создайте новый профиль.");
            }

            Console.Write("Логин: ");
            string login = (Console.ReadLine() ?? string.Empty).Trim();

            Console.Write("Пароль: ");
            string password = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidArgumentException("Логин и пароль не могут быть пустыми.");
            }

            var profile = ProfileRepository.GetByCredentials(login, password);
            if (profile == null)
            {
                throw new AuthenticationException("Неверный логин или пароль.");
            }

            AppInfo.CurrentProfile = profile;
            AppInfo.Profiles = ProfileRepository.GetAll();
            AppInfo.UserTodos[profile.Id] = CreateTodoList(TodoRepository.GetAll(profile.Id));
            SubscribeToTodoEvents(profile.Id, AppInfo.UserTodos[profile.Id]);

            AppInfo.ClearUndoRedo();
            return true;
        }

        private static bool CreateProfile()
        {
            Console.Write("Логин: ");
            string login = (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(login))
            {
                throw new InvalidArgumentException("Логин не может быть пустым.");
            }

            if (ProfileRepository.LoginExists(login))
            {
                throw new DuplicateLoginException("Этот логин уже занят.");
            }

            Console.Write("Пароль: ");
            string password = Console.ReadLine() ?? string.Empty;

            Console.Write("Имя: ");
            string firstName = (Console.ReadLine() ?? string.Empty).Trim();

            Console.Write("Фамилия: ");
            string lastName = (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                throw new InvalidArgumentException("Пароль, имя и фамилия не могут быть пустыми.");
            }

            Console.Write("Год рождения: ");
            if (!int.TryParse(Console.ReadLine(), out int birthYear) || birthYear <= 0 || birthYear > DateTime.Now.Year)
            {
                throw new InvalidArgumentException("Год рождения должен быть корректным числом.");
            }

            var profile = new Profile(login, password, firstName, lastName, birthYear);
            ProfileRepository.Add(profile);

            AppInfo.Profiles = ProfileRepository.GetAll();
            AppInfo.CurrentProfile = profile;
            AppInfo.UserTodos[profile.Id] = new TodoList();
            SubscribeToTodoEvents(profile.Id, AppInfo.UserTodos[profile.Id]);

            AppInfo.ClearUndoRedo();
            return true;
        }

        private static TodoList CreateTodoList(IEnumerable<TodoItem> items)
        {
            var todoList = new TodoList();
            foreach (var item in items)
            {
                todoList.Add(item);
            }

            return todoList;
        }

        private static void SubscribeToTodoEvents(Guid profileId, TodoList todoList)
        {
            todoList.OnTodoAdded += item =>
            {
                item.ProfileId = profileId;
                item.Profile = null;
                TodoRepository.Add(item);
            };

            todoList.OnTodoDeleted += item =>
            {
                TodoRepository.Delete(item.Id);
            };

            todoList.OnTodoUpdated += item =>
            {
                item.ProfileId = profileId;
                item.Profile = null;
                TodoRepository.Update(item);
            };

            todoList.OnStatusChanged += item =>
            {
                item.ProfileId = profileId;
                item.Profile = null;
                TodoRepository.Update(item);
            };
        }

        private static void MainLoop()
        {
            while (true)
            {
                if (AppInfo.CurrentProfile is null)
                {
                    try
                    {
                        if (!SelectOrCreateProfile())
                        {
                            return;
                        }

                        Console.WriteLine($"\nДобро пожаловать, {AppInfo.CurrentProfile?.FirstName}!\n");
                    }
                    catch (ProfileNotFoundException ex)
                    {
                        Console.WriteLine($"Ошибка профиля: {ex.Message}");
                        continue;
                    }
                    catch (DuplicateLoginException ex)
                    {
                        Console.WriteLine($"Ошибка регистрации: {ex.Message}");
                        continue;
                    }
                    catch (AuthenticationException ex)
                    {
                        Console.WriteLine($"Ошибка авторизации: {ex.Message}");
                        continue;
                    }
                    catch (InvalidArgumentException ex)
                    {
                        Console.WriteLine($"Ошибка аргумента: {ex.Message}");
                        continue;
                    }
                    catch (DataStorageException ex)
                    {
                        Console.WriteLine($"Ошибка хранения данных: {ex.Message}");
                        continue;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Неожиданная ошибка.");
                        continue;
                    }
                }

                Console.Write("> ");
                string input = Console.ReadLine() ?? string.Empty;

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("До свидания!");
                    break;
                }

                try
                {
                    var command = CommandParser.Parse(input);
                    command.Execute();

                    if (command is IUndoableCommand undoableCmd)
                    {
                        AppInfo.UndoStack.Push(undoableCmd);
                        AppInfo.RedoStack.Clear();
                    }
                }
                catch (TaskNotFoundException ex)
                {
                    Console.WriteLine($"Ошибка задачи: {ex.Message}");
                }
                catch (AuthenticationException ex)
                {
                    Console.WriteLine($"Ошибка авторизации: {ex.Message}");
                }
                catch (InvalidCommandException ex)
                {
                    Console.WriteLine($"Ошибка команды: {ex.Message}");
                }
                catch (InvalidArgumentException ex)
                {
                    Console.WriteLine($"Ошибка аргумента: {ex.Message}");
                }
                catch (DataStorageException ex)
                {
                    Console.WriteLine($"Ошибка хранения данных: {ex.Message}");
                }
                catch (Exception)
                {
                    Console.WriteLine("Неожиданная ошибка.");
                }
            }
        }
    }
}
