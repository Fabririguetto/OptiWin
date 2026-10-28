using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;

namespace OptiWin.Core
{
    public class DiskInfoModel
    {
        public string Name { get; set; } = "";
        public string VolumeLabel { get; set; } = "";
        public string DisplayTitle => string.IsNullOrWhiteSpace(VolumeLabel) ? $"Disco Local ({Name})" : $"Disco {Name} [{VolumeLabel}]";
        public double TotalGB { get; set; }
        public double FreeGB { get; set; }
        public double UsedGB { get; set; }
        public int UsagePercent { get; set; }
        public string DriveType { get; set; } = "Fijo";
        public string FormatDetails => $"{UsedGB} GB usados de {TotalGB} GB ({FreeGB} GB libres)";
        public string UsageBadge => $"{UsagePercent}%";
        public string StatusColor => UsagePercent > 90 ? "#ef4444" : (UsagePercent > 75 ? "#f59e0b" : "#38bdf8");
    }

    public static class HardwareMonitor
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime, 
                                                  out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime, 
                                                  out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

        private static ulong _prevIdleTime;
        private static ulong _prevKernelTime;
        private static ulong _prevUserTime;
        private static bool _firstSample = true;

        private static string? _cachedCpuName;
        private static string? _cachedGpuName;

        public class SystemSnapshot
        {
            public int CpuUsagePercent { get; set; }
            public int GpuUsagePercent { get; set; }
            public string CpuName { get; set; } = "Cargando...";
            public string GpuName { get; set; } = "Cargando...";
            public string OsVersion { get; set; } = "";
            public string MachineName { get; set; } = "";
            public string UptimeFormatted { get; set; } = "";
            public List<DiskInfoModel> AllDisks { get; set; } = new();
        }

        private static ulong FileTimeToUInt64(System.Runtime.InteropServices.ComTypes.FILETIME ft)
        {
            return ((ulong)ft.dwHighDateTime << 32) | (uint)ft.dwLowDateTime;
        }

        public static int GetCurrentCpuUsage()
        {
            if (!GetSystemTimes(out var idleFt, out var kernelFt, out var userFt))
                return 0;

            ulong idleTime = FileTimeToUInt64(idleFt);
            ulong kernelTime = FileTimeToUInt64(kernelFt);
            ulong userTime = FileTimeToUInt64(userFt);

            if (_firstSample)
            {
                _prevIdleTime = idleTime;
                _prevKernelTime = kernelTime;
                _prevUserTime = userTime;
                _firstSample = false;
                return 0;
            }

            ulong usrDiff = userTime - _prevUserTime;
            ulong kerDiff = kernelTime - _prevKernelTime;
            ulong idlDiff = idleTime - _prevIdleTime;

            _prevIdleTime = idleTime;
            _prevKernelTime = kernelTime;
            _prevUserTime = userTime;

            ulong totalSys = kerDiff + usrDiff;
            if (totalSys == 0) return 0;

            ulong totalDiff = totalSys - idlDiff;
            double percent = (totalDiff * 100.0) / totalSys;

            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            return (int)Math.Round(percent);
        }

        public static string GetCpuName()
        {
            if (!string.IsNullOrEmpty(_cachedCpuName)) return _cachedCpuName;

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                var name = key?.GetValue("ProcessorNameString")?.ToString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _cachedCpuName = name.Trim();
                    return _cachedCpuName;
                }
            }
            catch { }

            _cachedCpuName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Procesador x64";
            return _cachedCpuName;
        }

        public static string GetGpuName()
        {
            if (!string.IsNullOrEmpty(_cachedGpuName)) return _cachedGpuName;

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
                string primaryGpu = "";
                ulong maxRam = 0;

                foreach (ManagementObject mo in searcher.Get())
                {
                    var name = mo["Name"]?.ToString() ?? "";
                    ulong.TryParse(mo["AdapterRAM"]?.ToString() ?? "0", out ulong ram);

                    // Preferir la placa de video dedicada (mayor VRAM)
                    if (ram >= maxRam && !string.IsNullOrWhiteSpace(name))
                    {
                        maxRam = ram;
                        primaryGpu = name.Trim();
                    }
                }

                if (!string.IsNullOrEmpty(primaryGpu))
                {
                    _cachedGpuName = primaryGpu;
                    return _cachedGpuName;
                }
            }
            catch { }

            _cachedGpuName = "Controlador Gráfico";
            return _cachedGpuName;
        }

        public static List<DiskInfoModel> GetAllDisks()
        {
            var list = new List<DiskInfoModel>();
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
                    {
                        double totalGB = Math.Round(drive.TotalSize / (1024.0 * 1024.0 * 1024.0), 1);
                        double freeGB = Math.Round(drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0), 1);
                        double usedGB = Math.Round(totalGB - freeGB, 1);
                        int usagePercent = totalGB > 0 ? (int)Math.Round((usedGB / totalGB) * 100) : 0;

                        list.Add(new DiskInfoModel
                        {
                            Name = drive.Name,
                            VolumeLabel = drive.VolumeLabel,
                            TotalGB = totalGB,
                            FreeGB = freeGB,
                            UsedGB = usedGB,
                            UsagePercent = usagePercent,
                            DriveType = drive.DriveType == DriveType.Removable ? "Extraíble (USB)" : "Unidad Fija (SSD/HDD)"
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        public static SystemSnapshot GetSnapshot()
        {
            int cpu = GetCurrentCpuUsage();
            int gpu = GpuMonitor.GetGpuUsage();
            string uptime = GetFormattedUptime();
            var disks = GetAllDisks();

            return new SystemSnapshot
            {
                CpuUsagePercent = cpu,
                GpuUsagePercent = gpu,
                CpuName = GetCpuName(),
                GpuName = GetGpuName(),
                OsVersion = GetOsFriendlyName(),
                MachineName = Environment.MachineName,
                UptimeFormatted = uptime,
                AllDisks = disks
            };
        }

        private static string GetFormattedUptime()
        {
            var ts = TimeSpan.FromMilliseconds(Environment.TickCount64);
            if (ts.Days > 0)
                return $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";
            return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        }

        private static string GetOsFriendlyName()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                var prod = key?.GetValue("ProductName")?.ToString();
                var build = key?.GetValue("CurrentBuild")?.ToString();
                if (!string.IsNullOrEmpty(prod))
                {
                    if (int.TryParse(build, out int b) && b >= 22000)
                    {
                        prod = prod.Replace("Windows 10", "Windows 11");
                    }
                    return $"{prod} (Build {build})";
                }
            }
            catch { }

            return Environment.OSVersion.VersionString;
        }
    }
}
