// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;

namespace DepotDownloader
{
    /// <summary>
    /// Stores app names locally so they can be used in no-login mode.
    /// </summary>
    static class AppNameStore
    {
        private static readonly Dictionary<uint, string> AppNames = new();
        private static string filePath;

        public static void LoadFromFile(string path)
        {
            filePath = path;
            AppNames.Clear();

            if (!File.Exists(path))
                return;

            try
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    var parts = line.Split('\t', 2);
                    if (parts.Length == 2 && uint.TryParse(parts[0], out var appId))
                    {
                        AppNames[appId] = parts[1];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load app names cache: {ex.Message}");
            }
        }

        public static void Save()
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                var lines = new List<string>();
                foreach (var kvp in AppNames)
                {
                    lines.Add($"{kvp.Key}\t{kvp.Value}");
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to save app names cache: {ex.Message}");
            }
        }

        public static bool TryGetAppName(uint appId, out string name)
        {
            return AppNames.TryGetValue(appId, out name);
        }

        public static void SetAppName(uint appId, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                AppNames[appId] = name;
            }
        }
    }
}
