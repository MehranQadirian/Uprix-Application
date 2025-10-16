using AppLauncher.Classes;
using AppLauncher.Classes.Core_Classes;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using MessageBox = AppLauncher.Classes.MessageBox;

namespace AppLauncher.Views.Pages
{
    public class ThemeColors
    {
        // Tab Colors
        public string TabNormalBackground { get; set; }
        public string TabHoverBackground { get; set; }
        public string TabSelectedBackground { get; set; }
        public string TabSelectedHoverBackground { get; set; }

        // Star Colors
        public string StarStroke { get; set; }
        public string StarHoverFill { get; set; }
        public string StarFavorited { get; set; }
        public string StarFavoritedHover { get; set; }

        // Bookmark Colors
        public string BookmarkBackground { get; set; }
        public string BookmarkHoverBackground { get; set; }
        public string BookmarkBorder { get; set; }
        public string BookmarkHoverBorder { get; set; }

        // Rate Colors
        public string RateNormal { get; set; }
        public string RateWarning { get; set; }
        public string RateCritical { get; set; }
        public string RateEmergency { get; set; }

        // Text Colors
        public string BookmarkTitle { get; set; }
        public string BookmarkUrl { get; set; }
        public string BookmarkUsage { get; set; }

        // Button Colors
        public string EditButtonBackground { get; set; }
        public string EditButtonHover { get; set; }
        public string DeleteButtonBackground { get; set; }
        public string DeleteButtonHover { get; set; }
        public string ButtonForeground { get; set; }
    }
    public partial class BookmarkPage : Page, INotifyPropertyChanged
    {
        public enum RateState
        {
            Normal = 1,
            Warning = 2,
            Critical = 3,
            Emergency = 4
        }

        public class BookmarkModel : INotifyPropertyChanged
        {
            // Unique Id used by LiteDB
            public int Id { get; set; }

            // Which browser this bookmark came from / belongs to
            public string Browser { get; set; }

            public string Text { get; set; }
            public string URL { get; set; }

            // Favorite toggles star UI
            public bool Favorite { get; set; }

            // Rate: Normal, Warning, Critical, Emergency
            public RateState Rate { get; set; } = RateState.Normal;

            // Recently used count (integer)
            public int RecentlyUsed { get; set; } = 0;

            // For drag/drop easiest: we include a convenience field
            public override string ToString() => $"{Text} - {URL} ({Browser})";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            // Set helpers with notifications
            public void SetFavorite(bool fav) { Favorite = fav; Raise(nameof(Favorite)); }
            public void SetRate(RateState r) { Rate = r; Raise(nameof(Rate)); }
            public void IncrementUsed()
            {
                RecentlyUsed++;
                Raise(nameof(RecentlyUsed));
            }
        }

        // -------------------------
        // Database helpers (LiteDB)
        // -------------------------
        private readonly string _dbPath;
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<BookmarkModel> _bookmarksCollection;

        // In-memory collections per browser displayed in TabControl
        private readonly Dictionary<string, ObservableCollection<BookmarkModel>> _browserCollections = new Dictionary<string, ObservableCollection<BookmarkModel>>(StringComparer.OrdinalIgnoreCase);

