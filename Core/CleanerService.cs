using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OptiWin.Core
{
    public static class CleanerService
    {
        [DllImport("shell32.dll")]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

        private const uint SHERB_NOCONFIRMATION = 0x00000001;
        private const uint SHERB_NOPROGRESSUI = 0x00000002;
        private const uint SHERB_NOSOUND = 0x00000004;

        public class CleanTarget
        {
            public string Name { get; set; } = "";
            public string Path { get; set; } = "";
            public long BytesFound { get; set; }
            public int FilesCount { get; set; }
        }

        public static List<string> GetDefaultTargets()
        {
            var targets = new List<string>
            {
                Path.GetTempPath(),
                Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Temp"),
                Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Prefetch"),
                Environment.ExpandEnvironmentVariables(@"%LocalAppData%\CrashDumps"),
                Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Google\Chrome\User Data\Default\Cache"),
                Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Microsoft\Edge\User Data\Default\Cache")
            };
            return targets;
        }

        public static async Task<long> ScanJunkFilesAsync()
        {
            return await Task.Run(() =>
            {
                long totalBytes = 0;
                int totalFiles = 0;

                foreach (var dir in GetDefaultTargets())
                {
                    if (!Directory.Exists(dir)) continue;

                    try
                    {
                        var di = new DirectoryInfo(dir);
                        foreach (var file in di.EnumerateFiles("*", SearchOption.AllDirectories))
                        {
                            try
                            {
                                totalBytes += file.Length;
                                totalFiles++;
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                AppLogger.Info($"Escaneo completado: {totalFiles} archivos encontrados ({Math.Round(totalBytes / (1024.0 * 1024.0), 1)} MB).");
                return totalBytes;
            });
        }

        public static async Task<long> CleanJunkFilesAsync()
        {
            return await Task.Run(() =>
            {
                long bytesDeleted = 0;
                int filesDeleted = 0;

                foreach (var dir in GetDefaultTargets())
                {
                    if (!Directory.Exists(dir)) continue;

                    try
                    {
                        var di = new DirectoryInfo(dir);
                        foreach (var file in di.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                        {
                            try
                            {
                                long len = file.Length;
                                file.Delete();
                                bytesDeleted += len;
                                filesDeleted++;
                            }
                            catch
                            {
                                // Archivos en uso por procesos activos
                            }
                        }

                        foreach (var subDir in di.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                        {
                            try
                            {
                                subDir.Delete(true);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                // Vaciar Papelera de Reciclaje
                try
                {
                    SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);
                    AppLogger.Info("Papelera de reciclaje vaciada.");
                }
                catch { }

                double mb = Math.Round(bytesDeleted / (1024.0 * 1024.0), 1);
                AppLogger.Success($"Limpieza finalizada: {filesDeleted} archivos eliminados. Espacio recuperado: {mb} MB.");
                return bytesDeleted;
            });
        }
    }
}
