// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using SteamKit2;

namespace DepotDownloader
{
    static class DepotKeysStore
    {
        private static Dictionary<uint, byte[]> depotKeys = new();
        private static string currentFilePath;

        public static void LoadFromFile(string filename)
        {
            currentFilePath = filename;
            depotKeys.Clear();

            if (!File.Exists(filename))
            {
                return;
            }

            try
            {
                var kv = KeyValue.LoadAsText(filename);
                if (kv == null || kv.Name != "depots")
                {
                    return;
                }

                foreach (var depotKv in kv.Children)
                {
                    if (uint.TryParse(depotKv.Name, out var depotId))
                    {
                        var decryptionKeyStr = depotKv["DecryptionKey"].Value;
                        if (!string.IsNullOrEmpty(decryptionKeyStr))
                        {
                            var keyBytes = Util.DecodeHexString(decryptionKeyStr);
                            if (keyBytes != null)
                            {
                                depotKeys[depotId] = keyBytes;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load depot keys: {0}", ex.Message);
            }
        }

        public static void Save()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                return;
            }

            try
            {
                var kv = new KeyValue("depots");

                foreach (var pair in depotKeys)
                {
                    var depotKv = new KeyValue(pair.Key.ToString());
                    depotKv["DecryptionKey"] = new KeyValue("DecryptionKey", Convert.ToHexString(pair.Value).ToLowerInvariant());
                    kv.Children.Add(depotKv);
                }

                kv.SaveToFile(currentFilePath, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save depot keys: {0}", ex.Message);
            }
        }

        public static bool TryGetDepotKey(uint depotId, out byte[] depotKey)
        {
            return depotKeys.TryGetValue(depotId, out depotKey);
        }

        public static void SetDepotKey(uint depotId, byte[] depotKey)
        {
            depotKeys[depotId] = depotKey;
        }
    }
}