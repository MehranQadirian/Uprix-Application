using LiteDB;
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Windows;

namespace AppLauncher.Classes.Core_Classes
{
    public static class DatabaseManager
    {
        private static readonly string AppDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppLauncher", "uprix-app.db");
        private static readonly string TaskDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TaskManager", "taskmanager.db");
        private static readonly string BookmarkDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppLauncher", "BookmarkManager", "uprix.db");

        private static LiteDatabase _taskDb;
        private static LiteDatabase _bookmarkDb;

        private static SQLiteConnection _appDbConnection;

        // Mutex to prevent multiple instances
        private static Mutex _appMutex = new Mutex(true, "AppLauncherUniqueMutex");

        static DatabaseManager()
        {
            // Prevent multiple instances
            if (!_appMutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Another instance of the application is already running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        // Get Task LiteDB with retry and dispose check
        public static LiteDatabase GetTaskDatabase()
        {
            if (_taskDb == null || IsDatabaseDisposed(_taskDb))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TaskDbPath));
                _taskDb = OpenLiteDbWithRetry(TaskDbPath);
            }
            return _taskDb;
        }

        // Get Bookmark LiteDB with retry and dispose check
        public static LiteDatabase GetBookmarkDatabase()
        {
            if (_bookmarkDb == null || IsDatabaseDisposed(_bookmarkDb))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(BookmarkDbPath));
                _bookmarkDb = OpenLiteDbWithRetry(BookmarkDbPath);
            }
            return _bookmarkDb;
        }

        // Check if database is disposed
        private static bool IsDatabaseDisposed(LiteDatabase db)
        {
            try
            {
                // Try a simple operation to check if db is usable
                db.GetCollectionNames();
                return false;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
            catch
            {
                // Other errors might indicate issues, but assume disposed for safety
                return true;
            }
        }

        // Open LiteDB with Shared mode and retry
        private static LiteDatabase OpenLiteDbWithRetry(string path, int retries = 3, int delayMs = 1000)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    return new LiteDatabase($"Filename={path};Mode=Shared");
                }
                catch (Exception ex)
                {
                    if (i == retries - 1)
                    {
                        MessageBox.Show($"Failed to open database after {retries} attempts: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        throw;
                    }
                    Thread.Sleep(delayMs); // Wait and retry
                }
            }
            return null;
        }

        // Get SQLite connection for Apps
        public static SQLiteConnection GetAppDbConnection()
        {
            if (_appDbConnection == null || _appDbConnection.State == System.Data.ConnectionState.Closed)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(AppDbPath));
                if (!File.Exists(AppDbPath))
                    SQLiteConnection.CreateFile(AppDbPath);
                _appDbConnection = new SQLiteConnection($"Data Source={AppDbPath};Version=3;");
                _appDbConnection.Open();
            }
            return _appDbConnection;
        }

        // Close all databases on app exit
        public static void CloseAll()
        {
            try
            {
                _taskDb?.Dispose();
                _bookmarkDb?.Dispose();
                _appDbConnection?.Close();
                _appDbConnection?.Dispose();
                _appMutex?.ReleaseMutex();
            }
            catch (Exception ex)
            {
                // Log error but don't crash app
                Console.WriteLine($"Error closing databases: {ex.Message}");
            }
            finally
            {
                _taskDb = null;
                _bookmarkDb = null;
                _appDbConnection = null;
            }
        }
    }
}