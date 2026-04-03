using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TodoApp.Models;

namespace TodoApp.Services
{
    public static class FileManager
    {
        private const string DataDir = "data";
        private const string ProfilesFileName = "profiles.csv";

        public static void EnsureDataDirectory()
        {
            if (!Directory.Exists(DataDir))
            {
                Directory.CreateDirectory(DataDir);
            }
        }

        public static void SaveProfile(Profile profile)
        {
            EnsureDataDirectory();
            string filePath = Path.Combine(DataDir, ProfilesFileName);

            var profiles = LoadAllProfiles();
            var existing = profiles.FirstOrDefault(p => p.Id == profile.Id);

            if (existing != null)
            {
                profiles.Remove(existing);
            }
            profiles.Add(profile);

            SaveAllProfiles(profiles, filePath);
        }

        public static Profile LoadProfile(string login, string password)
        {
            var profiles = LoadAllProfiles();
            return profiles.FirstOrDefault(p => p.Login == login && p.Password == password);
        }

        public static List<Profile> LoadAllProfiles()
        {
            EnsureDataDirectory();
            string filePath = Path.Combine(DataDir, ProfilesFileName);

            var profiles = new List<Profile>();

            if (!File.Exists(filePath))
            {
                return profiles;
            }

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(';');
                if (parts.Length == 6)
                {
                    var profile = new Profile
                    {
                        Id = Guid.Parse(parts[0]),
                        Login = parts[1],
                        Password = parts[2],
                        FirstName = parts[3],
                        LastName = parts[4],
                        BirthYear = int.Parse(parts[5])
                    };
                    profiles.Add(profile);
                }
            }

            return profiles;
        }

        private static void SaveAllProfiles(List<Profile> profiles, string filePath)
        {
            var lines = new List<string>();
            foreach (var profile in profiles)
            {
                string line = $"{profile.Id};{profile.Login};{profile.Password};{profile.FirstName};{profile.LastName};{profile.BirthYear}";
                lines.Add(line);
            }
            File.WriteAllLines(filePath, lines);
        }

        public static void SaveTodos(TodoList todos, string filePath)
        {
            var lines = new List<string>();
            int index = 0;

            foreach (var item in todos.GetAll())
            {
                string escapedText = EscapeCsv(item.Text);
                string line = $"{index};{escapedText};{item.Status};{item.LastUpdate:yyyy-MM-ddTHH:mm:ss}";
                lines.Add(line);
                index++;
            }

            File.WriteAllLines(filePath, lines);
        }

        public static TodoList LoadTodos(string filePath)
        {
            var todos = new TodoList();

            if (!File.Exists(filePath))
            {
                return todos;
            }

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = ParseCsvLine(line);
                if (parts.Count >= 4)
                {
                    string text = UnescapeCsv(parts[1]);
                    var status = Enum.Parse<TodoStatus>(parts[2]);
                    DateTime.TryParse(parts[3], out DateTime lastUpdate);

                    var item = new TodoItem(text)
                    {
                        Status = status,
                        LastUpdate = lastUpdate
                    };
                    todos.Add(item);
                }
            }

            return todos;
        }

        private static string EscapeCsv(string text)
        {
            return "\"" + text.Replace("\"", "\"\"").Replace("\n", "\\n") + "\"";
        }

        private static string UnescapeCsv(string text)
        {
            return text.Trim('"').Replace("\\n", "\n").Replace("\"\"", "\"");
        }

        private static List<string> ParseCsvLine(string line)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if (c == ';' && !inQuotes)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            parts.Add(current.ToString());
            return parts;
        }

        public static string GetTodoFilePath(Guid profileId)
        {
            return Path.Combine(DataDir, $"todos_{profileId}.csv");
        }

        // Метод для подписки на события TodoList
        // Сохраняет текущий список задач при любом изменении
        public static void SaveTodoList(TodoItem item)
        {
            var profile = AppInfo.CurrentProfile;
            if (profile != null)
            {
                var todoList = AppInfo.GetCurrentTodoList();
                if (todoList != null)
                {
                    string filePath = GetTodoFilePath(profile.Id);
                    SaveTodos(todoList, filePath);
                }
            }
        }
    }
}
