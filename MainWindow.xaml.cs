using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using IWshRuntimeLibrary;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Color = System.Windows.Media.Color;
using System.Text.Json;
using System.Net.Http;
using System.Windows.Threading;
using System.ComponentModel;

namespace AppLauncher
{
    public partial class MainWindow : Window
    {
        private const string CurrentVersion = "v2.0.0.1";
        private const double TILE_W = 160;
        private const double TILE_H = 160;
        private const double TILE_MARGIN = 10;
        private bool recalcScheduled = false;

        private readonly List<AppModel> allApps = new List<AppModel>();
        private List<AppModel> filteredApps = new List<AppModel>();
        private readonly Dictionary<string, ImageSource> iconCache = new Dictionary<string, ImageSource>();
        TextBlock appsName;
        private int currentPage = 0;
        private int itemsPerPage = 12;
        private bool isUpdateAvailable = false;
        private readonly Random rand = new Random();
        private Button selectedButton;

        public MainWindow()
        {
            InitializeComponent();
            SearchBox.Focus();
            this.StateChanged += Window_StateChanged;
            this.Closing += Window_Closing;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                RecalculatePagination();
                ClampPage();
                RenderCurrentPage();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
            
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
            LoadApps();
            ApplyFilterAndReset();
            RecalculatePagination();
            RenderCurrentPage();

            await CheckForUpdateAsync();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecalculatePagination();
            ClampPage();
            RenderCurrentPage();
        }

        private async Task CheckForUpdateAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("AppLauncher-VersionCheck");
                var url = "https://api.github.com/repos/MehranQadirian/Uprix-Application/releases/latest";
                var res = await client.GetAsync(url);
                res.EnsureSuccessStatusCode();