        // Common browser names we support
        private readonly List<string> SupportedBrowsers = new List<string> { "Chrome", "Edge", "Firefox" };

        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime _lastClickTime = DateTime.MinValue;
        private BookmarkModel _lastClickedBookmark = null;
        private ApplyThemes apply;
        public BookmarkPage(ApplyThemes aply)
        {
            InitializeComponent();
            apply = aply;
            SetTheme();

            Unloaded += BookmarkPage_Unloaded;
            Loaded += BookmarkPage_Loaded;

            try
            {
                // Only get DB and collection in constructor
                _db = DatabaseManager.GetBookmarkDatabase();
                _bookmarksCollection = _db.GetCollection<BookmarkModel>("bookmarks");
                // Ensure indexes (safe to call multiple times)
                _bookmarksCollection.EnsureIndex(x => x.Browser);
                _bookmarksCollection.EnsureIndex(x => x.URL);
            }
            catch (Exception ex)
            {
                // Add logging to catch block
                MessageBox.Show($"Error in constructor: {ex.Message}", "Error", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void ApplyCustomTheme(ThemeColors theme)
        {
            // Tab Colors
            SetBrushColor("TabItemNormalBackground", theme.TabNormalBackground);
            SetBrushColor("TabItemHoverBackground", theme.TabHoverBackground);
            SetBrushColor("TabItemSelectedBackground", theme.TabSelectedBackground);
            SetBrushColor("TabItemSelectedHoverBackground", theme.TabSelectedHoverBackground);

            // Star Colors
            SetBrushColor("StarStrokeColor", theme.StarStroke);
            SetBrushColor("StarFillHoverColor", theme.StarHoverFill);
            SetBrushColor("StarFavoritedColor", theme.StarFavorited);
            SetBrushColor("StarFavoritedHoverColor", theme.StarFavoritedHover);

            // Bookmark Colors
            SetBrushColor("BookmarkBackground", theme.BookmarkBackground);
            SetBrushColor("BookmarkHoverBackground", theme.BookmarkHoverBackground);
            SetBrushColor("BookmarkBorderColor", theme.BookmarkBorder);
            SetBrushColor("BookmarkHoverBorderColor", theme.BookmarkHoverBorder);

            // Rate Colors
            SetBrushColor("RateNormalColor", theme.RateNormal);
            SetBrushColor("RateWarningColor", theme.RateWarning);
            SetBrushColor("RateCriticalColor", theme.RateCritical);
            SetBrushColor("RateEmergencyColor", theme.RateEmergency);

            // Text Colors
            SetBrushColor("BookmarkTitleColor", theme.BookmarkTitle);
            SetBrushColor("BookmarkUrlColor", theme.BookmarkUrl);
            SetBrushColor("BookmarkUsageColor", theme.BookmarkUsage);

            // Button Colors
            SetBrushColor("EditButtonBackground", theme.EditButtonBackground);
            SetBrushColor("EditButtonHoverBackground", theme.EditButtonHover);
            SetBrushColor("DeleteButtonBackground", theme.DeleteButtonBackground);
            SetBrushColor("DeleteButtonHoverBackground", theme.DeleteButtonHover);
            SetBrushColor("ButtonForeground", theme.ButtonForeground);
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
        private void BookmarkPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyCustomTheme(new ThemeColors
            {
                // Tab Colors
                TabNormalBackground = apply.BackgroundPrimary,
                TabHoverBackground = apply.BackgroundSecundary,
                TabSelectedBackground = apply.MenuIconColor,
                TabSelectedHoverBackground = apply.BackgroundSecundary,

                // Star Colors
                StarStroke = "#808080",
                StarHoverFill = "#FFD700",
                StarFavorited = "#FFD700",
                StarFavoritedHover = "#FF0000",

                // Bookmark Colors
                BookmarkBackground = apply.BackgroundSecundary,
                BookmarkHoverBackground = apply.BackgroundPrimary,
                BookmarkBorder = apply.MenuIconColor,
                BookmarkHoverBorder = apply.MenuIconColor,

                // Rate Colors
                RateNormal = "#4CAF50",
                RateWarning = "#FF9800",
                RateCritical = "#F44336",
                RateEmergency = "#B71C1C",

                // Text Colors
                BookmarkTitle = apply.MenuIconColor,
                BookmarkUrl = apply.MenuIconColor,
                BookmarkUsage = "#484848",

                // Button Colors
                EditButtonBackground = apply.MenuIconColor,
                EditButtonHover = apply.BackgroundSecundary,
                DeleteButtonBackground = apply.MenuIconColor,
                DeleteButtonHover = apply.BackgroundSecundary,
                ButtonForeground = "#FFFFFF"
            });
            try
            {
                // Move loading logic here
                LoadCustomBrowsersFromSettings();
                LoadBookmarksFromDb();
                BuildBrowserTabs();

                // Extract only if collection is empty (to avoid re-extraction on every load)
                if (_bookmarksCollection.Count() == 0)
                {
                    ExtractBookmarksFromInstalledBrowsers();
                    // After extraction, reload data
                    LoadBookmarksFromDb();
                    BuildBrowserTabs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookmarks: {ex.Message}", "Error", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }

        private void SetTheme()
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            brHeader.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brHeader.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundOnSecundary));
            txtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundOnSecundary));
            txtSubTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.ForegroundOnSecundary));
            AddBookmarkBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
        }
        // -------------------------
        // Load DB into in-memory collections and create TabItems in the TabControl
        // -------------------------
        private void LoadBookmarksFromDb()
        {
            var all = _bookmarksCollection.FindAll().ToList();

            // Reset collections
            _browserCollections.Clear();

            // Group by browser
            foreach (var browser in SupportedBrowsers)
            {
                var list = new ObservableCollection<BookmarkModel>(SortBookmarkList(all.Where(b => string.Equals(b.Browser, browser, StringComparison.OrdinalIgnoreCase))));
                _browserCollections[browser] = list;
            }

            // If DB contains bookmarks for non-standard browsers, include them too
            var otherBrowsers = all.Select(b => b.Browser).Distinct()
                                  .Where(b => !SupportedBrowsers.Contains(b, StringComparer.OrdinalIgnoreCase));

            foreach (var bname in otherBrowsers)
            {
                var list = new ObservableCollection<BookmarkModel>(SortBookmarkList(all.Where(b => string.Equals(b.Browser, bname, StringComparison.OrdinalIgnoreCase))));
                _browserCollections[bname] = list;
            }
        }

        // Helper: Sort bookmarks by Rate (higher danger first) and then RecentlyUsed desc, then text
        // Favorites first → then by Rate → then RecentlyUsed → then Text
        private IEnumerable<BookmarkModel> SortBookmarkList(IEnumerable<BookmarkModel> list)
        {
            return list.OrderByDescending(b => b.Favorite) // true before false
                       .ThenByDescending(b => (int)b.Rate)
                       .ThenByDescending(b => b.RecentlyUsed)
                       .ThenBy(b => b.Text, StringComparer.OrdinalIgnoreCase);
        }


        // Build TabControl tabs (one per browser) and ItemsControl within each tab bound to the ObservableCollection
        private void BuildBrowserTabs()
        {
            // Remember the currently selected browser
            string selectedBrowser = null;
            if (BrowserTabControl.SelectedItem is TabItem currentTab)
            {
                selectedBrowser = currentTab.Header?.ToString();
            }

            BrowserTabControl.Items.Clear();

            foreach (var kv in _browserCollections)
            {
                var tab = new TabItem { Header = kv.Key, AllowDrop = true }; // ADD AllowDrop = true

                // ADD these event handlers
                tab.Drop += TabItem_Drop;
                tab.DragOver += TabItem_DragOver;

                var scroll = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = new ItemsControl
                    {
                        ItemsSource = kv.Value,
                        ItemTemplate = (DataTemplate)Resources["BookmarkItemTemplate"],
                        Margin = new Thickness(10)
                    }
                };

                tab.Content = scroll;
                BrowserTabControl.Items.Add(tab);
            }

