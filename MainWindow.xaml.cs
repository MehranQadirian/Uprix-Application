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
using System.Windows.Shapes;
using IWshRuntimeLibrary;             
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Color = System.Windows.Media.Color;

namespace AppLauncher
{
    public partial class MainWindow : Window
    {
        private const double TILE_W = 160;
        private const double TILE_H = 160;
        private const double TILE_MARGIN = 10;

        private readonly List<AppModel> allApps = new List<AppModel>();
        private List<AppModel> filteredApps = new List<AppModel>();
        private readonly Dictionary<string, ImageSource> iconCache = new Dictionary<string, ImageSource>();

        private int currentPage = 0;
        private int itemsPerPage = 12;  

        private readonly Random rand = new Random();
        private Button selectedButton;

        public MainWindow()
        {
            InitializeComponent();
            SearchBox.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateParticles(56);              
            LoadApps();                        
            ApplyFilterAndReset();            
            RecalculatePagination();           
            RenderCurrentPage();               
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecalculatePagination();
            ClampPage();
            RenderCurrentPage();
        }

        // ---------- Load Apps ----------
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
            catch
            {
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
            catch
            {
            }
        }

        // ---------- Filtering & Pagination ----------
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
            double availableW = Math.Max(0, AppsPanel.ActualWidth);
            if (availableW <= 0) availableW = Scroller.ActualWidth;
            double availableH = Math.Max(0, Scroller.ViewportHeight);
            if (availableH <= 0) availableH = Scroller.ActualHeight - 2;
            double tileTotalW = TILE_W + 2 * TILE_MARGIN;
            double tileTotalH = TILE_H + 2 * TILE_MARGIN;

            int cols = Math.Max(1, (int)(availableW / tileTotalW));
            int rows = Math.Max(1, (int)(availableH / tileTotalH));

            itemsPerPage = Math.Max(1, cols * rows);
            UpdatePageLabel();
        }

