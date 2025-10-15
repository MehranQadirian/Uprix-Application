using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AppLauncher.Classes;
using AppLauncher.Classes.MainClasses;

namespace AppLauncher.Views.Windows
{
    public partial class UpdateWindow : Window
    {
        #region Variables
        private MainWindow mainWindow;
        private UpdaterService updaterService;
        public string CurrentVersion = "v3.0.0.0";
        private new const string Owner = "MehranQadirian"; 
        private const string Repo = "Uprix-Application"; 
        private const string AssetNameFilter = "Setup";
        private static readonly HttpClient Http = new HttpClient();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private double _progressMaxWidth; 
        private string _downloadedFile;
        private string userName = Environment.UserName;
        private ApplyThemes apply;
        #endregion
        #region Methods
        public UpdateWindow(MainWindow main , ApplyThemes aply)
        {
            InitializeComponent();
            apply = aply;
            mainWindow = main;
            updaterService = new UpdaterService(CurrentVersion, mainWindow);
            Loaded += UpdateWindow_Loaded;
            SizeChanged += (s, e) => _progressMaxWidth = GetProgressTrackWidth();
        }

        private void SetTheme()
        {
            brMain.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            Title.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            SubBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            SubBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundLightPart));
            brProgress.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundLightPart));
            brProgress.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ProgressFill.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            CancelButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            CancelButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
        }

        private async void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetTheme();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AppLauncher-VersionCheck");
            var url = "https://api.github.com/repos/MehranQadirian/Uprix-Application/releases/latest";
            var res = await client.GetAsync(url);
            res.EnsureSuccessStatusCode();

            using var stream = await res.Content.ReadAsStreamAsync();
            var release = await JsonSerializer.DeserializeAsync<GitHubReleaseUpdate>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _progressMaxWidth = GetProgressTrackWidth();
            SetIndeterminate(true, "Checking latest release…");

            try
            {
                var rel = await GetLatestReleaseAsync(_cts.Token);
                if (rel == null)
                {
                    SetIndeterminate(false, "No release found.");
                    return;
                }
                var githubVersionStr = rel.tag_name.TrimStart('v');
                var currentVersionStr = mainWindow.CurrentVersion.TrimStart('v');
                if (Version.TryParse(githubVersionStr, out var githubVersion) &&
    Version.TryParse(currentVersionStr, out var currentVersion))
                {
                    if (githubVersion <= currentVersion)
                    {
                        SetIndeterminate(false, $"You already have the latest version ({currentVersion}).");
                        await Task.Delay(5000);
                        Close();
                        return;
                    }
                }
                var asset = PickAsset(rel);
                if (asset == null)
                {
                    SetIndeterminate(false, "No suitable installer (.exe/.msi) in release.");
                    return;
                }

                StatusText.Text = $"Downloading: {asset.name}";
                SetIndeterminate(false, "Downloading…");

                var tempPath = Path.Combine(Path.GetTempPath(), asset.name);
                _downloadedFile = await DownloadAsync(asset.browser_download_url, tempPath, asset.size, _cts.Token);

                if (!File.Exists(_downloadedFile))
                {
                    StatusText.Text = "Download failed.";
                    return;
                }

                PercentText.Text = "100%";
                AnimateProgressTo(1.0);

                StatusText.Text = "Launching installer…";
                await Task.Delay(400);

                try
                {
                    if (_downloadedFile.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
                    {
                        var psi = new ProcessStartInfo("msiexec.exe", $"/i \"{_downloadedFile}\"")
                        {
                            UseShellExecute = true,
                            Verb = "runas"
                        };
                        Process.Start(psi);
                    }
                    else // .exe
                    {
                        var psi = new ProcessStartInfo(_downloadedFile)
                        {
                            UseShellExecute = true,
                            Verb = "runas"
                        };
                        Process.Start(psi);
                    }
                }
                catch (Exception ex)
                {
                    NotificationWindow notification = new NotificationWindow("Updater", "Failed to start installer:\n" + ex.Message, MessageBoxImage.Error);
                    notification.ShowNotification();
                    await Task.Delay(3000).ContinueWith(_ =>
                     {
                         Application.Current.Dispatcher.Invoke(() => notification.CloseNotification());
                     });
                    return;
                }


                Application.Current.Shutdown();
            }
            catch (OperationCanceledException)
            {
                StatusText.Text = "Canceled.";
            }
            catch (Exception ex)
            {
                NotificationWindow notification = new NotificationWindow("Update Error",  ex.Message, MessageBoxImage.Error);
                notification.ShowNotification();
                await Task.Delay(3000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() => notification.CloseNotification());
                });
                StatusText.Text = "Error.";
            }
        }

        private async Task<GitHubReleaseUpdate> GetLatestReleaseAsync(CancellationToken ct)
        {
            var url = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.UserAgent.ParseAdd("AppLauncher-Updater");

            using var res = await Http.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();

            using var stream = await res.Content.ReadAsStreamAsync();
            var rel = await JsonSerializer.DeserializeAsync<GitHubReleaseUpdate>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return rel;
        }

        private GitHubAsset PickAsset(GitHubReleaseUpdate rel)
        {
            if (rel?.assets == null || rel.assets.Length == 0) return null;

            var query = rel.assets.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(AssetNameFilter))
                query = query.Where(a => a.name.IndexOf(AssetNameFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            query = query.Where(a => a.name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                                     a.name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase));

            return query.OrderByDescending(a => a.size).FirstOrDefault();
        }

        private async Task<string> DownloadAsync(string url, string destPath, long expectedSize, CancellationToken ct)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.UserAgent.ParseAdd("AppLauncher-Updater");

            using var res = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            res.EnsureSuccessStatusCode();

            var total = expectedSize > 0 ? expectedSize : res.Content.Headers.ContentLength ?? -1;

            using (var httpStream = await res.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                var buffer = new byte[81920];
                long readTotal = 0;
                int read;

                UpdateProgress(0);

                while ((read = await httpStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    readTotal += read;

                    if (total > 0)
                    {
                        double p = (double)readTotal / total;
                        UpdateProgress(p);
                    }
                    else
                    {
                        var tick = Math.Min(0.95, (readTotal / (1024d * 1024d * 50d)));
                        UpdateProgress(tick);
                    }
                }

                UpdateProgress(1.0);
            }

            return destPath;
        }

        private void SetIndeterminate(bool on, string status)
        {
            StatusText.Text = status;
            IndeterminateShimmer.Visibility = on ? Visibility.Visible : Visibility.Collapsed;

            if (on)
            {
                PercentText.Text = "…";
                AnimateProgressTo(0.15, 250);
            }
        }

        private void UpdateProgress(double p)
        {
            Dispatcher.Invoke(() =>
            {
                var clamped = Math.Max(0, Math.Min(1, p));
                PercentText.Text = $"{Math.Floor(clamped * 100)}%";
                AnimateProgressTo(clamped);
            });
        }

        private void AnimateProgressTo(double fraction, int durationMs = 180)
        {
            var targetWidth = _progressMaxWidth * fraction;
            var anim = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            ProgressFill.BeginAnimation(WidthProperty, anim, HandoffBehavior.SnapshotAndReplace);
        }

        private double GetProgressTrackWidth()
        {
            var parent = ProgressFill.Parent as FrameworkElement;
            return parent?.ActualWidth > 0 ? parent.ActualWidth : 480;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try { _cts.Cancel(); } catch { }
            Close();
        }
        #endregion
    }

    public class GitHubReleaseUpdate
    {
        public string tag_name { get; set; }
        public GitHubAsset[] assets { get; set; }
    }

    public class GitHubAsset
    {
        public string name { get; set; }
        public string browser_download_url { get; set; }
        public long size { get; set; }
    }
}