                using var stream = await res.Content.ReadAsStreamAsync();
                var release = await JsonSerializer.DeserializeAsync<GitHubRelease>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (release != null && release.Tag_name != null && release.Tag_name != CurrentVersion)
                {
                    isUpdateAvailable = true;
                    NotificationWindow notif = new NotificationWindow("Update Available", $"New version available: {release.Tag_name}\nCurrent version: {CurrentVersion}"
                        , MessageBoxImage.Information);
                    notif.ShowNotification();
                    await Task.Delay(3000).ContinueWith(_ =>
                    {
                        Application.Current.Dispatcher.Invoke(() => notif.CloseNotification());
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckForUpdateAsync: {ex.Message}");
            }
        }

        public class GitHubRelease
        {
            public string Tag_name { get; set; }
        }

        private void LoadApps()
        {
            string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string commonStart = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

            EnumerateShortcutsSafe(startMenu);
            EnumerateShortcutsSafe(commonStart);

            var unique = allApps
                .GroupBy(a => a.Path, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            allApps.Clear();
            allApps.AddRange(unique);
        }

        private void EnumerateShortcutsSafe(string root)
        {
            try
            {
                foreach (var shortcut in Directory.GetFiles(root, "*.lnk", SearchOption.AllDirectories))
                    TryAddShortcut(shortcut);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EnumerateShortcutsSafe for {root}: {ex.Message}");
            }
        }

        private void TryAddShortcut(string path)
        {
            try
            {
                var shell = new WshShell();
                var lnk = (IWshShortcut)shell.CreateShortcut(path);
                if (!string.IsNullOrWhiteSpace(lnk.TargetPath) && System.IO.File.Exists(lnk.TargetPath))
                {
                    allApps.Add(new AppModel
                    {
                        Name = System.IO.Path.GetFileNameWithoutExtension(path),
                        Path = lnk.TargetPath
                    });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TryAddShortcut for {path}: {ex.Message}");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == "") SearchBox.Background = new SolidColorBrush(Colors.Transparent);
            else SearchBox.Background = new SolidColorBrush(Colors.White);
            ApplyFilterAndReset();
            RenderCurrentPage();
        }

        private void ApplyFilterAndReset()
        {
            string q = SearchBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(q))
                filteredApps = allApps.ToList();
            else
                filteredApps = allApps
                    .Where(a => a.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            currentPage = 0;
            ClampPage();
            UpdatePageLabel();
        }

        private void RecalculatePagination()
        {
            if (Scroller == null) return;

            bool viewportReady = Scroller.ViewportHeight > 1 && Scroller.ViewportWidth > 1;
            if (!viewportReady)
            {
                if (!recalcScheduled)
                {
                    recalcScheduled = true;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        recalcScheduled = false;
                        RecalculatePagination();
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                return;
            }

            int newItemsPerPage;
            double availableW = Math.Max(0, Scroller.ViewportWidth);
            if (availableW <= 0) availableW = Scroller.ViewportWidth;
            double availableH = Math.Max(0, Scroller.ViewportHeight);
            if (availableH <= 0) availableH = Scroller.ActualHeight - 2;
            double tileTotalW = TILE_W + 2 * TILE_MARGIN;
            double tileTotalH = TILE_H + 2 * TILE_MARGIN;

            int cols = Math.Max(1, (int)(availableW / tileTotalW));
            int rows = Math.Max(1, (int)(availableH / tileTotalH));

            newItemsPerPage = Math.Max(1, cols * rows);

            if (newItemsPerPage != itemsPerPage)
            {
                itemsPerPage = newItemsPerPage;
                ClampPage();
                UpdatePageLabel();
                RenderCurrentPage();
            }
        }

        private void ClampPage()
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)filteredApps.Count / itemsPerPage));
            if (currentPage >= totalPages) currentPage = totalPages - 1;
            if (currentPage < 0) currentPage = 0;

            PrevBtn.IsEnabled = currentPage > 0;
            NextBtn.IsEnabled = currentPage < totalPages - 1;
            PrevBtn.Visibility = currentPage > 0 ? Visibility.Visible : Visibility.Hidden;
            NextBtn.Visibility = currentPage < totalPages - 1 ? Visibility.Visible : Visibility.Hidden;
            UpdatePageLabel();
        }

        private void UpdatePageLabel()
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)filteredApps.Count / Math.Max(1, itemsPerPage)));
            PageLabel.Text = $"{currentPage + 1} / {totalPages}";
        }

        private void RenderCurrentPage()
        {
            AppsPanel.Children.Clear();

            if (filteredApps.Count == 0)
            {
                var noAppText = new TextBlock
                {
                    Text = "No program found.",
                    Foreground = new SolidColorBrush(Color.FromRgb(69, 74, 53)),
                    Margin = new Thickness(8),
                    FontSize = 16
                };
                AppsPanel.Children.Add(noAppText);
                UpdatePageLabel();
                return;
            }
            ClampPage();
            int start = currentPage * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, filteredApps.Count);
            AppsPanel.Visibility = Visibility.Visible;
            for (int i = start; i < end; i++)
            {
                var app = filteredApps[i];
                var tile = CreateTile(app);
                tile.Opacity = 0;
                tile.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                tile.RenderTransform = new TranslateTransform { X = 40 };
                AppsPanel.Children.Add(tile);
                var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
                {
                    BeginTime = TimeSpan.FromMilliseconds(8 * (i - start))
                };

                var slide = new DoubleAnimation(40, 0, TimeSpan.FromMilliseconds(200))
                {
                    BeginTime = TimeSpan.FromMilliseconds(8 * (i - start)),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                tile.BeginAnimation(OpacityProperty, fade);
                (tile.RenderTransform as TranslateTransform)?.BeginAnimation(TranslateTransform.XProperty, slide);
            }
        }

        private FrameworkElement CreateTile(AppModel app)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };

            var img = new Image
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(0, 0, 0, 8),
                Opacity = 0.9
            };

            if (iconCache.TryGetValue(app.Path, out var cached))
            {
                img.Source = cached;
            }
            else
            {
                img.Source = null;
                _ = Task.Run(() =>
                {
                    var icon = TryLoadIcon(app.Path);
                    if (icon != null)
                    {
                        iconCache[app.Path] = icon;
                        Dispatcher.Invoke(() => img.Source = icon,DispatcherPriority.Background);
                    }
                });
            }

            var txt = new TextBlock
            {
                Name = "appsName",
                Text = app.Name,
                Foreground = new SolidColorBrush(Color.FromRgb(69, 74, 53)),
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = TILE_W - 16
            };
            appsName = txt;
            panel.Children.Add(img);
            panel.Children.Add(txt);

            var btn = new Button
            {
                Content = panel,
                Width = TILE_W,
                Height = TILE_H,
                Margin = new Thickness(TILE_MARGIN),
                Style = FindResource("ModernTileButton") as Style,
                Tag = app
            };

            btn.Click += (s, e) => SelectButton(btn);
            btn.MouseDoubleClick += LaunchApp;

            return btn;
        }

        private FrameworkElement CreateListItem(AppModel app)
        {
            var grid = new Grid
            {
                Height = 40,
                Margin = new Thickness(5)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var img = new Image
            {
                Width = 32,
                Height = 32,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 10, 0)
            };

            if (iconCache.TryGetValue(app.Path, out var cached))
            {
                img.Source = cached;
            }
            else
            {
                img.Source = null;
                _ = Task.Run(() =>
                {
                    var icon = TryLoadIcon(app.Path);
                    if (icon != null)
                    {
                        iconCache[app.Path] = icon;
                        Dispatcher.Invoke(() => img.Source = icon, System.Windows.Threading.DispatcherPriority.Background);
                    }
                });
            }

            Grid.SetColumn(img, 0);

            var txt = new TextBlock
            {
                Text = app.Name,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };

            Grid.SetColumn(txt, 1);

            grid.Children.Add(img);
            grid.Children.Add(txt);

            var btn = new Button
            {
                Content = grid,
                Style = FindResource("ClassicListItemButton") as Style,
                Tag = app
            };

            btn.Click += (s, e) => SelectButton(btn);
            btn.MouseDoubleClick += LaunchApp;

            return btn;
        }

        private void SelectButton(Button btn)
        {
            if (selectedButton != null)
            {
                selectedButton.Style = FindResource("ModernTileButton") as Style;
            }

            selectedButton = btn;
            btn.Style = FindResource("ModernTileButtonSelected") as Style;
        }

        private void LaunchApp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AppModel app)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(app.Path) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error executing {app.Name}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private ImageSource TryLoadIcon(string exePath)
        {
            try
            {
                using Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (icon == null) return null;

                var src = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(48, 48));

                src.Freeze();
                return src;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TryLoadIcon for {exePath}: {ex.Message}");
                return null;
            }
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                RenderCurrentPage();
                UpdatePageLabel();
            }
            ClampPage();
            SearchBox.Focus();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)filteredApps.Count / itemsPerPage));
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                RenderCurrentPage();
                UpdatePageLabel();
            }
            ClampPage();
            SearchBox.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                NextBtn_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                PrevBtn_Click(null, null);
                e.Handled = true;
            }
        }
        private void ApplyTheme()
        {
            SearchBox.Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.White, BlurRadius = 5, ShadowDepth = 0 };
            SearchBox.Background = new SolidColorBrush(Colors.Transparent);
            SearchBox.Foreground = new SolidColorBrush(Color.FromRgb(69, 74, 53));
            SearchBox.BorderThickness = new Thickness(0);
            this.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            PrevBtn.Style = FindResource("AnimatedNavButton") as Style;
            NextBtn.Style = FindResource("AnimatedNavButton") as Style;
            brSearchBox.Style = FindResource("BorderSearchBox") as Style;
            Scroller.CanContentScroll = false;

            ParticleCanvas.Background = new SolidColorBrush(Color.FromRgb(233, 233, 233));
            RecalculatePagination();
        }

        private void BtnAboutUs_Click(object sender, RoutedEventArgs e)
        {
            AboutUsWindow aboutUsWindow = new AboutUsWindow() { Owner =  this};
            aboutUsWindow.tbStatusUpdate.Visibility = isUpdateAvailable ? Visibility.Visible : Visibility.Hidden;
            aboutUsWindow.Show();
            Show();
        }
    }

    public class AppModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ImageSource Icon { get; set; }
    }

}
