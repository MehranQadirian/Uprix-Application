using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using System.ComponentModel;
using AppLauncher.Views.Pages;
using System.Windows.Navigation;
using AppLauncher.Classes;
using AppLauncher.Classes.MainClasses;
using AppLauncher.Views.Controls;

using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using AppLauncher.Classes.Core_Classes;
using System.Windows.Media.Effects;
using MessageBox = AppLauncher.Classes.MessageBox;

namespace AppLauncher
{
    public class ThemeColorsMainWindow
    {
        public string CaretBrushTextBoxes { get; set; }
    }

    public partial class MainWindow : Window
    {
        #region Variables
        private bool isMenuExpanded = false;
        public string CurrentVersion = "v3.0.0.0";
        private Button currentActiveButton = null;
        public const double TILE_W = 160;
        public const double TILE_H = 160;
        public const double TILE_MARGIN = 10;
        private bool recalcScheduled = false;

        private readonly List<AppModel> allApps = new List<AppModel>();
        private List<AppModel> filteredApps = new List<AppModel>();
        public readonly Dictionary<string, ImageSource> iconCache = new Dictionary<string, ImageSource>();
        private DispatcherTimer _clockTimer;
        TextBlock appsName;
        TextBlock ratetxt;
        Image appimg;
        Border activeIndicator;
        private int currentPage = 0;
        private int itemsPerPage = 12;
        public bool isUpdateAvailable = false;
        private readonly Random rand = new Random();
        private Button selectedButton;

        private bool isFavorite = false;
        private ApplyThemes apply = new ApplyThemes();
        private SolidColorBrush forePrim;
        private SolidColorBrush foreSec;
        private SearchEngine searchEngine = new SearchEngine();
        private UpdaterService updaterService;

        // New: Track last visited page
        private string lastViewPage = "Launcher";
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            updaterService = new UpdaterService(CurrentVersion, this);

            HamburgerButton.Click += HamburgerButton_Click;
            ViewButton.Click += ViewButton_Click;
            ExploreButton.Click += ExploreButton_Click;
            SubscriptionButton.Click += SubscriptionButton_Click;
            SettingsButton.Click += SettingsButton_Click;
            AboutUsButton.Click += AboutUsButton_Click;

            Loaded += MainWindow_Loaded;
            SearchBox.Focus();
            this.StateChanged += Window_StateChanged;
            this.Closing += Window_Closing;
            this.PreviewMouseDown += Window_PreviewMouseDown;

            _resizeTimer = new DispatcherTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(200);
            _resizeTimer.Tick += ResizeTimer_Tick;

            // Load last visited page from settings
            lastViewPage = Properties.Settings.Default.LastViewPage ?? "Launcher";
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            Properties.Settings.Default.lastWidth = Width;
            Properties.Settings.Default.lastHeight = Height;
            Properties.Settings.Default.Save();
            if (LauncherBaseGrid.IsEnabled && LauncherBaseGrid.Visibility == Visibility.Visible)
            {
                RefreshAppGrid();
            }
        }

        private SolidColorBrush ToBrush(string color) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

        private void SetFillAndStroke(System.Windows.Shapes.Shape shape, string color)
        {
            var brush = ToBrush(color);
            shape.Fill = brush;
            shape.Stroke = brush;
        }

        private void ApplyCustomTheme(ThemeColorsMainWindow theme)
        {
            SetBrushColor("CaretBrushTextBoxes", theme.CaretBrushTextBoxes);
        }

        private void SetBrushColor(string resourceKey, string colorHex)
        {
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(colorHex);

                if (this.Resources.Contains(resourceKey))
                {
                    var obj = this.Resources[resourceKey];
                    if (obj is SolidColorBrush scb && !scb.IsFrozen)
                    {
                        scb.Color = color;
                    }
                    else
                    {
                        this.Resources[resourceKey] = new SolidColorBrush(color);
                    }
                }
                else
                {
                    this.Resources.Add(resourceKey, new SolidColorBrush(color));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting color for {resourceKey}: {ex.Message}");
            }
        }

