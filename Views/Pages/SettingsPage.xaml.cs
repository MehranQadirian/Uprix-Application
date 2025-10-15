using AppLauncher.Classes;
using AppLauncher.Classes.Core_Classes;
using AppLauncher.Classes.MainClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace AppLauncher.Views.Pages
{
    public partial class SettingsPage : Page
    {
        private List<CustomBrowser> customBrowsers = new List<CustomBrowser>();
        // Add this nested class
        public class CustomBrowser
        {
            public string Name { get; set; }
            public string ExecutablePath { get; set; }
        }
        private List<AppModel> allApps { get; set; }
        private List<AppModel> filteredApps { get; set; }
        private int currentPage = 0;
        private string penndingTheme , lastTheme;
        private MainWindow main;
        private ApplyThemes apply;
        public bool isChecked { get; set; } 
        public SettingsPage(MainWindow win , List<AppModel> AApps , List<AppModel> FApps , int CPage ,ApplyThemes applying)
        {
            InitializeComponent();
            apply = applying;
            SetTheme();
            main = win;
            allApps = AApps;
            filteredApps = FApps;
            currentPage = CPage;
            lastTheme = Properties.Settings.Default.Theme;
            penndingTheme = Properties.Settings.Default.Theme;
            LoadCustomBrowsers();
        }

        private void SetTheme()
        {
            gridMain.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            brThemes.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            brMain.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            StatusSelectedTheme.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            var savedColor = Properties.Settings.Default.TextColor;
            SetTextTheme(savedColor);
        }
        private void SetTextTheme(Color color)
        {
            // رنگ جدید را در تنظیمات ذخیره کن
            Properties.Settings.Default.TextColor = color;
            Properties.Settings.Default.Save();

            // برای همه TextBlockها تنظیم کن
            txtTh.Foreground = new SolidColorBrush(color);
            txtTh1.Foreground = new SolidColorBrush(color);
            txtTh2.Foreground = new SolidColorBrush(color);
            txtTh3.Foreground = new SolidColorBrush(color);
            txtTh4.Foreground = new SolidColorBrush(color);
            txtTh5.Foreground = new SolidColorBrush(color);
            txtTh6.Foreground = new SolidColorBrush(color);
            txtTh7.Foreground = new SolidColorBrush(color);
            txtTh8.Foreground = new SolidColorBrush(color);
            txtTh9.Foreground = new SolidColorBrush(color);
        }
        private void LoadCustomBrowsers()
        {
            string browsersJson = Properties.Settings.Default.CustomBrowsers;
            if (!string.IsNullOrEmpty(browsersJson))
            {
                customBrowsers = JsonConvert.DeserializeObject<List<CustomBrowser>>(browsersJson)
                                ?? new List<CustomBrowser>();
            }
            RenderCustomBrowsers();
        }
        private void RenderCustomBrowsers()
        {
            CustomBrowsersPanel.Children.Clear();

            foreach (var browser in customBrowsers)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0)),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(0, 5, 0, 5),
                    Padding = new Thickness(10)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var nameBlock = new TextBlock
                {
                    Text = browser.Name,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14
                };
                Grid.SetColumn(nameBlock, 0);

                var pathBlock = new TextBlock
                {
                    Text = browser.ExecutablePath,
                    Foreground = Brushes.LightGray,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(pathBlock, 1);

                var deleteBtn = new Button
                {
                    Content = "Remove",
                    Style = (Style)FindResource("AnimatedNavButton"),
                    Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    Foreground = Brushes.White,
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = browser
                };
                deleteBtn.Click += RemoveCustomBrowser_Click;
                Grid.SetColumn(deleteBtn, 2);

                grid.Children.Add(nameBlock);
                grid.Children.Add(pathBlock);
                grid.Children.Add(deleteBtn);
                border.Child = grid;
                CustomBrowsersPanel.Children.Add(border);
            }
        }
        private void AddCustomBrowser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe",
                Title = "Select Browser Executable"
            };

            if (dialog.ShowDialog() == true)
            {
                string browserName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);

                var inputDialog = new Window
                {
                    Title = "Browser Name",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this)
                };

                var stack = new StackPanel { Margin = new Thickness(15) };
                stack.Children.Add(new TextBlock { Text = "Enter browser name:", Margin = new Thickness(0, 0, 0, 10) });

                var textBox = new System.Windows.Controls.TextBox { Text = browserName, Margin = new Thickness(0, 0, 0, 10) };
                stack.Children.Add(textBox);

                var okBtn = new System.Windows.Controls.Button { Content = "OK", Width = 80 };
                okBtn.Click += (s, args) => { inputDialog.DialogResult = true; inputDialog.Close(); };
                stack.Children.Add(okBtn);

                inputDialog.Content = stack;

                if (inputDialog.ShowDialog() == true)
                {
                    customBrowsers.Add(new CustomBrowser
                    {
                        Name = textBox.Text,
                        ExecutablePath = dialog.FileName
                    });

                    SaveCustomBrowsers();
                    RenderCustomBrowsers();
                }
            }
        }

        private void RemoveCustomBrowser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is CustomBrowser browser)
            {
                customBrowsers.Remove(browser);
                SaveCustomBrowsers();
                RenderCustomBrowsers();
            }
        }

        private void SaveCustomBrowsers()
        {
            Properties.Settings.Default.CustomBrowsers = JsonConvert.SerializeObject(customBrowsers);
            Properties.Settings.Default.Save();
        }

        
        // Bookmark reset methods
        private void ResetAllBookmarksButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset ALL bookmarks? This cannot be undone.",
                "Reset All Bookmarks", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var dbPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AppLauncher", "BookmarkManager", "uprix.db");

                if (System.IO.File.Exists(dbPath))
                {
                    System.IO.File.Delete(dbPath);
                    MessageBox.Show("All bookmarks have been reset.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ResetBookmarkFavButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset all bookmark favorites?",
                "Reset Bookmark Favorites", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var dbPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "AppLauncher", "BookmarkManager", "uprix.db");

                    using (var db = new LiteDB.LiteDatabase(dbPath))
                    {
                        var col = db.GetCollection<dynamic>("bookmarks");
                        var all = col.FindAll();
                        foreach (var item in all)
                        {
                            item.Favorite = false;
                            col.Update(item);
                        }
                    }

                    MessageBox.Show("All bookmark favorites have been reset.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ResetFavButton_Click(object sender, RoutedEventArgs e)
        {
            // دیتابیس رو ریست کن
            DatabaseHelper.ResetFavorite();

            // لیست برنامه‌ها در حافظه رو هم ریست کن
            foreach (var app in allApps)
            {
                app.Favorite = false;
            }

            // دوباره لیست رو بازسازی کن
            filteredApps = allApps.ToList();
            currentPage = 0;
            main.RenderCurrentPage();

            System.Windows.MessageBox.Show("All favorites and usage history have been reset.",
                "Reset Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ResetRateButton_Click(object sender, RoutedEventArgs e)
        {
            // دیتابیس رو ریست کن
            DatabaseHelper.ResetRate();

            // لیست برنامه‌ها در حافظه رو هم ریست کن
            foreach (var app in allApps)
            {
                app.Rate = 0;
            }

            // دوباره لیست رو بازسازی کن
            filteredApps = allApps.ToList();
            currentPage = 0;
            main.RenderCurrentPage();

            _ = MessageBox.Show("All usage history have been reset.",
                "Reset Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetTheme_Click(object sender, RoutedEventArgs e)
        {
            if(sender is System.Windows.Controls.Button button)
            {
                penndingTheme = button.Name;
                StatusSelectedTheme.Text = $"Selected {button.Name} theme";
            }
        }
        public static async Task<bool> IsInternetAvailable()
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };
                var response = await client.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        private string GenerateCode(string systemName, string machineName, string windowsVersion)
        {
            string baseString = $"{systemName}|{machineName}|{windowsVersion}";

            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(baseString));

            Random rnd = new Random();
            string randomSuffix = "";
            for (int i = 0; i < 6; i++)
            {
                randomSuffix += (char)rnd.Next(33, 126);
            }

            return encoded + "-" + randomSuffix;
        }
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@gmail\.com$";
            return Regex.IsMatch(email, pattern);
        }
        private async Task<bool> DoesEmailExist(string email)
        {
            using (var client = new HttpClient())
            {
                string apiKey = "<API-KEY>";
                string url = $"https://emailvalidation.abstractapi.com/v1/?api_key={apiKey}&email={email}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode) return false;

                var result = await response.Content.ReadAsStringAsync();

                dynamic json = JsonConvert.DeserializeObject(result);
                return json.deliverability == "DELIVERABLE";
            }
        }


        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult dia = (DialogResult)System.Windows.
            //    MessageBox.Show($"Are you sure you want to apply the {penndingTheme} theme?",
            //    "Apply Theme",MessageBoxButton.YesNo ,MessageBoxImage.Question);
            lastTheme = Properties.Settings.Default.Theme;
            //Properties.Settings.Default.Theme = dia == DialogResult.Yes ?
            //    penndingTheme : Properties.Settings.Default.Theme;
            Properties.Settings.Default.Theme = penndingTheme;
            Properties.Settings.Default.Save();
            if (lastTheme != Properties.Settings.Default.Theme)
            {
                //System.Windows.MessageBox.Show($"{penndingTheme} theme applied");
                main.SetTheme();
                SetTheme();
            }
        }
    }
}
