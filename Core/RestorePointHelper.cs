using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OptiWin.Core
{
    public static class RestorePointHelper
    {
        public static async Task<bool> CreateRestorePointAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    AppLogger.Info("Creando Punto de Restauración del Sistema...");

                    // Habilitar restauración en disco C: si estuviera desactivada
                    var enableScript = "Enable-ComputerRestore -Drive 'C:\\' -ErrorAction SilentlyContinue; " +
                                       $"Checkpoint-Computer -Description 'OptiWin_Seguridad_{DateTime.Now:yyyyMMdd_HHmm}' -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop";

                    var psi = new ProcessStartInfo("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -Command \"{enableScript}\"")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true
                    };

                    using var p = Process.Start(psi);
                    string err = p?.StandardError.ReadToEnd() ?? "";
                    p?.WaitForExit(30000);

                    if (p?.ExitCode == 0)
                    {
                        AppLogger.Success("¡Punto de restauración del sistema creado con éxito!");
                        return true;
                    }
                    else
                    {
                        AppLogger.Warning($"No se pudo crear el punto de restauración (puede requerir habilitar Protección del Sistema en Windows): {err}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Error("Error creando punto de restauración", ex);
                    return false;
                }
            });
        }
    }
}