        public void SetTheme()
        {
            Console.WriteLine(DateTime.Now.Date);
            apply.ApplyThm();

            // MenuIconColor
            SetFillAndStroke(viewSVG, apply.MenuIconColor);
            SetFillAndStroke(exploreSVG, apply.MenuIconColor);
            SetFillAndStroke(subscriptionSVG, apply.MenuIconColor);
            SetFillAndStroke(aboutSVG, apply.MenuIconColor);
            SetFillAndStroke(hapburSVG, apply.MenuIconColor);
            SetFillAndStroke(settingsSVG, apply.MenuIconColor);

            copyrightSVG.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ClearSearchBoxBtnIcon.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            SearchIcon.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            Application.Current.Resources["ActiveIndicatorBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));

            Application.Current.Resources["AnimationNavButtonForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            NavigationPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            Copyright.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            SearchBox.Foreground = Copyright.Foreground;
            AboutUsText.Foreground = Copyright.Foreground;
            SettingsText.Foreground = Copyright.Foreground;
            ViewText.Foreground = Copyright.Foreground;
            ExploreText.Foreground = Copyright.Foreground;
            SubscriptionText.Foreground = Copyright.Foreground;
            SearchPlaceHolder.Foreground = Copyright.Foreground;
            AppName.Foreground = Copyright.Foreground;
            foreSec = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));

            // BackgroundPrimary
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            brLauncher.Background = Background;
            ParticleCanvas.Background = Background;
            brPageNum.Background = Background;
            var app = Application.Current;
            app.Resources["ComboBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            app.Resources["ComboBoxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundLightPart));
            app.Resources["ComboBoxText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundPrimary));
            app.Resources["ComboBoxHoverBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSelectedTile));
            app.Resources["ComboBoxFocusBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundPrimary));
            app.Resources["ComboBoxDisabledBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            app.Resources["ComboBoxDisabledText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundStatus));
            app.Resources["ComboBoxArrow"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundSecundary));
            app.Resources["ComboBoxDropdownBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            app.Resources["ComboBoxItemHoverBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundLightPart));
            app.Resources["ComboBoxItemSelectedBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSelectedTile));
            app.Resources["CaretBrushTextBoxes"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundPrimary));

            // BackgroundSecundary
            NavigationPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brHamburgerBtn.Background = NavigationPanel.Background;
            brSearchBox.Background = NavigationPanel.Background;
            prvPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            nxtPath.Fill = prvPath.Fill;

