using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppLauncher.Classes.Core_Classes;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using File = System.IO.File;

namespace AppLauncher.Classes.MainClasses
{
    public class AppLoader
    {
        private readonly List<AppModel> _allApps;

        public AppLoader(List<AppModel> allApps)
        {
            _allApps = allApps;
        }

        public async Task LoadAsync(IProgress<string> progress)
        {
            // First load from database
            var cachedApps = DatabaseHelper.LoadAppsFromDb();
            var dbLookup = cachedApps.ToDictionary(a => a.Path, StringComparer.OrdinalIgnoreCase);

            string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string commonStart = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

            var discoveredApps = new List<AppModel>();

            await EnumerateShortcutsAsync(startMenu, progress, discoveredApps);
            await EnumerateShortcutsAsync(commonStart, progress, discoveredApps);
            EnumerateRegistryApps(progress, discoveredApps);

            // Merge: use DB data if exists, otherwise use discovered
            foreach (var app in discoveredApps)
            {
                if (dbLookup.TryGetValue(app.Path, out var cachedApp))
                {
                    // Use cached Rate and Favorite
                    app.Rate = cachedApp.Rate;
                    app.Favorite = cachedApp.Favorite;
                }
                _allApps.Add(app);
            }

            // Remove duplicates and sort
            var unique = _allApps
                .GroupBy(a => a.Path, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(a => a.Rate).First())
                .OrderByDescending(a => a.Rate)
                .ThenBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            _allApps.Clear();
            _allApps.AddRange(unique);

            // Save to database
            DatabaseHelper.SaveAppsToDb(_allApps);
        }

        private void EnumerateRegistryApps(IProgress<string> progress, List<AppModel> targetList)
        {
            string[] registryKeys =
            {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

            foreach (var rootKey in new[] { Registry.LocalMachine, Registry.CurrentUser })
            {
                foreach (var keyPath in registryKeys)
                {
                    using (var key = rootKey.OpenSubKey(keyPath))
                    {
                        if (key == null) continue;

                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                try
                                {
                                    string name = subKey.GetValue("DisplayName") as string;
                                    string exePath = subKey.GetValue("DisplayIcon") as string;
                                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(exePath))
                                    {
                                        exePath = exePath.Trim('"');
                                        if (File.Exists(exePath))
                                        {
                                            progress.Report($"Loading {name}...");
                                            targetList.Add(new AppModel
                                            {
                                                Name = name,
                                                Path = exePath,
                                                Rate = 0,
                                                Favorite = false
                                            });
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        private async Task EnumerateShortcutsAsync(string root, IProgress<string> progress, List<AppModel> targetList)
        {
            try
            {
                foreach (var shortcut in Directory.GetFiles(root, "*.lnk", SearchOption.AllDirectories))
                {
                    progress.Report($"Loading {Path.GetFileNameWithoutExtension(shortcut)}...");
                    await Task.Run(() => TryAddShortcut(shortcut, targetList));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EnumerateShortcuts for {root}: {ex.Message}");
            }
        }

        private void TryAddShortcut(string path, List<AppModel> targetList)
        {
            try
            {
                var shell = new WshShell();
                var lnk = (IWshShortcut)shell.CreateShortcut(path);

                if (!string.IsNullOrWhiteSpace(lnk.TargetPath) && File.Exists(lnk.TargetPath))
                {
                    if (Path.GetExtension(lnk.TargetPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        targetList.Add(new AppModel
                        {
                            Name = Path.GetFileNameWithoutExtension(path),
                            Path = lnk.TargetPath,
                            Rate = 0,
                            Favorite = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TryAddShortcut for {path}: {ex.Message}");
            }
        }
    }
}
