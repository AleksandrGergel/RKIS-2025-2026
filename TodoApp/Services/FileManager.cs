using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TodoApp.Exceptions;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class FileManager : IDataStorage
    {
        private const string ProfilesFileName = "profiles.dat";

        private static readonly byte[] Key = Encoding.UTF8.GetBytes("TodoAppStorageKeyForAes256Data!!");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("TodoAppInitVect!");

        private readonly string _dataDirectory;

        public FileManager(string dataDirectory)
        {
            if (string.IsNullOrWhiteSpace(dataDirectory))
            {
                throw new InvalidArgumentException("Путь к хранилищу не может быть пустым.");
            }

            _dataDirectory = dataDirectory;
            EnsureDataDirectory();
        }

        public void SaveProfiles(IEnumerable<Profile> profiles)
        {
            try
            {
                using var writer = CreateEncryptedWriter(GetProfilesPath());
                foreach (var profile in profiles)
                {
                    writer.WriteLine(SerializeProfile(profile));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataStorageException("Нет доступа к файлу профилей.", ex);
            }
            catch (IOException ex)
            {
                throw new DataStorageException("Не удалось сохранить профили.", ex);
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException("Не удалось зашифровать профили.", ex);
            }
        }

        public IEnumerable<Profile> LoadProfiles()
        {
            var profiles = new List<Profile>();
            string path = GetProfilesPath();

            if (!File.Exists(path))
            {
                return profiles;
            }

            try
            {
                using var reader = CreateEncryptedReader(path);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    profiles.Add(DeserializeProfile(line));
                }

                return profiles;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataStorageException("Нет доступа к файлу профилей.", ex);
            }
            catch (IOException ex)
            {
                throw new DataStorageException("Не удалось прочитать профили.", ex);
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException("Не удалось расшифровать профили. Данные повреждены или ключ неверный.", ex);
            }
            catch (FormatException ex)
            {
                throw new DataStorageException("Файл профилей повреждён.", ex);
            }
        }

        public void SaveTodos(Guid userId, IEnumerable<TodoItem> todos)
        {
            try
            {
                using var writer = CreateEncryptedWriter(GetTodosPath(userId));
                foreach (var item in todos)
                {
                    writer.WriteLine(SerializeTodo(item));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataStorageException("Нет доступа к файлу задач.", ex);
            }
            catch (IOException ex)
            {
                throw new DataStorageException("Не удалось сохранить задачи.", ex);
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException("Не удалось зашифровать задачи.", ex);
            }
        }

        public IEnumerable<TodoItem> LoadTodos(Guid userId)
        {
            var todos = new List<TodoItem>();
            string path = GetTodosPath(userId);

            if (!File.Exists(path))
            {
                return todos;
            }

            try
            {
                using var reader = CreateEncryptedReader(path);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    todos.Add(DeserializeTodo(line));
                }

                return todos;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataStorageException("Нет доступа к файлу задач.", ex);
            }
            catch (IOException ex)
            {
                throw new DataStorageException("Не удалось прочитать задачи.", ex);
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException("Не удалось расшифровать задачи. Данные повреждены или ключ неверный.", ex);
            }
            catch (FormatException ex)
            {
                throw new DataStorageException("Файл задач повреждён.", ex);
            }
        }

        private void EnsureDataDirectory()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        private StreamWriter CreateEncryptedWriter(string path)
        {
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            var bufferedStream = new BufferedStream(fileStream);
            var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            var cryptoStream = new CryptoStream(bufferedStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            return new StreamWriter(cryptoStream, Encoding.UTF8);
        }

        private StreamReader CreateEncryptedReader(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var bufferedStream = new BufferedStream(fileStream);
            var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            var cryptoStream = new CryptoStream(bufferedStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            return new StreamReader(cryptoStream, Encoding.UTF8);
        }

        private string GetProfilesPath()
        {
            return Path.Combine(_dataDirectory, ProfilesFileName);
        }

        private string GetTodosPath(Guid userId)
        {
            return Path.Combine(_dataDirectory, $"todos_{userId}.dat");
        }

        private static string SerializeProfile(Profile profile)
        {
            return string.Join(";",
                profile.Id,
                Escape(profile.Login),
                Escape(profile.Password),
                Escape(profile.FirstName),
                Escape(profile.LastName),
                profile.BirthYear);
        }

        private static Profile DeserializeProfile(string line)
        {
            var parts = ParseLine(line);
            if (parts.Count != 6
                || !Guid.TryParse(parts[0], out var id)
                || !int.TryParse(parts[5], out var birthYear))
            {
                throw new FormatException("Некорректная строка профиля.");
            }

            return new Profile
            {
                Id = id,
                Login = Unescape(parts[1]),
                Password = Unescape(parts[2]),
                FirstName = Unescape(parts[3]),
                LastName = Unescape(parts[4]),
                BirthYear = birthYear
            };
        }

        private static string SerializeTodo(TodoItem item)
        {
            return string.Join(";",
                Escape(item.Text),
                item.Status,
                item.LastUpdate.ToString("O"));
        }

        private static TodoItem DeserializeTodo(string line)
        {
            var parts = ParseLine(line);
            if (parts.Count != 3
                || !Enum.TryParse<TodoStatus>(parts[1], out var status)
                || !DateTime.TryParse(parts[2], out var lastUpdate))
            {
                throw new FormatException("Некорректная строка задачи.");
            }

            return new TodoItem(Unescape(parts[0]))
            {
                Status = status,
                LastUpdate = lastUpdate
            };
        }

        private static string Escape(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        private static string Unescape(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        private static List<string> ParseLine(string line)
        {
            return new List<string>(line.Split(';'));
        }
    }
}
