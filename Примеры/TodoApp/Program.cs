using System;
using System.IO;
using System.Linq;
using TodoApp.Commands;
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
            while (true)
            {
                Console.WriteLine("Войти в существующий профиль? [y/n]");
                Console.Write("> ");

                string choice = Console.ReadLine()?.ToLower() ?? "n";

                if (choice == "y")
                {
                    return LoginProfile();
                }
                else if (choice == "n")
                {
                    return CreateProfile();
                }
                else
                {
                    Console.WriteLine("Пожалуйста, введите 'y' или 'n'");
                }
            }
        }

        private static bool LoginProfile()
        {
            if (AppInfo.Profiles.Count == 0)
            {
                Console.WriteLine("Нет сохранённых профилей. Пожалуйста, создайте новый.");
                return false;
            }

            Console.Write("Логин: ");
            string login = Console.ReadLine() ?? "";

            Console.Write("Пароль: ");
            string password = Console.ReadLine() ?? "";

            var profile = FileManager.LoadProfile(login, password);

            if (profile != null)
            {
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

            Console.WriteLine("Неверный логин или пароль.");
            return LoginProfile();
        }

        private static bool CreateProfile()
        {
            Console.Write("Логин: ");
            string login = Console.ReadLine() ?? "";

            if (AppInfo.Profiles.Any(p => p.Login == login))
            {
                Console.WriteLine("Этот логин уже занят.");
                return false;
            }

            Console.Write("Пароль: ");
            string password = Console.ReadLine() ?? "";

            Console.Write("Имя: ");
            string firstName = Console.ReadLine() ?? "";

            Console.Write("Фамилия: ");
            string lastName = Console.ReadLine() ?? "";

            Console.Write("Год рождения: ");
            if (!int.TryParse(Console.ReadLine(), out int birthYear))
            {
                Console.WriteLine("Неверный формат года.");
                return false;
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
					if (!SelectOrCreateProfile())
					{
						return;
					}

					Console.WriteLine($"\nДобро пожаловать, {AppInfo.CurrentProfile?.FirstName}!\n");
				}

				Console.Write("> ");
                string input = Console.ReadLine() ?? "";

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("До свидания!");
                    break;
                }

                ICommand command = CommandParser.Parse(input);
                command.Execute();

                if (command is IUndoableCommand undoableCmd)
                {
                    AppInfo.UndoStack.Push(undoableCmd);
                    AppInfo.RedoStack.Clear();
                }
            }
        }
    }
}
