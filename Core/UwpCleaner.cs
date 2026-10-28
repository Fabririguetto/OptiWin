using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OptiWin.Core
{
    public class BloatwareApp
    {
        public string DisplayName { get; set; } = "";
        public string PackageName { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public static class UwpCleaner
    {
        public static List<BloatwareApp> GetKnownBloatware()
        {
            return new List<BloatwareApp>
            {
                new BloatwareApp { DisplayName = "Cortana", PackageName = "*Microsoft.549981C3F5F10*", Description = "Asistente de voz en desuso de Windows" },
                new BloatwareApp { DisplayName = "Bing Noticias", PackageName = "*Microsoft.BingNews*", Description = "App de noticias y feeds en segundo plano" },
                new BloatwareApp { DisplayName = "Bing El Tiempo", PackageName = "*Microsoft.BingWeather*", Description = "Previsión meteorológica y consumo de red" },
                new BloatwareApp { DisplayName = "Centro de Comentarios", PackageName = "*Microsoft.WindowsFeedbackHub*", Description = "Recolección de encuestas y opiniones" },
                new BloatwareApp { DisplayName = "Sugerencias y Consejos", PackageName = "*Microsoft.Getstarted*", Description = "Tutoriales y popups promocionales de Windows" },
                new BloatwareApp { DisplayName = "Obtener Ayuda", PackageName = "*Microsoft.GetHelp*", Description = "Asistente web de soporte técnico de Microsoft" },
                new BloatwareApp { DisplayName = "Solitario & Juegos Casuales", PackageName = "*Microsoft.MicrosoftSolitaireCollection*", Description = "Juego preinstalado con publicidad" },
                new BloatwareApp { DisplayName = "Mapas de Windows", PackageName = "*Microsoft.WindowsMaps*", Description = "Servicio de cartografía y telemetría de ubicación" }
            };
        }

        public static async Task<bool> UninstallAppAsync(BloatwareApp app)
        {
            return await Task.Run(() =>
            {
                try
                {
                    AppLogger.Info($"Desinstalando bloatware: {app.DisplayName}...");
                    var script = $"Get-AppxPackage -Name '{app.PackageName}' -AllUsers | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue";
                    
                    var psi = new ProcessStartInfo("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    using var p = Process.Start(psi);
                    p?.WaitForExit(10000);

                    AppLogger.Success($"App desinstalada correctamente: {app.DisplayName}");
                    return true;
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"Error desinstalando {app.DisplayName}", ex);
                    return false;
                }
            });
        }
    }
}
