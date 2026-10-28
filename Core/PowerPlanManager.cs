using System;
using System.Diagnostics;

namespace OptiWin.Core
{
    public static class PowerPlanManager
    {
        public const string UltimatePerformanceGuid = "e9a42b02-d5df-448d-aa00-03f14749eb61";
        public const string HighPerformanceGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
        public const string BalancedGuid = "381b4222-f694-41f0-9685-ff5bb260df2e";

        public static bool EnableUltimatePerformance()
        {
            try
            {
                // 1. Desbloquear el plan si no existe
                RunPowerCfg($"-duplicatescheme {UltimatePerformanceGuid}");
                
                // 2. Activarlo
                RunPowerCfg($"/setactive {UltimatePerformanceGuid}");

                AppLogger.Success("Plan de Energía 'Máximo Rendimiento' (Ultimate Performance) activado con éxito.");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Error activando plan Ultimate Performance", ex);
                return false;
            }
        }

        public static bool SetBalancedPlan()
        {
            try
            {
                RunPowerCfg($"/setactive {BalancedGuid}");
                AppLogger.Info("Plan de Energía 'Equilibrado' restaurado.");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Error restaurando plan Equilibrado", ex);
                return false;
            }
        }

        private static void RunPowerCfg(string args)
        {
            var psi = new ProcessStartInfo("powercfg.exe", args)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(3000);
        }
    }
}
