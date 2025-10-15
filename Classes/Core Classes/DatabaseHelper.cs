using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using AppLauncher.Classes.MainClasses;
namespace AppLauncher.Classes.Core_Classes
{
    public static class DatabaseHelper
    {
        private static readonly string DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AppLauncher", "uprix-app.db");

        private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        static DatabaseHelper()
        {
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            try
            {
                var dir = Path.GetDirectoryName(DbPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!System.IO.File.Exists(DbPath))
                    SQLiteConnection.CreateFile(DbPath);

                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string createTable = @"
    CREATE TABLE IF NOT EXISTS Apps (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
        Path TEXT NOT NULL UNIQUE,
        Rate INTEGER DEFAULT 0,
        Favorite INTEGER DEFAULT 0
    );";


                using var cmd = new SQLiteCommand(createTable, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing database: " + ex.Message);
            }
        }

        public static List<AppModel> LoadAppsFromDb()
        {
            var apps = new List<AppModel>();
            try
            {
                var conn = DatabaseManager.GetAppDbConnection();

                string sql = @"
SELECT Name, Path, Rate, Favorite
FROM Apps
ORDER BY 
    CASE WHEN Rate IS NULL THEN 0 ELSE Rate END DESC,
    Name COLLATE NOCASE;";
                using var cmd = new SQLiteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    apps.Add(new AppModel
                    {
                        Name = reader.GetString(0),
                        Path = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
                        Rate = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),  // Changed to GetInt64
                        Favorite = reader.GetInt32(3) == 1
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading apps from DB: " + ex.Message);
            }
            return apps;
        }

        public static void SaveAppsToDb(List<AppModel> apps)
        {
            try
            {
                var conn = DatabaseManager.GetAppDbConnection();

                using var tran = conn.BeginTransaction();

                // ✅ Use proper UPSERT with ON CONFLICT
                string upsertSql = @"
            INSERT INTO Apps (Name, Path, Rate, Favorite) 
            VALUES (@Name, @Path, @Rate, @Favorite)
            ON CONFLICT(Path) DO UPDATE SET
                Name = excluded.Name,
                Rate = excluded.Rate,
                Favorite = excluded.Favorite;";

                using var cmd = new SQLiteCommand(upsertSql, conn);
                cmd.Parameters.Add(new SQLiteParameter("@Name"));
                cmd.Parameters.Add(new SQLiteParameter("@Path"));
                cmd.Parameters.Add(new SQLiteParameter("@Rate"));
                cmd.Parameters.Add(new SQLiteParameter("@Favorite"));

                foreach (var app in apps)
                {
                    cmd.Parameters["@Name"].Value = app.Name;
                    cmd.Parameters["@Path"].Value = app.Path;
                    cmd.Parameters["@Rate"].Value = app.Rate;
                    cmd.Parameters["@Favorite"].Value = app.Favorite ? 1 : 0;
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();

                Console.WriteLine($"✅ Successfully saved {apps.Count} apps to database");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving apps to DB: " + ex.Message);
            }
        }
        public static void ResetAll()
        {
            try
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();
                string sql = "UPDATE Apps SET Rate = 0, Favorite = 0;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error resetting all settings: " + ex.Message);
            }
        }
        public static void ResetFavorite()
        {
            try
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();
                string sql = "UPDATE Apps SET Favorite = 0;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error resetting all settings: " + ex.Message);
            }
        }
        public static void ResetRate()
        {
            try
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();
                string sql = "UPDATE Apps SET Rate = 0;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error resetting all settings: " + ex.Message);
            }
        }
        public static void ClearAppsDb()
        {
            try
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();
                using var cmd = new SQLiteCommand("DELETE FROM Apps;", conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error clearing DB: " + ex.Message);
            }
        }
        public static void UpdateRate(AppModel app)
        {
            try
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                string sql = "UPDATE Apps SET Rate=@Rate WHERE Path=@Path;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Rate", (long)app.Rate);  // Changed to long
                cmd.Parameters.AddWithValue("@Path", app.Path);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    string insertSql = @"INSERT INTO Apps (Name, Path, Rate, Favorite) 
                                VALUES (@Name, @Path, @Rate, @Favorite);";
                    using var insertCmd = new SQLiteCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@Name", app.Name);
                    insertCmd.Parameters.AddWithValue("@Path", app.Path);
                    insertCmd.Parameters.AddWithValue("@Rate", (long)app.Rate);
                    insertCmd.Parameters.AddWithValue("@Favorite", app.Favorite ? 1 : 0);
                    insertCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating rate: " + ex.Message);
            }
        }

        public static void UpdateFavorite(AppModel app)
        {
            try
            {
                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();
                string sql = "UPDATE Apps SET Favorite=@Favorite WHERE Path=@Path;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Favorite", app.Favorite ? 1 : 0);
                cmd.Parameters.AddWithValue("@Path", app.Path);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating favorite: " + ex.Message);
            }
        }

    }
}