        private void ClampPage()
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)filteredApps.Count / itemsPerPage));
            if (currentPage >= totalPages) currentPage = totalPages - 1;
            if (currentPage < 0) currentPage = 0;

            PrevBtn.IsEnabled = currentPage > 0;
            NextBtn.IsEnabled = currentPage < totalPages - 1;
            // PrevBtn Visiblity
            if (!PrevBtn.IsEnabled) PrevBtn.Visibility = PrevBtn.Visibility = Visibility.Hidden;
            else PrevBtn.Visibility = PrevBtn.Visibility = Visibility.Visible;
            // NextBtn Visiblity
            if (!NextBtn.IsEnabled) NextBtn.Visibility = NextBtn.Visibility = Visibility.Hidden;
            else NextBtn.Visibility = NextBtn.Visibility = Visibility.Visible;
            if (currentPage > 0 || currentPage < totalPages - 1) UpdatePageLabel();
        }

        private void UpdatePageLabel()
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)filteredApps.Count / Math.Max(1, itemsPerPage)));
            if (currentPage + 1 > 0)
            {
                int pageNumber = currentPage + 1;
                PageLabel.Text = $"Page {pageNumber} / {totalPages}";
            }
        }

        // ---------- Render ----------
        private void RenderCurrentPage()
        {
            AppsPanel.Children.Clear();

            if (filteredApps.Count == 0)
            {
                var noAppText = new TextBlock
                {
                    Text = "No program found.",
                    Foreground = Brushes.White,
                    Margin = new Thickness(8),
                    FontSize = 16,
                    Opacity = 0
                };

                AppsPanel.Children.Add(noAppText);

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                noAppText.BeginAnimation(OpacityProperty, fadeIn);

                UpdatePageLabel();
                return;
            }

            ClampPage();

            int start = currentPage * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, filteredApps.Count);

            for (int i = start; i < end; i++)
            {
                var app = filteredApps[i];
                var tile = CreateTile(app);

                tile.Opacity = 0;
                tile.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                tile.RenderTransform = new TranslateTransform { X = 40, Y = 0 };

                AppsPanel.Children.Add(tile);

                var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
                {
                    BeginTime = TimeSpan.FromMilliseconds(50 * (i - start)) 
                };

                var slide = new DoubleAnimation(40, 0, TimeSpan.FromMilliseconds(300))
                {
                    BeginTime = TimeSpan.FromMilliseconds(50 * (i - start)),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                tile.BeginAnimation(OpacityProperty, fade);
                (tile.RenderTransform as TranslateTransform)?.BeginAnimation(TranslateTransform.XProperty, slide);
            }
        }

        // ---------- Tile UI ----------
        private FrameworkElement CreateTile(AppModel app)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };

            var img = new Image
            {
                Width = 48,
                Height = 48,
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
                        Dispatcher.Invoke(() =>
                        {
                            img.Source = icon;
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                });
            }

            var txt = new TextBlock
            {
                Text = app.Name,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = TILE_W - 16
            };

            panel.Children.Add(img);
            panel.Children.Add(txt);

            var btn = new Button
            {
                Content = panel,
                Width = TILE_W,
                Height = TILE_H,
                Margin = new Thickness(TILE_MARGIN),
                Background = new SolidColorBrush(Color.FromRgb(46, 46, 62)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Tag = app,
                Opacity = 0
            };

            btn.Template = CreateCardStyle();

            btn.MouseEnter += (s, e) => AnimateScale(btn, 1.07);
            btn.MouseLeave += (s, e) => AnimateScale(btn, selectedButton == btn ? 1.03 : 1.0);

            btn.Click += (s, e) => SelectButton(btn);

            btn.MouseDoubleClick += (s, e) =>
            {
                try
                {
                    var notif = new NotificationWindow("Runing", $"{app.Name} is runing", MessageBoxImage.None) { Owner = this };
                    notif.ShowNotification();
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        Application.Current.Dispatcher.Invoke(() => notif.CloseNotification());
                        if (notif.IsNotificationVisible) Process.Start(new ProcessStartInfo(app.Path) { UseShellExecute = true });
                    });
                }
                catch (Exception ex)
                {
                    var notif = new NotificationWindow($"Error executing {app.Name} program", $"{ex.Message}", MessageBoxImage.Error) { Owner = this };
                    notif.ShowNotification();
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        Application.Current.Dispatcher.Invoke(() => notif.CloseNotification());
                    });
                }
            };

            return btn;
        }

        private ControlTemplate CreateCardStyle()
        {
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(14));
            border.SetValue(Border.SnapsToDevicePixelsProperty, true);
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.EffectProperty, new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 10,
                Opacity = 0.28,
                ShadowDepth = 2
            });

            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(content);
            template.VisualTree = border;

            return template;
        }

        private void SelectButton(Button btn)
        {
            foreach (var child in AppsPanel.Children.OfType<Button>())
            {
                if (child == btn) continue;
                child.Background = new SolidColorBrush(Color.FromRgb(46, 46, 62));
                AnimateScale(child, 1.0);
            }

            selectedButton = btn;

            var brush = new SolidColorBrush(Color.FromRgb(12, 0, 102));
            btn.Background = brush;

            var pulse = new ColorAnimation(Color.FromRgb(0, 19, 163),
                                           TimeSpan.FromMilliseconds(400))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            brush.BeginAnimation(SolidColorBrush.ColorProperty, pulse);

            AnimateScale(btn, 1.03);
        }

        private void AnimateScale(UIElement element, double scale)
        {
            if (!(element.RenderTransform is ScaleTransform t))
            {
                t = new ScaleTransform(1, 1);
                element.RenderTransform = t;
                element.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            }

            var ax = new DoubleAnimation(scale, TimeSpan.FromMilliseconds(120)) { EasingFunction = new QuadraticEase() };
            var ay = new DoubleAnimation(scale, TimeSpan.FromMilliseconds(120)) { EasingFunction = new QuadraticEase() };
            t.BeginAnimation(ScaleTransform.ScaleXProperty, ax);
            t.BeginAnimation(ScaleTransform.ScaleYProperty, ay);
        }

        // ---------- Async Icon ----------
        private ImageSource TryLoadIcon(string exePath)
        {
            try
            {
                using (Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath))
                {
                    if (icon == null) return null;

                    var src = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(48, 48));

                    src.Freeze();
                    return src;
                }
            }
            catch
            {
                return null;
            }
        }

        // ---------- Pager (Buttons & Keyboard) ----------
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

        // ---------- Particles (Lightweight) ----------
        private void CreateParticles(int count)
        {
            ParticleCanvas.Children.Clear();

            for (int i = 0; i < count; i++)
            {
                var x = rand.Next(3, 7);
                var dot = new Ellipse
                {
                    Width = x,
                    Height = x,
                    Fill = Brushes.White,
                    Opacity = 0.22
                };

                double startX = rand.Next((int)Math.Max(1, ParticleCanvas.ActualWidth));
                double startY = rand.Next((int)Math.Max(1, ParticleCanvas.ActualHeight));

                Canvas.SetLeft(dot, startX);
                Canvas.SetTop(dot, startY);

                ParticleCanvas.Children.Add(dot);

                var dur = TimeSpan.FromSeconds(rand.Next(9, 34));
                var anim = new DoubleAnimation(startY, -12, dur)
                {
                    RepeatBehavior = RepeatBehavior.Forever
                };
                dot.BeginAnimation(Canvas.TopProperty, anim);

                var fade = new DoubleAnimation(0.12, 0.28, TimeSpan.FromSeconds(rand.Next(6, 12)))
                {
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                dot.BeginAnimation(UIElement.OpacityProperty, fade);
            }
        }
        private void CheckUpdateNow()
        {
            var win = new UpdateWindow { Owner = this };
            win.ShowDialog();
        }

        private void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            CheckUpdateNow();
        }
        private void btnTelegram_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://t.me/UprixApplication",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open Telegram link:\n" + ex.Message);
            }
        }

        private void btnGithub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/MehranQadirian/Uprix-Application",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open GitHub link:\n" + ex.Message);
            }
        }

        private void btnTelephone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string machineName = Environment.MachineName;
                string userName = Environment.UserName;
                string osVersion = Environment.OSVersion.ToString();
                string dotnetVersion = Environment.Version.ToString();

                string subject = Uri.EscapeDataString("Uprix Application - Support Request");
                string body = Uri.EscapeDataString(
                    $"Hello Uprix Team" +
                    $"\nI need support regarding the Uprix Application" +
                    $"\nMachine Name: {machineName}" +
                    $"\nUser Name: {userName}" +
                    $"\nOS Version: {osVersion}" +
                    $"\n.NET Version: {dotnetVersion}" +
                    $"\n\n\n[Please describe your issue here... <Persian or English> ]:\n\n\t"
                );

                string gmailUrl = $"https://mail.google.com/mail/?view=cm&fs=1" +
                                  $"&to=mehranghadirian01@gmail.com" +
                                  $"&su={subject}" +
                                  $"&body={body}";

                Process.Start(new ProcessStartInfo
                {
                    FileName = gmailUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open Gmail:\n" + ex.Message);
            }
        }

    }

    // ---------- Model ----------
    public class AppModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ImageSource Icon { get; set; } 
    }
}
