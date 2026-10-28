using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace OptiWin.Core
{
    public class StartupItem
    {
        public string Name { get; set; } = "";
        public string Command { get; set; } = "";
        public string Location { get; set; } = "";
        public RegistryHive Hive { get; set; }
    }

    public static class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static List<StartupItem> GetStartupItems()
        {
            var list = new List<StartupItem>();

            // 1. Current User
            ReadFromHive(RegistryHive.CurrentUser, "Usuario Actual (HKCU)", list);

            // 2. Local Machine
            ReadFromHive(RegistryHive.LocalMachine, "Sistema (HKLM)", list);

            return list;
        }

        private static void ReadFromHive(RegistryHive hive, string locationLabel, List<StartupItem> list)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(RunKeyPath, writable: false);
                if (key == null) return;

                foreach (var valueName in key.GetValueNames())
                {
                    var cmd = key.GetValue(valueName)?.ToString() ?? "";
                    list.Add(new StartupItem
                    {
                        Name = valueName,
                        Command = cmd,
                        Location = locationLabel,
                        Hive = hive
                    });
                }
            }
            catch { }
        }

        public static bool RemoveStartupItem(StartupItem item)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(item.Hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(RunKeyPath, writable: true);
                if (key == null) return false;

                key.DeleteValue(item.Name, false);
                AppLogger.Success($"Elemento de inicio eliminado: {item.Name} ({item.Location})");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Error eliminando elemento de inicio {item.Name}", ex);
                return false;
            }
        }
    }
}
