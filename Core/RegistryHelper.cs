using Microsoft.Win32;
using System;

namespace OptiWin.Core
{
    public static class RegistryHelper
    {
        public static bool SetDword(RegistryHive hive, string subKeyPath, string valueName, int value)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.CreateSubKey(subKeyPath, writable: true);
                if (key == null) return false;

                key.SetValue(valueName, value, RegistryValueKind.DWord);
                AppLogger.Info($"Registro actualizado: {hive}\\{subKeyPath} -> {valueName} = {value}");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Error escribiendo DWORD en {hive}\\{subKeyPath}\\{valueName}", ex);
                return false;
            }
        }

        public static bool SetString(RegistryHive hive, string subKeyPath, string valueName, string value)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.CreateSubKey(subKeyPath, writable: true);
                if (key == null) return false;

                key.SetValue(valueName, value, RegistryValueKind.String);
                AppLogger.Info($"Registro actualizado: {hive}\\{subKeyPath} -> {valueName} = \"{value}\"");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Error escribiendo String en {hive}\\{subKeyPath}\\{valueName}", ex);
                return false;
            }
        }

        public static bool DeleteValue(RegistryHive hive, string subKeyPath, string valueName)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(subKeyPath, writable: true);
                if (key == null) return true;

                if (key.GetValue(valueName) != null)
                {
                    key.DeleteValue(valueName, false);
                    AppLogger.Info($"Valor de registro eliminado: {hive}\\{subKeyPath}\\{valueName}");
                }
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Error eliminando valor en {hive}\\{subKeyPath}\\{valueName}", ex);
                return false;
            }
        }

        public static bool CheckDword(RegistryHive hive, string subKeyPath, string valueName, int expectedValue)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(subKeyPath, writable: false);
                if (key == null) return false;

                var val = key.GetValue(valueName);
                if (val is int intVal)
                {
                    return intVal == expectedValue;
                }
                if (val is long longVal)
                {
                    return longVal == expectedValue;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool CheckString(RegistryHive hive, string subKeyPath, string valueName, string expectedValue)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(subKeyPath, writable: false);
                if (key == null) return false;

                var val = key.GetValue(valueName)?.ToString();
                return string.Equals(val, expectedValue, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
