using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using AppLauncher.Classes.Core_Classes;
using AppLauncher.Views.Windows;

namespace AppLauncher.Classes.MainClasses
{
    public class UpdaterService
    {


        private readonly string _currentVersion;
        public MainWindow win;
        private readonly string machineName = "HIDE";
        private readonly string userName = "HIDE";
        private static readonly HttpClient client = new HttpClient();
        private readonly string baseUrl = "<YOUR-URL(WORKER)>";
        public UpdaterService(string currentVersion, MainWindow main)
        {
            win = main;
            _currentVersion = currentVersion;
        }
        public async Task CheckForUpdateAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("AppLauncher-VersionCheck");
                var url = "https://api.github.com/repos/{your-username}/{repo}/releases/latest";
                var res = await client.GetAsync(url);
                res.EnsureSuccessStatusCode();

                using var stream = await res.Content.ReadAsStreamAsync();
                var release = await JsonSerializer.DeserializeAsync<GitHubRelease>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (release != null && !string.IsNullOrWhiteSpace(release.Tag_name))
                {
                    var githubVersionStr = release.Tag_name.TrimStart('v');
                    var currentVersionStr = _currentVersion.TrimStart('v');

                    if (Version.TryParse(githubVersionStr, out var githubVersion) &&
                        Version.TryParse(currentVersionStr, out var currentVersion))
                    {
                        if (githubVersion > currentVersion)
                        {
                            win.isUpdateAvailable = true;
                            NotificationWindow notif = new NotificationWindow("Update Available",
                                $"New version available: {release.Tag_name}\nCurrent version: {_currentVersion}",
                                MessageBoxImage.Information);

                            notif.ShowNotification();
                            await Task.Delay(3000).ContinueWith(_ =>
                            {
                                Application.Current.Dispatcher.Invoke(() => notif.CloseNotification());
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckForUpdateAsync: {ex.Message}");
            }
        }

        public async Task<string> SendMessageAsync(string message, string source = "", string priority = "normal")
        {
            try
            {
                source = string.IsNullOrEmpty(source) ? $"{userName} - {machineName}" : source;

                var payload = new
                {
                    message = message,
                    priority = priority,
                    source = source
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseUrl, content);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;

                return $"Send Avtivision is OFF {{ Message : {message} }}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