            // BackgroundSelectedTile
            Application.Current.Resources["SelectedTileApps"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSelectedTile));
            brSearchBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundOnSecundary));
            SearchBox.CaretBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));

            RefreshAppGrid();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.isMaximaied = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.isMaximaied = false;
                Properties.Settings.Default.Save();
            }
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                RefreshAppGrid();
            }), DispatcherPriority.Loaded);
        }

        public void RestartApp()
        {
            Process currentProcess = Process.GetCurrentProcess();
            _ = Process.Start(currentProcess.MainModule.FileName);
            Application.Current.Shutdown();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SetTheme();
            ApplyCustomTheme(new ThemeColorsMainWindow
            {
                CaretBrushTextBoxes = apply.MenuIconColor
            });
            Width = Properties.Settings.Default.lastWidth;
            Height = Properties.Settings.Default.lastHeight;
            WindowState = Properties.Settings.Default.isMaximaied ? WindowState.Maximized : WindowState.Normal;

            var lazyLoadingOverlay = new LazyLoadingOverlay(
                apply,
                backgroundColor: apply.BackgroundPrimary,
                skeletonColor: apply.BackgroundSecundary,
                shimmerColor: apply.BackgroundSelectedTile
            )
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            Grid mainGrid = (Grid)Content;
            Grid.SetRowSpan(lazyLoadingOverlay, 10);
            Grid.SetColumnSpan(lazyLoadingOverlay, 10);
            Panel.SetZIndex(lazyLoadingOverlay, 9999);
            mainGrid.Children.Add(lazyLoadingOverlay);

            lazyLoadingOverlay.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            lazyLoadingOverlay.BeginAnimation(OpacityProperty, fadeIn);

            var progress = new Progress<string>(status =>
            {
                lazyLoadingOverlay.UpdateStatus(status);
            });

            AppLoader appLoader = new AppLoader(allApps);
            await appLoader.LoadAsync(progress);

            await Task.Delay(300);

            await lazyLoadingOverlay.HideWithAnimation();
            mainGrid.Children.Remove(lazyLoadingOverlay);

            ApplyFilterAndReset();
            RecalculatePagination();
            RenderCurrentPage();

            // Navigate to last visited page
            ViewButton_Click(null, null);

            await updaterService.CheckForUpdateAsync();
            await updaterService.SendMessageAsync($"{Environment.UserName} Running and using {CurrentVersion}", "", "low");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save last visited page
            Properties.Settings.Default.LastViewPage = lastViewPage;
            Properties.Settings.Default.Save();

            DatabaseManager.CloseAll();
            Application.Current.Shutdown();
        }

        private DispatcherTimer _resizeTimer;

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceHolder.Foreground = string.IsNullOrWhiteSpace(SearchBox.Text)
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor))
                : new SolidColorBrush(Colors.Transparent);
            SearchBox.Foreground = !string.IsNullOrWhiteSpace(SearchBox.Text)
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor))
                : new SolidColorBrush(Colors.Transparent);
            ClearSearchBoxButton.Visibility = !string.IsNullOrWhiteSpace(SearchBox.Text)
                ? Visibility.Visible :
                Visibility.Hidden;
            ApplyFilterAndReset();
            RenderCurrentPage();
        }

        private void ApplyFilterAndReset()
        {
            string q = SearchBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(q))
            {
                filteredApps = allApps
                    .OrderByDescending(a => a.Rate)
                    .ThenBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
            }
            else
            {
                filteredApps = allApps
                    .Select(a => new { App = a, Score = searchEngine.ComputeScore(q, a.Name) })
                    .Where(x => x.Score > 0.3)
                    .OrderByDescending(x => x.App.Rate)
                    .ThenByDescending(x => x.Score)
                    .Select(x => x.App)
                    .ToList();
            }

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

        public void RenderCurrentPage()
        {
            AppsPanel.Children.Clear();

            if (filteredApps.Count == 0)
            {
                var noAppText = new TextBlock
                {
                    Text = "No program found.",
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Margin = new Thickness(8),
                    FontSize = 16,
                    Opacity = 0
                };
                AppsPanel.Children.Add(noAppText);

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                noAppText.BeginAnimation(UIElement.OpacityProperty, fadeIn);

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
                var translate = new TranslateTransform { Y = 12 };
                tile.RenderTransform = translate;

                AppsPanel.Children.Add(tile);

                var delay = TimeSpan.FromMilliseconds(10 * (i - start));

                var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(30))
                {
                    BeginTime = delay,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var slide = new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(50))
                {
                    BeginTime = delay,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                tile.BeginAnimation(OpacityProperty, fade);
                translate.BeginAnimation(TranslateTransform.YProperty, slide);
            }
        }

        private string FormatRate(long rate)
        {
            if (rate < 1000) return $"{rate}";
            if (rate < 1_000_000) return $"{rate / 1000}K";
            if (rate < 1_000_000_000) return $"{rate / 1_000_000}M";
            if (rate < 1_000_000_000_000) return $"{rate / 1_000_000_000}B";
            return $"{rate}";
        }

        private void RefreshAppGrid()
        {
            RecalculatePagination();
            ClampPage();
            RenderCurrentPage();
        }

        private FrameworkElement CreateTile(AppModel app)
        {
            var gridpanel = new Grid
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };
            var img = new Image
            {
                Name = "AppIMG",
                Width = 50,
                Height = 50,
                Margin = new Thickness(0, -30, 0, 8),
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
                    if (!string.IsNullOrEmpty(app.Path) && icon != null)
                    {
                        try
                        {
                            Dispatcher.Invoke(() => img.Source = icon, DispatcherPriority.Background);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error for load pictures apps : " + ex.Message);
                        }
                    }
                });
            }

            var AppsName = new TextBlock
            {
                Name = "appsName",
                Text = app.Name,
                Foreground = foreSec,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = TILE_W - 16
            };

            string rateDisplay = "";
            if (app.Rate > 0)
            {
                string formattedRate = FormatRate(app.Rate);
                rateDisplay = app.Rate > 1 ? $"Used : {formattedRate} times" : $"Used : {formattedRate} time";
            }

            var rateText = new TextBlock
            {
                Name = "ratetxt",
                Text = rateDisplay,
                Foreground = foreSec,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = TILE_W - 16
            };

            ratetxt = rateText;
            appsName = AppsName;
            appimg = img;

            string starFilled = "M 24.0098 5 A 1.50015 1.50015 0 0 0 22.6582 5.83008 L 17.5059 16.1348 L 5.27148 18.0176 A 1.50015 1.50015 0 0 0 4.43945 20.5605 L 12.9023 29.0234 L 11.0176 41.2715 A 1.50015 1.50015 0 0 0 13.1934 42.8301 L 24 37.1914 L 34.8066 42.8301 A 1.50015 1.50015 0 0 0 36.9824 41.2715 L 35.0977 29.0234 L 43.5605 20.5605 A 1.50015 1.50015 0 0 0 42.7285 18.0176 L 30.4941 16.1348 L 25.3418 5.83008 A 1.50015 1.50015 0 0 0 24.0098 5 Z";
            string starOutline = "M 24.0098 5 A 1.50015 1.50015 0 0 0 22.6582 5.83008 L 17.5059 16.1348 L 5.27148 18.0176 A 1.50015 1.50015 0 0 0 4.43945 20.5605 L 12.9023 29.0234 L 11.0176 41.2715 A 1.50015 1.50015 0 0 0 13.1934 42.8301 L 24 37.1914 L 34.8066 42.8301 A 1.50015 1.50015 0 0 0 36.9824 41.2715 L 35.0977 29.0234 L 43.5605 20.5605 A 1.50015 1.50015 0 0 0 42.7285 18.0176 L 30.4941 16.1348 L 25.3418 5.83008 A 1.50015 1.50015 0 0 0 24.0098 5 Z M 24 9.85352 L 28.1582 18.1699 A 1.50015 1.50015 0 0 0 29.2715 18.9824 L 39.3457 20.5332 L 32.4395 27.4395 A 1.50015 1.50015 0 0 0 32.0176 28.7285 L 33.5664 38.7988 L 24.6934 34.1699 A 1.50015 1.50015 0 0 0 23.3066 34.1699 L 14.4336 38.7988 L 15.9824 28.7285 A 1.50015 1.50015 0 0 0 15.5605 27.4395 L 8.6543 20.5332 L 18.7285 18.9824 A 1.50015 1.50015 0 0 0 19.8418 18.1699 L 24 9.85352 Z";

            var favIcon = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(app.Favorite ? starFilled : starOutline),
                Fill = app.Favorite ? Brushes.Gold : Brushes.White,
                StrokeThickness = 1,
                Stroke = app.Favorite ? Brushes.Gold : Brushes.White,
                Width = 20,
                Height = 20,
                Stretch = Stretch.Uniform,
                Style = app.Favorite ? FindResource("StarActivePathStyle") as Style : FindResource("StarInactivePathStyle") as Style
            };

            var favBtn = new Button
            {
                Content = favIcon,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                Margin = new Thickness(100, 0, 0, 30),
                BorderThickness = new Thickness(0),
                Tag = app,
                Style = FindResource("AnimatedNavButton") as Style
            };

            favBtn.Click += (s, e) =>
            {
                app.Favorite = !app.Favorite;
                favIcon.Data = Geometry.Parse(app.Favorite ? starFilled : starOutline);
                favIcon.Fill = app.Favorite ? Brushes.Gold : Brushes.White;
                favIcon.Stroke = app.Favorite ? Brushes.Gold : Brushes.White;
                favIcon.Style = app.Favorite ? FindResource("StarActivePathStyle") as Style : FindResource("StarInactivePathStyle") as Style;

                DatabaseHelper.UpdateFavorite(app);

                if (isFavorite)
                    ShowFavorites();
            };

            panel.Children.Add(favBtn);
            panel.Children.Add(img);
            panel.Children.Add(AppsName);
            panel.Children.Add(rateText);

            var btn = new Button
            {
                Content = panel,
                Width = TILE_W,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary)),
                Height = TILE_H,
                Margin = new Thickness(TILE_MARGIN),
                Style = FindResource("ModernTileButton") as Style,
                Tag = app,
                ToolTip = $"{app.Name}\nPath⇒[{app.Path}]\nUsed: {app.Rate} times"
            };

            btn.Click += (s, e) => SelectButton(btn);
            btn.MouseDoubleClick += LaunchApp;

            return btn;
        }

        private void ShowFavorites()
        {
            filteredApps = allApps
                .Where(a => a.Favorite)
                .OrderByDescending(a => a.Rate)
                .ThenBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            currentPage = 0;
            RenderCurrentPage();
        }

        private void ShowAllApps()
        {
            ApplyFilterAndReset();
            currentPage = 0;
            RenderCurrentPage();
        }

        private void SetButtonGlowColor(Button btn, Color color)
        {
            if (btn.Template != null)
            {
                var border = btn.Template.FindName("border", btn) as Border;
                if (border != null && border.Effect is DropShadowEffect glow)
                {
                    glow.Color = color;
                }
            }
        }

        private void SelectButton(Button btn)
        {
            if (selectedButton != null)
            {
                selectedButton.Style = FindResource("ModernTileButton") as Style;
            }

            selectedButton = btn;
            btn.Style = FindResource("ModernTileButtonSelected") as Style;
            btn.ApplyTemplate();
            SetButtonGlowColor(btn, (Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
        }

        private void LaunchApp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AppModel app)
            {
                try
                {
                    Console.WriteLine($"🚀 Launching: {app.Name} (Current Rate: {app.Rate})");

                    Process.Start(new ProcessStartInfo(app.Path) { UseShellExecute = true });
                    app.Rate++;

                    Console.WriteLine($"📊 New Rate: {app.Rate}");

                    DatabaseHelper.UpdateRate(app);

                    allApps.Sort((a, b) =>
                    {
                        int rateCompare = b.Rate.CompareTo(a.Rate);
                        if (rateCompare != 0) return rateCompare;
                        return string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
                    });

                    Console.WriteLine($"✅ First 3 apps after sort: {string.Join(", ", allApps.Take(3).Select(a => $"{a.Name}({a.Rate})"))}");

                    DatabaseHelper.SaveAppsToDb(allApps);

                    ApplyFilterAndReset();
                    RenderCurrentPage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error executing {app.Name}: {ex.Message}", "Error", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
                }
            }
        }

        public ImageSource TryLoadIcon(string exePath)
        {
            try
            {
                using Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (icon == null) return null;

                var src = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(128, 128));

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
            if ((e.Key == Key.Right && Keyboard.Modifiers == ModifierKeys.Control && SearchBox.IsFocused)
                || (e.Key == Key.Right && Keyboard.Modifiers == ModifierKeys.Control && !SearchBox.IsFocused))
            {
                NextBtn_Click(null, null);
                e.Handled = true;
            }
            else if ((e.Key == Key.Left && Keyboard.Modifiers == ModifierKeys.Control && SearchBox.IsFocused)
                || (e.Key == Key.Left && Keyboard.Modifiers == ModifierKeys.Control && !SearchBox.IsFocused))
            {
                PrevBtn_Click(null, null);
                e.Handled = true;
            }
            else if ((e.Key == Key.Escape && SearchBox.IsFocused)
                || (e.Key == Key.Escape && !SearchBox.IsFocused))
            {
                ClearSearchBoxButton_Click(null, null);
                e.Handled = true;
            }
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                PrevBtn_Click(null, null);
            }
            else if (e.Delta < 0)
            {
                NextBtn_Click(null, null);
            }

            e.Handled = true;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenu();
        }

        private void ToggleMenu()
        {
            if (isMenuExpanded)
            {
                CollapseMenu();
            }
            else
            {
                ExpandMenu();
            }
        }

        private void ExpandMenu()
        {
            Storyboard expandStoryboard = (Storyboard)FindResource("ExpandMenu");
            expandStoryboard.Begin();

            Storyboard expandFont = (Storyboard)FindResource("ExpandFont");
            expandFont.Begin();

            isMenuExpanded = true;
        }

        private void CollapseMenu()
        {
            Storyboard collapseStoryboard = (Storyboard)FindResource("CollapseMenu");
            collapseStoryboard.Begin();

            Storyboard collapseFont = (Storyboard)FindResource("CollapseFont");
            collapseFont.Begin();

            isMenuExpanded = false;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Check if menu is expanded and click is outside the navigation panel
            if (isMenuExpanded)
            {
                // Get the position of the click
                System.Windows.Point clickPosition = e.GetPosition(this);

                // Get the bounds of the navigation panel
                System.Windows.Point navPanelPosition = NavigationPanel.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                Rect navPanelBounds = new Rect(
                    navPanelPosition.X,
                    navPanelPosition.Y,
                    NavigationPanel.ActualWidth,
                    NavigationPanel.ActualHeight
                );

                // If click is outside the navigation panel, collapse the menu
                if (!navPanelBounds.Contains(clickPosition))
                {
                    CollapseMenu();
                }
            }
        }

        private void SetActiveButton(Button button)
        {
            if (currentActiveButton != null && currentActiveButton != button)
            {
                currentActiveButton.Tag = "Inactive";
            }

            button.Tag = "Active";
            currentActiveButton = button;
        }

        // New Navigation Methods
        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            // Close menu if expanded
            if (isMenuExpanded)
            {
                CollapseMenu();
            }

            SetActiveButton(ViewButton);
            NavigateToView(lastViewPage);
        }

        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            // Close menu if expanded
            if (isMenuExpanded)
            {
                CollapseMenu();
            }

            SetActiveButton(ExploreButton);
            var explorePage = new ExplorePage(this, apply) { KeepAlive = false };
            MainContentControl.Navigate(explorePage);
            SetVisibleMainContentControl();
            LauncherBaseGrid.Visibility = Visibility.Hidden;
        }

        private void SubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            // Close menu if expanded
            if (isMenuExpanded)
            {
                CollapseMenu();
            }

            SetActiveButton(SubscriptionButton);
            var subscriptionPage = new SubscriptionPage(apply) { KeepAlive = false };
            MainContentControl.Navigate(subscriptionPage);
            SetVisibleMainContentControl();
            LauncherBaseGrid.Visibility = Visibility.Hidden;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Close menu if expanded
            if (isMenuExpanded)
            {
                CollapseMenu();
            }

            SetActiveButton(SettingsButton);
            var settingsPage = new SettingsPage(this, allApps, filteredApps, currentPage, apply) { KeepAlive = false };
            MainContentControl.Navigate(settingsPage);
            SetVisibleMainContentControl();
            LauncherBaseGrid.Visibility = Visibility.Hidden;
        }

        private void AboutUsButton_Click(object sender, RoutedEventArgs e)
        {
            // Close menu if expanded
            if (isMenuExpanded)
            {
                CollapseMenu();
            }

            SetActiveButton(AboutUsButton);
            var aboutUsPage = new AboutUsPage(this, apply) { KeepAlive = false };
            //aboutUsPage.tbStatusUpdate.Visibility = isUpdateAvailable ? Visibility.Visible : Visibility.Hidden;
            aboutUsPage.UpdateBadge.Visibility = isUpdateAvailable ? Visibility.Visible : Visibility.Hidden;
            MainContentControl.Navigate(aboutUsPage);
            SetVisibleMainContentControl();
            LauncherBaseGrid.Visibility = Visibility.Hidden;
        }

        // Public method to navigate to View with specific page
        public void NavigateToView(string pageName)
        {
            lastViewPage = pageName;
            SetActiveButton(ViewButton);

            switch (pageName)
            {
                case "Launcher":
                    ShowAllApps();
                    isFavorite = false;
                    LauncherBaseGrid.Visibility = Visibility.Visible;
                    SetHiddenMainContentControl();
                    break;

                case "Favorites":
                    ShowFavorites();
                    isFavorite = true;
                    LauncherBaseGrid.Visibility = Visibility.Visible;
                    SetHiddenMainContentControl();
                    break;

                case "Bookmarks":
                    var bookmarkPage = new BookmarkPage(apply) { KeepAlive = false };
                    MainContentControl.Navigate(bookmarkPage);
                    SetVisibleMainContentControl();
                    LauncherBaseGrid.Visibility = Visibility.Hidden;
                    break;

                case "TaskManager":
                    var taskPage = new TaskManagerPage(apply) { KeepAlive = false };
                    MainContentControl.Navigate(taskPage);
                    SetVisibleMainContentControl();
                    LauncherBaseGrid.Visibility = Visibility.Hidden;
                    break;

                default:
                    ShowAllApps();
                    isFavorite = false;
                    LauncherBaseGrid.Visibility = Visibility.Visible;
                    SetHiddenMainContentControl();
                    break;
            }
        }

        private void SetVisibleMainContentControl()
        {
            MainContentControl.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MainContentControl.Background = Brushes.Transparent;
            MainContentControl.Visibility = Visibility.Visible;
        }

        private void SetHiddenMainContentControl()
        {
            MainContentControl.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MainContentControl.Background = Brushes.Transparent;
            MainContentControl.Visibility = Visibility.Hidden;
        }

        public void NavigateToSettingsPage()
        {
            SetActiveButton(SettingsButton);
            var settingsPage = new SettingsPage(this, allApps, filteredApps, currentPage, apply) { KeepAlive = false };
            MainContentControl.Navigate(settingsPage);
            SetVisibleMainContentControl();
            LauncherBaseGrid.Visibility = Visibility.Hidden;
        }

        private void ClearSearchBoxButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
        }

        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Shapes.Path p)
                p.Opacity = 0.3;
        }

        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Shapes.Path p)
                p.Opacity = 1;
        }
    }
}