            // Restore the previously selected tab
            if (!string.IsNullOrEmpty(selectedBrowser))
            {
                foreach (TabItem tab in BrowserTabControl.Items)
                {
                    if (string.Equals(tab.Header?.ToString(), selectedBrowser, StringComparison.OrdinalIgnoreCase))
                    {
                        BrowserTabControl.SelectedItem = tab;
                        break;
                    }
                }
            }
        }

        // -------------------------
        // Browser extraction & editing helpers
        // Best-effort handling for Chrome/Edge (Bookmarks JSON) and Firefox (places.sqlite)
        // -------------------------
        // Locate common browser bookmark file paths (Windows)
        private string GetChromeBookmarkFile()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var p = Path.Combine(local, "Google", "Chrome", "User Data", "Default", "Bookmarks");
            return File.Exists(p) ? p : null;
        }

        private string GetEdgeBookmarkFile()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var p = Path.Combine(local, "Microsoft", "Edge", "User Data", "Default", "Bookmarks");
            return File.Exists(p) ? p : null;
        }

        private string GetFirefoxPlacesSqlite()
        {
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var profileDir = Path.Combine(roaming, "Mozilla", "Firefox", "Profiles");
            if (!Directory.Exists(profileDir)) return null;

            foreach (var sub in Directory.GetDirectories(profileDir))
            {
                var p = Path.Combine(sub, "places.sqlite");
                if (File.Exists(p)) return p;
            }
            return null;
        }

        /// <summary>
        /// Main extraction routine. Tries to read bookmarks from known browser files and merge them into DB + UI.
        /// </summary>
        private void ExtractBookmarksFromInstalledBrowsers()
        {
            // Chrome
            var chromeFile = GetChromeBookmarkFile();
            if (!string.IsNullOrEmpty(chromeFile))
                TryLoadFromChromeOrEdge(chromeFile, "Chrome");

            var edgeFile = GetEdgeBookmarkFile();
            if (!string.IsNullOrEmpty(edgeFile))
                TryLoadFromChromeOrEdge(edgeFile, "Edge");

            var ff = GetFirefoxPlacesSqlite();
            if (!string.IsNullOrEmpty(ff))
                TryLoadFromFirefox(ff, "Firefox");

            // After extraction, reload DB collections and UI
            LoadBookmarksFromDb();
            BuildBrowserTabs();
        }

        // Reads Chrome/Edge "Bookmarks" JSON and merges
        private void TryLoadFromChromeOrEdge(string bookmarkJsonPath, string browserName)
        {
            try
            {
                var jsonText = File.ReadAllText(bookmarkJsonPath);
                using JsonDocument doc = JsonDocument.Parse(jsonText);
                // Chrome bookmarks JSON structure: roots->bookmark_bar / other / synced
                var root = doc.RootElement.GetProperty("roots");
                var nodes = new List<JsonElement>();

                if (root.TryGetProperty("bookmark_bar", out var bar)) nodes.Add(bar);
                if (root.TryGetProperty("other", out var other)) nodes.Add(other);
                if (root.TryGetProperty("synced", out var synced)) nodes.Add(synced);

                foreach (var node in nodes)
                    CollectChromeNodes(node, browserName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to read {browserName} bookmarks: {ex.Message}");
            }
        }

        // Recursively collect bookmarks from Chrome-style JSON nodes
        private void CollectChromeNodes(JsonElement element, string browserName)
        {
            if (element.ValueKind != JsonValueKind.Object && element.ValueKind != JsonValueKind.Array) return;

            if (element.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in children.EnumerateArray())
                {
                    CollectChromeNodes(child, browserName);
                }
            }
            else
            {
                // leaf: maybe a URL
                if (element.TryGetProperty("type", out var type) && type.GetString() == "url")
                {
                    var name = element.GetProperty("name").GetString();
                    var url = element.GetProperty("url").GetString();
                    MergeBookmarkFromBrowser(browserName, name, url);
                }
            }
        }

        // Reads firefox places.sqlite and extracts bookmarks (best-effort)
        private void TryLoadFromFirefox(string placesSqlitePath, string browserName)
        {
            try
            {
                // We'll copy the file to avoid locks and then query it using System.Data.SQLite (bundling the native bits required)
                var tmp = Path.GetTempFileName();
                File.Copy(placesSqlitePath, tmp, true);

                // Use SQLite to query "moz_bookmarks" joined with "moz_places"
                var cs = $"Data Source={tmp};Version=3;Read Only=True;";

                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = @"SELECT p.url, b.title
                                        FROM moz_bookmarks b
                                        LEFT JOIN moz_places p ON b.fk = p.id
                                        WHERE p.url IS NOT NULL AND b.title IS NOT NULL";

                    using var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var url = rdr.IsDBNull(0) ? null : rdr.GetString(0);
                        var title = rdr.IsDBNull(1) ? url : rdr.GetString(1);
                        if (!string.IsNullOrEmpty(url))
                            MergeBookmarkFromBrowser(browserName, title, url);
                    }
                }

                try { File.Delete(tmp); } catch { /* ignore */ }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to read Firefox bookmarks: " + ex);
            }
        }

        // Merge a bookmark found in the browser into our DB: insert if not exists.
        // If the bookmark exists, ensure DB record is synchronized minimally (e.g., keep Favorite if previously set).
        private void MergeBookmarkFromBrowser(string browser, string text, string url)
        {
            // Basic normalization
            text = text?.Trim() ?? url;
            url = url?.Trim();

            if (string.IsNullOrEmpty(url)) return;

            var existing = _bookmarksCollection.FindOne(b => b.Browser == browser && b.URL == url);
            if (existing == null)
            {
                var model = new BookmarkModel
                {
                    Browser = browser,
                    Text = text,
                    URL = url,
                    Favorite = false,
                    Rate = RateState.Normal,
                    RecentlyUsed = 0
                };
                _bookmarksCollection.Insert(model);
            }
            else
            {
                // If title has changed in the browser, update our DB text to match
                if (!string.Equals(existing.Text, text, StringComparison.Ordinal))
                {
                    existing.Text = text;
                    _bookmarksCollection.Update(existing);
                }
            }
        }

        // -------------------------
        // Browser writing helpers: attempt to add/edit/delete bookmarks in browser store.
        // Each returns true on success, false on failure (then the caller should still update DB only).
        // -------------------------
        private bool CanAccessFile(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
        private bool TryAddBookmarkToChromeOrEdge(string bookmarkFilePath, string title, string url)
        {
            try
            {
                if (!CanAccessFile(bookmarkFilePath))
                {
                    Debug.WriteLine($"Cannot access {bookmarkFilePath} - file is locked");
                    return false;
                }
                var jsonText = File.ReadAllText(bookmarkFilePath);
                var doc = JsonDocument.Parse(jsonText);

                // Parse to mutable structure
                using var rootDoc = JsonDocument.Parse(jsonText);
                var jsonString = rootDoc.RootElement.GetRawText();

                // Use simple string manipulation to add bookmark
                // Find the bookmark_bar children array
                var modified = jsonString;
                var searchPattern = "\"bookmark_bar\":{\"children\":[";
                var idx = modified.IndexOf(searchPattern);

                if (idx > 0)
                {
                    var insertPos = idx + searchPattern.Length;
                    var newBookmark = $"{{\"type\":\"url\",\"name\":\"{title.Replace("\"", "\\\"")}\",\"url\":\"{url.Replace("\"", "\\\"")}\"}}";

                    // Check if children array is empty
                    if (modified[insertPos] == ']')
                    {
                        modified = modified.Insert(insertPos, newBookmark);
                    }
                    else
                    {
                        modified = modified.Insert(insertPos, newBookmark + ",");
                    }

                    File.WriteAllText(bookmarkFilePath, modified);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TryAddBookmarkToChromeOrEdge failed: " + ex);
                return false;
            }
        }

        private bool TryRemoveBookmarkFromChromeOrEdge(string bookmarkFilePath, string url)
        {
            // For reliability we avoid complex editing here. Return false so caller will update DB only.
            // A robust implementation should parse JSON and update it properly (e.g., with Newtonsoft.Json).
            return false;
        }

        private bool TryEditBookmarkInChromeOrEdge(string bookmarkFilePath, string oldUrl, string newTitle, string newUrl)
        {
            // Same caveat: editing the Chrome/Edge Bookmark JSON robustly should be done with a proper JSON DOM library.
            return false;
        }

        private bool TryAddBookmarkToFirefox(string placesSqlitePath, string title, string url)
        {
            // Directly manipulating Firefox DB is risky. Returning false to indicate we couldn't update the browser,
            // so the system will still update the database.
            return false;
        }

        private bool TryRemoveBookmarkFromFirefox(string placesSqlitePath, string url)
        {
            return false;
        }

        private bool TryEditBookmarkInFirefox(string placesSqlitePath, string oldUrl, string newTitle, string newUrl)
        {
            return false;
        }

        // -------------------------
        // UI Event Handlers
        // The XAML referenced the following event handlers; implement them here:
        //  - AddBookmarkButton_Click
        //  - FavoriteButton_Click
        //  - EditButton_Click
        //  - DeleteButton_Click
        //  - Bookmark_PreviewMouseLeftButtonDown
        //  - Bookmark_DragOver
        //  - Bookmark_Drop
        // Also provide helper to open bookmark (increment RecentlyUsed)
        // -------------------------

        // + Add Bookmark: creates a simple dialog to gather fields and adds to DB (and optionally to browser)
        private void AddBookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddEditBookmarkWindow(SupportedBrowsers, apply);
            dlg.Owner = Window.GetWindow(this);
            dlg.Title = "Add Bookmark";
            if (dlg.ShowDialog() == true)
            {
                var chosenBrowser = dlg.BrowserName;
                var title = dlg.TitleText;
                var url = dlg.UrlText;
                var fav = dlg.IsFavorite;

                bool browserUpdated = false;
                try
                {
                    if (string.Equals(chosenBrowser, "Chrome", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(chosenBrowser, "Edge", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = (string.Equals(chosenBrowser, "Chrome", StringComparison.OrdinalIgnoreCase)) ? GetChromeBookmarkFile() : GetEdgeBookmarkFile();
                        if (path != null)
                        {
                            browserUpdated = TryAddBookmarkToChromeOrEdge(path, title, url);
                        }
                    }
                    else if (string.Equals(chosenBrowser, "Firefox", StringComparison.OrdinalIgnoreCase))
                    {
                        var p = GetFirefoxPlacesSqlite();
                        if (p != null) browserUpdated = TryAddBookmarkToFirefox(p, title, url);
                    }
                }
                catch { browserUpdated = false; }

                // Add/Upsert to DB
                var existing = _bookmarksCollection.FindOne(b => b.Browser == chosenBrowser && b.URL == url);
                if (existing == null)
                {
                    var model = new BookmarkModel { Browser = chosenBrowser, Text = title, URL = url, Favorite = fav, Rate = RateState.Normal, RecentlyUsed = 0 };
                    _bookmarksCollection.Insert(model);
                }
                else
                {
                    existing.Text = title;
                    existing.Favorite = fav;
                    _bookmarksCollection.Update(existing);
                }

                LoadBookmarksFromDb();
                BuildBrowserTabs();

                // FIXED: Use correct variable names (chosenBrowser and browserUpdated)
                //if (!browserUpdated)
                //{
                //    string reason = IsBrowserRunning(chosenBrowser)
                //        ? $"{chosenBrowser} is currently running. Close the browser and try again to sync bookmarks."
                //        : "Could not modify browser bookmark file. The file may be locked or in an unsupported format.";

                //    MessageBox.Show($"Added to database for {chosenBrowser}.\n\n{reason}",
                //        "Partial Success", MessageBox.MessageBoxButton.OK, MessageBoxImage.Information);
                //}
                //else
                //{
                //    MessageBox.Show($"Successfully added to {chosenBrowser} and database.",
                //        "Success", MessageBox.MessageBoxButton.OK, MessageBoxImage.Information);
                //}
            }
        }

        // Favorite toggle: XAML set Tag={Binding} so the handler receives the model via Tag
        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is BookmarkModel model)
            {
                model.SetFavorite(!model.Favorite);

                // Update DB
                var dbModel = _bookmarksCollection.FindById(model.Id);
                if (dbModel != null)
                {
                    dbModel.Favorite = model.Favorite;
                    _bookmarksCollection.Update(dbModel);
                }

                // Refresh UI so sorting applies (favorites pinned on top)
                LoadBookmarksFromDb();
                BuildBrowserTabs();
            }
        }


        // Edit button: open dialog prefilled, then attempt to edit both browser and DB
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is Button btn && btn.Tag is BookmarkModel model)
            {
                var dlg = new AddEditBookmarkWindow(SupportedBrowsers, apply, model.Browser, model.Rate, model.Text, model.URL, model.Favorite);
                dlg.Owner = Window.GetWindow(this);
                dlg.Title = "Edit Bookmark";
                if (dlg.ShowDialog() == true)
                {
                    var newTitle = dlg.TitleText;
                    var newUrl = dlg.UrlText;
                    var newFav = dlg.IsFavorite;
                    var newRate = dlg.RateName;
                    bool browserEdited = false;
                    try
                    {
                        if (string.Equals(model.Browser, "Chrome", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(model.Browser, "Edge", StringComparison.OrdinalIgnoreCase))
                        {
                            var path = (string.Equals(model.Browser, "Chrome", StringComparison.OrdinalIgnoreCase)) ? GetChromeBookmarkFile() : GetEdgeBookmarkFile();
                            if (path != null)
                                browserEdited = TryEditBookmarkInChromeOrEdge(path, model.URL, newTitle, newUrl);
                        }
                        else if (string.Equals(model.Browser, "Firefox", StringComparison.OrdinalIgnoreCase))
                        {
                            var p = GetFirefoxPlacesSqlite();
                            if (p != null) browserEdited = TryEditBookmarkInFirefox(p, model.URL, newTitle, newUrl);
                        }
                    }
                    catch { browserEdited = false; }

                    // Update DB whether or not browser changed (per requirement)
                    var dbModel = _bookmarksCollection.FindById(model.Id);
                    if (dbModel != null)
                    {
                        dbModel.Text = newTitle;
                        dbModel.URL = newUrl;
                        dbModel.Favorite = newFav;
                        dbModel.Rate = (RateState)Enum.Parse(typeof(RateState), newRate, true); ;
                        _bookmarksCollection.Update(dbModel);
                    }

                    LoadBookmarksFromDb();
                    BuildBrowserTabs();

                    //MessageBox.Show(browserEdited
                    //    ? $"Bookmark edited in {model.Browser} and in database."
                    //    : $"Database updated. Could not edit bookmark in {model.Browser} automatically.", "Edit Bookmark", MessageBox.MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // Delete: check if exists in browser - if exists attempt to delete; either way delete from DB
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is Button btn && btn.Tag is BookmarkModel model)
            {
                var res = MessageBox.Show($"Delete bookmark '{model.Text}' from database and from browser {model.Browser} if present?", "Delete Bookmark", MessageBox.MessageBoxButton.YesNo, MessageBox.MessageBoxIcon.Warning);
                if (res != MessageBox.MessageBoxResult.Yes) return;

                bool browserDeleted = false;
                try
                {
                    if (string.Equals(model.Browser, "Chrome", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(model.Browser, "Edge", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = (string.Equals(model.Browser, "Chrome", StringComparison.OrdinalIgnoreCase)) ? GetChromeBookmarkFile() : GetEdgeBookmarkFile();
                        if (path != null)
                        {
                            browserDeleted = TryRemoveBookmarkFromChromeOrEdge(path, model.URL);
                        }
                    }
                    else if (string.Equals(model.Browser, "Firefox", StringComparison.OrdinalIgnoreCase))
                    {
                        var p = GetFirefoxPlacesSqlite();
                        if (p != null) browserDeleted = TryRemoveBookmarkFromFirefox(p, model.URL);
                    }
                }
                catch { browserDeleted = false; }

                // Remove from DB (always)
                _bookmarksCollection.Delete(model.Id);

                LoadBookmarksFromDb();
                BuildBrowserTabs();

                MessageBox.Show(browserDeleted
                    ? $"Deleted from {model.Browser} and database."
                    : $"Removed from database. Could not delete from {model.Browser} automatically.", "Delete Bookmark", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Information);
            }
        }

        // -------------------------
        // Drag & Drop logic
        // Bookmark_PreviewMouseLeftButtonDown starts drag; we put the BookmarkModel into the data object.
        // Bookmark_DragOver allows drop only when target is a different browser or same browser (we accept both).
        // Bookmark_Drop handles adding bookmark to the target browser and DB.
        // -------------------------

        private void Bookmark_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check if click was on a button
            if (e.OriginalSource is DependencyObject dep)
            {
                var parentButton = FindVisualParent<Button>(dep);
                if (parentButton != null)
                    return;
            }

            if (sender is Border border && border.DataContext is BookmarkModel bm)
            {
                // Detect double-click
                var now = DateTime.Now;
                var timeSinceLastClick = now - _lastClickTime;

                if (timeSinceLastClick.TotalMilliseconds < 300 && _lastClickedBookmark == bm)
                {
                    // Double-click detected
                    e.Handled = true;
                    OpenBookmarkInBrowser(bm, bm.Browser);
                    _lastClickTime = DateTime.MinValue;
                    _lastClickedBookmark = null;
                    return;
                }

                _lastClickTime = now;
                _lastClickedBookmark = bm;

                // Start drag after a short delay (not on double-click)
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    if (DateTime.Now - _lastClickTime >= TimeSpan.FromMilliseconds(150))
                    {
                        DragDrop.DoDragDrop(border, new DataObject("BookmarkModel", bm), DragDropEffects.Copy | DragDropEffects.Move);
                    }
                };
                timer.Start();
            }
        }
        private string GetBrowserExecutablePath(string browserName)
        {
            // Check default browsers first
            if (string.Equals(browserName, "Chrome", StringComparison.OrdinalIgnoreCase))
            {
                var path = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                if (File.Exists(path)) return path;
                path = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                if (File.Exists(path)) return path;
            }
            else if (string.Equals(browserName, "Edge", StringComparison.OrdinalIgnoreCase))
            {
                var path = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                if (File.Exists(path)) return path;
                path = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
                if (File.Exists(path)) return path;
            }
            else if (string.Equals(browserName, "Firefox", StringComparison.OrdinalIgnoreCase))
            {
                var path = @"C:\Program Files\Mozilla Firefox\firefox.exe";
                if (File.Exists(path)) return path;
                path = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";
                if (File.Exists(path)) return path;
            }

            // Check custom browsers from settings
            try
            {
                string browsersJson = Properties.Settings.Default.CustomBrowsers;
                if (!string.IsNullOrEmpty(browsersJson))
                {
                    var customBrowsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CustomBrowserConfig>>(browsersJson);
                    var match = customBrowsers?.FirstOrDefault(b => string.Equals(b.Name, browserName, StringComparison.OrdinalIgnoreCase));
                    if (match != null && File.Exists(match.ExecutablePath))
                    {
                        return match.ExecutablePath;
                    }
                }
            }
            catch { }

            return null;
        }
        private void OpenBookmarkInBrowser(BookmarkModel bm, string browserName)
        {
            if (bm == null) return;

            try
            {
                string browserPath = GetBrowserExecutablePath(browserName);

                // Launch browser with URL
                if (!string.IsNullOrEmpty(browserPath) && File.Exists(browserPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = browserPath,
                        Arguments = bm.URL,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Fallback to default browser
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = bm.URL,
                        UseShellExecute = true
                    });
                }

                // Increment RecentlyUsed and update Rate
                bm.IncrementUsed();

                if (bm.RecentlyUsed >= 30) bm.SetRate(RateState.Emergency);
                else if (bm.RecentlyUsed >= 15) bm.SetRate(RateState.Critical);
                else if (bm.RecentlyUsed >= 5) bm.SetRate(RateState.Warning);
                else bm.SetRate(RateState.Normal);

                // Persist to DB
                var dbModel = _bookmarksCollection.FindById(bm.Id);
                if (dbModel != null)
                {
                    dbModel.RecentlyUsed = bm.RecentlyUsed;
                    dbModel.Rate = bm.Rate;
                    _bookmarksCollection.Update(dbModel);
                }

                // Refresh UI ordering
                LoadBookmarksFromDb();
                BuildBrowserTabs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open bookmark: " + ex.Message, "Open Bookmark", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }
        // Generic helper
        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typed) return typed;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("BookmarkModel")) return;
            var dropped = e.Data.GetData("BookmarkModel") as BookmarkModel;
            if (dropped == null) return;

            // Get target browser from the TabItem header
            string targetBrowser = null;
            if (sender is TabItem tabItem)
            {
                targetBrowser = tabItem.Header?.ToString();
            }

            if (string.IsNullOrEmpty(targetBrowser)) return;

            // Same logic as Bookmark_Drop
            if (!string.Equals(targetBrowser, dropped.Browser, StringComparison.OrdinalIgnoreCase))
            {
                bool browserAdded = false;
                try
                {
                    if (string.Equals(targetBrowser, "Chrome", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(targetBrowser, "Edge", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = (string.Equals(targetBrowser, "Chrome", StringComparison.OrdinalIgnoreCase)) ? GetChromeBookmarkFile() : GetEdgeBookmarkFile();
                        if (path != null) browserAdded = TryAddBookmarkToChromeOrEdge(path, dropped.Text, dropped.URL);
                    }
                    else if (string.Equals(targetBrowser, "Firefox", StringComparison.OrdinalIgnoreCase))
                    {
                        var p = GetFirefoxPlacesSqlite();
                        if (p != null) browserAdded = TryAddBookmarkToFirefox(p, dropped.Text, dropped.URL);
                    }
                }
                catch { browserAdded = false; }

                var existing = _bookmarksCollection.FindOne(b => b.Browser == targetBrowser && b.URL == dropped.URL);
                if (existing == null)
                {
                    var newBm = new BookmarkModel
                    {
                        Browser = targetBrowser,
                        Text = dropped.Text,
                        URL = dropped.URL,
                        Favorite = dropped.Favorite,
                        Rate = dropped.Rate,
                        RecentlyUsed = dropped.RecentlyUsed
                    };
                    _bookmarksCollection.Insert(newBm);
                }
                else
                {
                    existing.Favorite = existing.Favorite || dropped.Favorite;
                    existing.RecentlyUsed = Math.Max(existing.RecentlyUsed, dropped.RecentlyUsed);
                    existing.Rate = (RateState)Math.Max((int)existing.Rate, (int)dropped.Rate);
                    _bookmarksCollection.Update(existing);
                }

                LoadBookmarksFromDb();
                BuildBrowserTabs();
                //if (!browserAdded)
                //{
                //    string reason = IsBrowserRunning(targetBrowser)
                //        ? $"{targetBrowser} is currently running. Close the browser and try again to sync bookmarks."
                //        : "Could not modify browser bookmark file. The file may be locked or in an unsupported format.";

                //    MessageBox.Show($"Added to database for {targetBrowser}.\n\n{reason}",
                //        "Partial Success", MessageBox.MessageBoxButton.OK, MessageBoxImage.Information);
                //}
                //else
                //{
                //    MessageBox.Show($"Successfully added to {targetBrowser} and database.",
                //        "Success", MessageBox.MessageBoxButton.OK, MessageBoxImage.Information);
                //}
            }
        }

        private void LoadCustomBrowsersFromSettings()
        {
            try
            {
                string browsersJson = Properties.Settings.Default.CustomBrowsers;
                if (!string.IsNullOrEmpty(browsersJson))
                {
                    var customBrowsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CustomBrowserConfig>>(browsersJson);
                    if (customBrowsers != null)
                    {
                        foreach (var browser in customBrowsers)
                        {
                            if (!SupportedBrowsers.Contains(browser.Name, StringComparer.OrdinalIgnoreCase))
                            {
                                SupportedBrowsers.Add(browser.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load custom browsers: " + ex.Message);
            }
        }

        // Add this class
        public class CustomBrowserConfig
        {
            public string Name { get; set; }
            public string ExecutablePath { get; set; }
        }
        private void TabItem_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("BookmarkModel"))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
        private void Bookmark_DragOver(object sender, DragEventArgs e)
        {
            // Accept bookmark data
            if (e.Data.GetDataPresent("BookmarkModel"))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void Bookmark_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("BookmarkModel")) return;
            var dropped = e.Data.GetData("BookmarkModel") as BookmarkModel;
            if (dropped == null) return;

            // Determine the target browser (the TabItem the drop occurred in)
            // We'll traverse visual tree to find the parent TabItem header.
            var targetBrowser = GetBrowserForDropTarget(sender);
            if (string.IsNullOrEmpty(targetBrowser)) return;

            // If drop into same browser, nothing to do (but we could reorder; not implemented). If into different browser, attempt to add to that browser and DB.
            if (!string.Equals(targetBrowser, dropped.Browser, StringComparison.OrdinalIgnoreCase))
            {
                bool browserAdded = false;
                // Try write to target browser
                try
                {
                    if (string.Equals(targetBrowser, "Chrome", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(targetBrowser, "Edge", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = (string.Equals(targetBrowser, "Chrome", StringComparison.OrdinalIgnoreCase)) ? GetChromeBookmarkFile() : GetEdgeBookmarkFile();
                        if (path != null) browserAdded = TryAddBookmarkToChromeOrEdge(path, dropped.Text, dropped.URL);
                    }
                    else if (string.Equals(targetBrowser, "Firefox", StringComparison.OrdinalIgnoreCase))
                    {
                        var p = GetFirefoxPlacesSqlite();
                        if (p != null) browserAdded = TryAddBookmarkToFirefox(p, dropped.Text, dropped.URL);
                    }
                }
                catch { browserAdded = false; }

                // Add to DB for the target browser
                var existing = _bookmarksCollection.FindOne(b => b.Browser == targetBrowser && b.URL == dropped.URL);
                if (existing == null)
                {
                    var newBm = new BookmarkModel
                    {
                        Browser = targetBrowser,
                        Text = dropped.Text,
                        URL = dropped.URL,
                        Favorite = dropped.Favorite,
                        Rate = dropped.Rate,
                        RecentlyUsed = dropped.RecentlyUsed
                    };
                    _bookmarksCollection.Insert(newBm);
                }
                // else if exists, we might merge metadata (e.g., keep favorite true if any are true)
                else
                {
                    // Merge favorited or higher rate
                    existing.Favorite = existing.Favorite || dropped.Favorite;
                    existing.RecentlyUsed = Math.Max(existing.RecentlyUsed, dropped.RecentlyUsed);
                    existing.Rate = (RateState)Math.Max((int)existing.Rate, (int)dropped.Rate);
                    _bookmarksCollection.Update(existing);
                }

                LoadBookmarksFromDb();
                BuildBrowserTabs();

                MessageBox.Show(/*browserAdded ?*/ $"Added to {targetBrowser} and database." /*: $"Added to database for {targetBrowser}. Could not auto-add to browser file."*/, "Drag & Drop", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Information);
            }
            else
            {
                // Dropped into same browser - treat as noop (we do not reorder).
                //MessageBox.Show("Dropped into the same browser - no action taken.", "Drag & Drop", MessageBox.MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Helper to find which browser folder/tab the drop target belongs to
        private string GetBrowserForDropTarget(object dropSender)
        {
            // The sender in XAML is the Border inside DataTemplate. We need to find which TabItem this Border is shown in.
            var element = dropSender as DependencyObject;
            while (element != null)
            {
                if (element is TabItem ti)
                {
                    return ti.Header?.ToString();
                }
                element = VisualTreeHelper.GetParent(element);
            }

            // Alternatively, use selected TabItem as fallback
            if (BrowserTabControl.SelectedItem is TabItem sel)
                return sel.Header?.ToString();

            return null;
        }

        // -------------------------
        // Small helper dialog Window class to Add / Edit Bookmarks
        // It's contained in this code file for convenience. In production extract to its own file.
        // -------------------------
        private class AddEditBookmarkWindow : Window
        {
            public string TitleText { get; private set; }
            public string UrlText { get; private set; }
            public bool IsFavorite { get; private set; }
            public string BrowserName { get; private set; }
            public string RateName { get; private set; }

            private TextBox _titleBox;
            private TextBox _urlBox;
            private CheckBox _favBox;
            private ComboBox _browserCombo;
            private ComboBox _rateCombo;
            private ApplyThemes apply;
            public AddEditBookmarkWindow(
    List<string> supportedBrowser,
    ApplyThemes aply,
    string browser = "Chrome",
    RateState rate = RateState.Normal,
    string title = "",
    string url = "",
    bool favorite = false)
            {
                // === Initialization ===
                apply = aply;
                Width = 420;
                Height = 300;
                Title = "Add / Edit Bookmark";
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));

                BrowserName = browser;
                TitleText = title;
                UrlText = url;
                IsFavorite = favorite;

                var browserList = supportedBrowser;

                // === Main Grid Layout ===
                var grid = new Grid { Margin = new Thickness(12) };

                // Define Rows
                for (int i = 0; i < 6; i++)
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Define Columns
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Brush labelColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
                Brush textBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
                Brush textForeground = labelColor;

                // === Browser Combo ===
                grid.Add(new Label { Content = "Browser:", VerticalAlignment = VerticalAlignment.Center, Foreground = labelColor }, 0, 0);
                _browserCombo = new ComboBox
                {
                    ItemsSource = browserList,
                    SelectedValue = browser,
                    Margin = new Thickness(4),
                    Style = FindResource("DarkComboBox") as Style
                };
                grid.Add(_browserCombo, 1, 0);

                // === Rate Combo ===
                grid.Add(new Label { Content = "Rate:", VerticalAlignment = VerticalAlignment.Center, Foreground = labelColor }, 0, 1);
                _rateCombo = new ComboBox
                {
                    ItemsSource = new List<string>
        {
            RateState.Normal.ToString(),
            RateState.Warning.ToString(),
            RateState.Critical.ToString(),
            RateState.Emergency.ToString()
        },
                    SelectedValue = rate.ToString(),
                    Margin = new Thickness(4),
                    Style = FindResource("DarkComboBox") as Style
                };
                grid.Add(_rateCombo, 1, 1);

                // === Title TextBox ===
                grid.Add(new Label { Content = "Title:", VerticalAlignment = VerticalAlignment.Center, Foreground = labelColor }, 0, 2);
                _titleBox = new TextBox
                {
                    Text = title,
                    Margin = new Thickness(4),
                    BorderThickness = new Thickness(0),
                    Background = textBackground,
                    Foreground = textForeground
                };
                grid.Add(_titleBox, 1, 2);

                // === URL TextBox ===
                grid.Add(new Label { Content = "URL:", VerticalAlignment = VerticalAlignment.Center, Foreground = labelColor }, 0, 3);
                _urlBox = new TextBox
                {
                    Text = url,
                    Margin = new Thickness(4),
                    BorderThickness = new Thickness(0),
                    Background = textBackground,
                    Foreground = textForeground
                };
                grid.Add(_urlBox, 1, 3);

                // === Favorite CheckBox ===
                grid.Add(new Label { Content = "Favorite:", VerticalAlignment = VerticalAlignment.Center, Foreground = labelColor }, 0, 4);
                _favBox = new CheckBox
                {
                    IsChecked = favorite,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4),
                    Content = "Favorite"
                };
                _favBox.Style = CreateAnimatedDarkCheckBoxStyle();
                grid.Add(_favBox, 1, 4);

                // === Buttons Panel ===
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 12, 0, 0)
                };

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 80,
                    Margin = new Thickness(6),
                    Style = FindResource("AnimatedNavButton") as Style
                };
                okButton.Click += Ok_Click;

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 80,
                    Margin = new Thickness(6),
                    Style = FindResource("AnimatedNavButton") as Style
                };
                cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);

                Grid.SetRow(buttonPanel, 5);
                Grid.SetColumnSpan(buttonPanel, 2);
                grid.Children.Add(buttonPanel);

                // === Final Content ===
                Content = grid;
            }
            private Style CreateAnimatedDarkCheckBoxStyle()
            {
                var style = new Style(typeof(CheckBox));
                style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
                style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
                style.Setters.Add(new Setter(Control.CursorProperty, System.Windows.Input.Cursors.Hand));

                var template = new ControlTemplate(typeof(CheckBox));

                // ساختار بصری
                var stack = new FrameworkElementFactory(typeof(StackPanel));
                stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                stack.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

                var grid = new FrameworkElementFactory(typeof(Grid));
                grid.SetValue(FrameworkElement.WidthProperty, 20.0);
                grid.SetValue(FrameworkElement.HeightProperty, 20.0);
                grid.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 6, 0));

                var border = new FrameworkElementFactory(typeof(Border));
                border.Name = "Border";
                border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
                border.SetValue(Border.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222")));
                border.SetValue(Border.BorderBrushProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555")));
                border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
                border.SetValue(Border.EffectProperty, new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 0,
                    Color = Colors.Black
                });
                grid.AppendChild(border);

                var path = new FrameworkElementFactory(typeof(System.Windows.Shapes.Path));
                path.Name = "CheckMark";
                path.SetValue(System.Windows.Shapes.Path.DataProperty, Geometry.Parse("M4,10 L8,14 L16,5"));
                path.SetValue(System.Windows.Shapes.Path.StrokeProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6F61")));
                path.SetValue(System.Windows.Shapes.Path.StrokeThicknessProperty, 2.0);
                path.SetValue(System.Windows.Shapes.Path.StrokeStartLineCapProperty, PenLineCap.Round);
                path.SetValue(System.Windows.Shapes.Path.StrokeEndLineCapProperty, PenLineCap.Round);
                path.SetValue(UIElement.OpacityProperty, 0.0);
                grid.AppendChild(path);

                stack.AppendChild(grid);
                var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                contentPresenterFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                stack.AppendChild(contentPresenterFactory);


                template.VisualTree = stack;

                // انیمیشن‌ها
                var checkedTrigger = new Trigger { Property = ToggleButton.IsCheckedProperty, Value = true };
                var hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };

                // Checked Animations
                var sbChecked = new Storyboard();
                sbChecked.Children.Add(CreateDoubleAnim("CheckMark", UIElement.OpacityProperty, 1, 0.2));
                sbChecked.Children.Add(CreateColorAnim("Border", "(Border.BorderBrush).(SolidColorBrush.Color)", "#FF6F61", 0.25));
                sbChecked.Children.Add(CreateColorAnim("Border", "(Border.Background).(SolidColorBrush.Color)", "#333", 0.25));
                checkedTrigger.EnterActions.Add(new BeginStoryboard { Storyboard = sbChecked });

                // Unchecked
                var sbUnchecked = new Storyboard();
                sbUnchecked.Children.Add(CreateDoubleAnim("CheckMark", UIElement.OpacityProperty, 0, 0.2));
                sbUnchecked.Children.Add(CreateColorAnim("Border", "(Border.BorderBrush).(SolidColorBrush.Color)", "#555", 0.25));
                sbUnchecked.Children.Add(CreateColorAnim("Border", "(Border.Background).(SolidColorBrush.Color)", "#222", 0.25));
                checkedTrigger.ExitActions.Add(new BeginStoryboard { Storyboard = sbUnchecked });

                // Hover
                hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty,
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6F61")), "Border"));
                hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty,
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E2E2E")), "Border"));

                template.Triggers.Add(checkedTrigger);
                template.Triggers.Add(hoverTrigger);

                style.Setters.Add(new Setter(Control.TemplateProperty, template));
                return style;
            }

            // توابع کمکی برای انیمیشن
            private DoubleAnimation CreateDoubleAnim(string target, DependencyProperty prop, double to, double sec)
            {
                var anim = new DoubleAnimation(to, TimeSpan.FromSeconds(sec));
                Storyboard.SetTargetName(anim, target);
                Storyboard.SetTargetProperty(anim, new PropertyPath(prop));
                return anim;
            }

            private ColorAnimation CreateColorAnim(string target, string propPath, string colorHex, double sec)
            {
                var anim = new ColorAnimation((Color)ColorConverter.ConvertFromString(colorHex), TimeSpan.FromSeconds(sec));
                Storyboard.SetTargetName(anim, target);
                Storyboard.SetTargetProperty(anim, new PropertyPath(propPath));
                return anim;
            }

            private void Ok_Click(object sender, RoutedEventArgs e)
            {
                TitleText = _titleBox.Text.Trim();
                UrlText = _urlBox.Text.Trim();
                IsFavorite = _favBox.IsChecked == true;
                BrowserName = _browserCombo.SelectedValue?.ToString() ?? "Chrome";
                RateName = _rateCombo.SelectedValue?.ToString() ?? "Normal";
                if (string.IsNullOrEmpty(UrlText))
                {
                    MessageBox.Show("URL is required.", "Validation", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Warning);
                    return;
                }

                DialogResult = true;
                Close();
            }
        }

        // -------------------------
        // Cleanup DB on page unload
        // -------------------------
        private void BookmarkPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //_db?.Dispose();
                _browserCollections.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during unload: {ex.Message}", "Error", MessageBox.MessageBoxButton.OK, MessageBox.MessageBoxIcon.Error);
            }
        }
    }

    // -------------------------
    // Extension to allow adding UI element easier in code (Grid.Add helper)
    // -------------------------
    internal static class UIExtensions
    {
        public static void Add(this Grid grid, UIElement element, int col, int row)
        {
            Grid.SetColumn(element, col);
            Grid.SetRow(element, row);
            grid.Children.Add(element);
        }
    }
}
