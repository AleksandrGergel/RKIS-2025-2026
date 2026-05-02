using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TodoApp.Commands;
using TodoApp.Models;

namespace TodoApp.Services
{
    public static class CommandParser
    {
        private static Dictionary<string, Func<string[], ICommand>> _commandHandlers;

        static CommandParser()
        {
            InitializeHandlers();
        }

        private static void InitializeHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<string[], ICommand>>
            {
                ["help"] = args => new HelpCommand(),
                ["profile"] = args => ParseProfileCommand(args),
                ["add"] = args => ParseAddCommand(args),
                ["view"] = args => ParseViewCommand(args),
                ["read"] = args => ParseReadCommand(args),
                ["status"] = args => ParseStatusCommand(args),
                ["update"] = args => ParseUpdateCommand(args),
                ["delete"] = args => ParseDeleteCommand(args),
                ["undo"] = args => new UndoCommand(),
                ["redo"] = args => new RedoCommand(),
            };
        }

        public static ICommand Parse(string inputString)
        {
            if (string.IsNullOrWhiteSpace(inputString))
            {
                return new HelpCommand();
            }

            var parts = SplitCommand(inputString);
            if (parts.Length == 0)
                return new HelpCommand();

            string command = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            if (_commandHandlers.ContainsKey(command))
            {
                try
                {
                    return _commandHandlers[command](args);
                }
                catch
                {
                    Console.WriteLine($"Ошибка при выполнении команды '{command}'");
                    return new HelpCommand();
                }
            }

            Console.WriteLine($"Неизвестная команда: '{command}'. Введите 'help' для справки.");
            return new HelpCommand();
        }

        private static ICommand ParseProfileCommand(string[] args)
        {
            bool logout = args.Any(a => a == "-o" || a == "--out");
            return new ProfileCommand(logout);
        }

        private static ICommand ParseAddCommand(string[] args)
        {
            bool isMultiline = args.Any(a => a == "-m" || a == "--multiline");

            if (isMultiline)
            {
                return new AddCommand("", true);
            }

            string text = string.Join(" ", args);
            text = text.Trim('"');

            return new AddCommand(text, false);
        }

        private static ICommand ParseViewCommand(string[] args)
        {
            bool showIndex = args.Any(a => a == "-i" || a == "--index");
            bool showStatus = args.Any(a => a == "-s" || a == "--status");
            bool showDate = args.Any(a => a == "-d" || a == "--update-date");
            bool showAll = args.Any(a => a == "-a" || a == "--all");

            if (showAll)
                return new ViewCommand(true, true, true);

            return new ViewCommand(showIndex, showStatus, showDate);
        }

        private static ICommand ParseReadCommand(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out int index))
            {
                return new ReadCommand(index);
            }

            Console.WriteLine("Используйте: read <индекс>");
            return new HelpCommand();
        }

        private static ICommand ParseStatusCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Используйте: status <индекс> <статус>");
                return new HelpCommand();
            }

            if (!int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Индекс должен быть числом.");
                return new HelpCommand();
            }

            string statusStr = args[1].ToLower();
            if (Enum.TryParse<TodoStatus>(statusStr, ignoreCase: true, out var status))
            {
                return new StatusCommand(index, status);
            }

            Console.WriteLine("Неизвестный статус. Доступные: NotStarted, InProgress, Completed, Postponed, Failed");
            return new HelpCommand();
        }

        private static ICommand ParseUpdateCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Используйте: update <индекс> \"новый текст\"");
                return new HelpCommand();
            }

            if (!int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Индекс должен быть числом.");
                return new HelpCommand();
            }

            string newText = string.Join(" ", args.Skip(1)).Trim('"');
            return new UpdateCommand(index, newText);
        }

        private static ICommand ParseDeleteCommand(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Используйте: delete <индекс>");
                return new HelpCommand();
            }

            return new DeleteCommand(index);
        }

        private static string[] SplitCommand(string input)
        {
            var result = new List<string>();
            var regex = new Regex(@"[^\s""]+|""([^""]*)""");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Groups[1].Success)
                {
                    result.Add(match.Groups[1].Value);
                }
                else
                {
                    result.Add(match.Value);
                }
            }

            return result.ToArray();
        }
    }
}
