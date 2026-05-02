using System.Net;

namespace TodoList.Server
{
    internal class Program
    {
        private const string Prefix = "http://localhost:5000/";
        private const string DataDirectory = "server_data";
        private const string ProfilesFileName = "server_profiles.dat";

        private static async Task Main()
        {
            Directory.CreateDirectory(DataDirectory);

            using var listener = new HttpListener();
            listener.Prefixes.Add(Prefix);
            listener.Start();

            Console.WriteLine($"Server started: {Prefix}");

            while (true)
            {
                var context = await listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                string path = context.Request.Url?.AbsolutePath.Trim('/') ?? string.Empty;
                string method = context.Request.HttpMethod.ToUpperInvariant();

                if (path == "profiles" && method == "POST")
                {
                    await SaveRequestBodyAsync(context, GetProfilesPath());
                    await WriteTextAsync(context.Response, "OK", HttpStatusCode.OK);
                    return;
                }

                if (path == "profiles" && method == "GET")
                {
                    await WriteFileAsync(context.Response, GetProfilesPath());
                    return;
                }

                if (path.StartsWith("todos/", StringComparison.OrdinalIgnoreCase))
                {
                    string userId = path.Substring("todos/".Length);
                    string filePath = GetTodosPath(userId);

                    if (method == "POST")
                    {
                        await SaveRequestBodyAsync(context, filePath);
                        await WriteTextAsync(context.Response, "OK", HttpStatusCode.OK);
                        return;
                    }

                    if (method == "GET")
                    {
                        await WriteFileAsync(context.Response, filePath);
                        return;
                    }
                }

                await WriteTextAsync(context.Response, "Not found", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                await WriteTextAsync(context.Response, ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private static async Task SaveRequestBodyAsync(HttpListenerContext context, string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await context.Request.InputStream.CopyToAsync(fileStream);
        }

        private static async Task WriteFileAsync(HttpListenerResponse response, string filePath)
        {
            if (!File.Exists(filePath))
            {
                await WriteTextAsync(response, string.Empty, HttpStatusCode.NotFound);
                return;
            }

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/octet-stream";

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await fileStream.CopyToAsync(response.OutputStream);
            response.Close();
        }

        private static async Task WriteTextAsync(HttpListenerResponse response, string text, HttpStatusCode statusCode)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            response.StatusCode = (int)statusCode;
            response.ContentType = "text/plain; charset=utf-8";
            await response.OutputStream.WriteAsync(bytes);
            response.Close();
        }

        private static string GetProfilesPath()
        {
            return Path.Combine(DataDirectory, ProfilesFileName);
        }

        private static string GetTodosPath(string userId)
        {
            return Path.Combine(DataDirectory, $"server_todos_{userId}.dat");
        }
    }
}
