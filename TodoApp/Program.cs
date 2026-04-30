using System;
using System.IO;
using System.Linq;
using TodoApp.Commands;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();

            FileManager.EnsureDataDirectory();
            AppInfo.Profiles = FileManager.LoadAllProfiles();

            MainLoop();
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

            var profile = FileManager.LoadProfile(login, password);
            if (profile == null)
            {
                throw new AuthenticationException("Неверный логин или пароль.");
            }

            AppInfo.CurrentProfile = profile;

            string todoPath = FileManager.GetTodoFilePath(profile.Id);
            if (File.Exists(todoPath))
            {
                AppInfo.UserTodos[profile.Id] = FileManager.LoadTodos(todoPath);
            }
            else
            {
                AppInfo.UserTodos[profile.Id] = new TodoList();
                FileManager.SaveTodos(AppInfo.UserTodos[profile.Id], todoPath);
            }

            var todoList = AppInfo.UserTodos[profile.Id];
            SubscribeToTodoEvents(todoList);

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

            if (AppInfo.Profiles.Any(p => p.Login == login))
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
            AppInfo.Profiles.Add(profile);
            FileManager.SaveProfile(profile);

            AppInfo.CurrentProfile = profile;
            AppInfo.UserTodos[profile.Id] = new TodoList();

            string todoPath = FileManager.GetTodoFilePath(profile.Id);
            FileManager.SaveTodos(AppInfo.UserTodos[profile.Id], todoPath);

            var todoList = AppInfo.UserTodos[profile.Id];
            SubscribeToTodoEvents(todoList);

            AppInfo.ClearUndoRedo();
            return true;
        }

        private static void SubscribeToTodoEvents(TodoList todoList)
        {
            todoList.OnTodoAdded += FileManager.SaveTodoList;
            todoList.OnTodoDeleted += FileManager.SaveTodoList;
            todoList.OnTodoUpdated += FileManager.SaveTodoList;
            todoList.OnStatusChanged += FileManager.SaveTodoList;
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
                catch (Exception)
                {
                    Console.WriteLine("Неожиданная ошибка.");
                }
            }
        }
    }
}
