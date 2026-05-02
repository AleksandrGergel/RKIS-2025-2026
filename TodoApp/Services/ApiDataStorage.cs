using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TodoApp.Exceptions;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class ApiDataStorage : IDataStorage
    {
        private readonly HttpClient _httpClient;

        public ApiDataStorage(string baseAddress)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public bool IsAvailable()
        {
            try
            {
                using var response = _httpClient.GetAsync("profiles").GetAwaiter().GetResult();
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        public void SaveProfiles(IEnumerable<Profile> profiles)
        {
            var dto = new List<ProfileDto>();
            foreach (var profile in profiles)
            {
                dto.Add(new ProfileDto
                {
                    Id = profile.Id,
                    Login = profile.Login,
                    Password = profile.Password,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    BirthYear = profile.BirthYear
                });
            }

            PostEncrypted("profiles", dto);
        }

        public IEnumerable<Profile> LoadProfiles()
        {
            var dto = GetEncrypted<List<ProfileDto>>("profiles");
            var profiles = new List<Profile>();

            foreach (var profile in dto)
            {
                profiles.Add(new Profile
                {
                    Id = profile.Id,
                    Login = profile.Login,
                    Password = profile.Password,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    BirthYear = profile.BirthYear
                });
            }

            return profiles;
        }

        public void SaveTodos(Guid userId, IEnumerable<TodoItem> todos)
        {
            var dto = new List<TodoItemDto>();
            foreach (var todo in todos)
            {
                dto.Add(new TodoItemDto
                {
                    Text = todo.Text,
                    Status = todo.Status,
                    LastUpdate = todo.LastUpdate
                });
            }

            PostEncrypted($"todos/{userId}", dto);
        }

        public IEnumerable<TodoItem> LoadTodos(Guid userId)
        {
            var dto = GetEncrypted<List<TodoItemDto>>($"todos/{userId}");
            var todos = new List<TodoItem>();

            foreach (var item in dto)
            {
                todos.Add(new TodoItem(item.Text)
                {
                    Status = item.Status,
                    LastUpdate = item.LastUpdate
                });
            }

            return todos;
        }

        private void PostEncrypted<T>(string url, T value)
        {
            try
            {
                string json = JsonSerializer.Serialize(value);
                byte[] encrypted = EncryptionService.Encrypt(json);
                using var content = new ByteArrayContent(encrypted);
                using var response = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is JsonException || ex is System.Security.Cryptography.CryptographicException)
            {
                throw new DataStorageException("Не удалось отправить данные на сервер.", ex);
            }
        }

        private T GetEncrypted<T>(string url) where T : new()
        {
            try
            {
                using var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new T();
                }

                response.EnsureSuccessStatusCode();
                byte[] encrypted = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                if (encrypted.Length == 0)
                {
                    return new T();
                }

                string json = EncryptionService.Decrypt(encrypted);
                return JsonSerializer.Deserialize<T>(json) ?? new T();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is JsonException || ex is System.Security.Cryptography.CryptographicException)
            {
                throw new DataStorageException("Не удалось получить данные с сервера.", ex);
            }
        }

        private class ProfileDto
        {
            public Guid Id { get; set; }
            public string Login { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public int BirthYear { get; set; }
        }

        private class TodoItemDto
        {
            public string Text { get; set; } = string.Empty;
            public TodoStatus Status { get; set; }
            public DateTime LastUpdate { get; set; }
        }
    }
}
