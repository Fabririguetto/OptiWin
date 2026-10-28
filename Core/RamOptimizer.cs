using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OptiWin.Core
{
    public static class RamOptimizer
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        public class RamInfo
        {
            public double TotalGB { get; set; }
            public double UsedGB { get; set; }
            public double FreeGB { get; set; }
            public int UsagePercent { get; set; }
        }

        public static RamInfo GetCurrentRamInfo()
        {
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                double total = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                double free = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                double used = total - free;

                return new RamInfo
                {
                    TotalGB = Math.Round(total, 1),
                    FreeGB = Math.Round(free, 1),
                    UsedGB = Math.Round(used, 1),
                    UsagePercent = (int)memStatus.dwMemoryLoad
                };
            }

            return new RamInfo { TotalGB = 0, UsedGB = 0, FreeGB = 0, UsagePercent = 0 };
        }

        public static double FreeMemory()
        {
            var before = GetCurrentRamInfo();
            int freedProcesses = 0;

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (!process.HasExited)
                    {
                        EmptyWorkingSet(process.Handle);
                        freedProcesses++;
                    }
                }
                catch
                {
                    // Algunos procesos del sistema están protegidos por el kernel
                }
            }

            // Forzar recolección de basura del propio optimizador
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var after = GetCurrentRamInfo();
            double recoveredMB = (after.FreeGB - before.FreeGB) * 1024.0;
            if (recoveredMB < 0) recoveredMB = 0;

            AppLogger.Success($"Optimización de RAM: {freedProcesses} procesos liberados. Memoria recuperada: {Math.Round(recoveredMB, 0)} MB.");
            return Math.Round(recoveredMB, 0);
        }
    }
}
