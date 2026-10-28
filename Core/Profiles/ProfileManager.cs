using OptiWin.Core.Tweaks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OptiWin.Core.Profiles
{
    public static class ProfileManager
    {
        public static async Task ApplyGamingProfileAsync(List<TweakModel> tweaks)
        {
            await Task.Run(() =>
            {
                AppLogger.Info("=== APLICANDO PERFIL GAMING DE ALTO RENDIMIENTO ===");

                // 1. Plan de Energía Ultimate Performance
                PowerPlanManager.EnableUltimatePerformance();

                // 2. DNS Cloudflare (1.1.1.1) para menor ping en juegos
                DnsManager.SetDns(DnsManager.DnsProvider.Cloudflare);

                // 3. Aplicar tweaks de gaming
                string[] targetIds = { "perf_network_throttle", "perf_system_responsiveness", "perf_gpu_priority_games", "perf_gamedvr_disable", "perf_auto_end_tasks", "perf_menu_delay", "priv_diagtrack", "sys_sticky_keys" };
                foreach (var id in targetIds)
                {
                    var tw = tweaks.FirstOrDefault(t => t.Id == id);
                    if (tw != null)
                    {
                        tw.ApplyAction?.Invoke(true);
                        tw.RefreshStatus();
                    }
                }

                // 4. Liberar memoria RAM
                RamOptimizer.FreeMemory();

                AppLogger.Success("=== ¡PERFIL GAMING ACTIVADO AL 100%! ===");
            });
        }

        public static async Task ApplyWorkProfileAsync(List<TweakModel> tweaks)
        {
            await Task.Run(() =>
            {
                AppLogger.Info("=== APLICANDO PERFIL TRABAJO Y PRODUCTIVIDAD ===");

                // 1. Plan Equilibrado
                PowerPlanManager.SetBalancedPlan();

                // 2. DNS Google (8.8.8.8)
                DnsManager.SetDns(DnsManager.DnsProvider.Google);

                // 3. Tweaks de productividad
                string[] targetIds = { "priv_bing_search", "sys_long_paths", "priv_diagtrack", "priv_browser_telemetry", "priv_error_reporting", "perf_auto_end_tasks" };
                foreach (var id in targetIds)
                {
                    var tw = tweaks.FirstOrDefault(t => t.Id == id);
                    if (tw != null)
                    {
                        tw.ApplyAction?.Invoke(true);
                        tw.RefreshStatus();
                    }
                }

                AppLogger.Success("=== ¡PERFIL TRABAJO ACTIVADO! ===");
            });
        }

        public static async Task ApplyCreatorProfileAsync(List<TweakModel> tweaks)
        {
            await Task.Run(() =>
            {
                AppLogger.Info("=== APLICANDO PERFIL CREACIÓN DE CONTENIDO / RENDER ===");

                // 1. Máximo rendimiento para rendering
                PowerPlanManager.EnableUltimatePerformance();

                // 2. Tweaks de rendimiento y GPU
                string[] targetIds = { "perf_gpu_priority_games", "perf_system_responsiveness", "sys_long_paths", "perf_auto_end_tasks" };
                foreach (var id in targetIds)
                {
                    var tw = tweaks.FirstOrDefault(t => t.Id == id);
                    if (tw != null)
                    {
                        tw.ApplyAction?.Invoke(true);
                        tw.RefreshStatus();
                    }
                }

                // 3. Vaciado masivo de RAM para liberar buffer de render
                RamOptimizer.FreeMemory();

                AppLogger.Success("=== ¡PERFIL CREACIÓN DE CONTENIDO ACTIVADO! ===");
            });
        }

        public static async Task RestoreDefaultsAsync(List<TweakModel> tweaks)
        {
            await Task.Run(() =>
            {
                AppLogger.Info("=== RESTAURANDO CONFIGURACIÓN POR DEFECTO DE WINDOWS ===");

                // 1. Plan equilibrado
                PowerPlanManager.SetBalancedPlan();

                // 2. DNS Automático
                DnsManager.SetDns(DnsManager.DnsProvider.DefaultDHCP);

                // 3. Desactivar todos los tweaks
                foreach (var tw in tweaks)
                {
                    tw.ApplyAction?.Invoke(false);
                    tw.RefreshStatus();
                }

                AppLogger.Success("=== ¡VALORES ORIGINALES DE WINDOWS RESTAURADOS! ===");
            });
        }
    }
}
