using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OptiWin.Core
{
    public static class GpuMonitor
    {
        private static List<PerformanceCounter>? _counters;
        private static DateTime _lastRefresh = DateTime.MinValue;
        private static readonly object _lock = new();

        public static int GetGpuUsage()
        {
            lock (_lock)
            {
                try
                {
                    // Refrescar instancias cada 15 segundos para detectar nuevos procesos/juegos
                    if (_counters == null || (DateTime.Now - _lastRefresh).TotalSeconds > 15)
                    {
                        DisposeCounters();
                        _counters = new List<PerformanceCounter>();

                        try
                        {
                            var category = new PerformanceCounterCategory("GPU Engine");
                            var instances = category.GetInstanceNames();
                            var target3D = instances.Where(i => i.EndsWith("engtype_3D", StringComparison.OrdinalIgnoreCase)).ToList();

                            foreach (var name in target3D)
                            {
                                try
                                {
                                    var c = new PerformanceCounter("GPU Engine", "Utilization Percentage", name, readOnly: true);
                                    c.NextValue(); // Primera lectura para inicializar el contador
                                    _counters.Add(c);
                                }
                                catch { }
                            }
                        }
                        catch { }

                        _lastRefresh = DateTime.Now;
                        return 0;
                    }

                    if (_counters.Count == 0) return 0;

                    float totalUtilization = 0;
                    for (int i = _counters.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            totalUtilization += _counters[i].NextValue();
                        }
                        catch
                        {
                            // Si el proceso cerró, quitar el contador
                            try { _counters[i].Dispose(); } catch { }
                            _counters.RemoveAt(i);
                        }
                    }

                    int finalVal = (int)Math.Round(totalUtilization);
                    if (finalVal > 100) finalVal = 100;
                    if (finalVal < 0) finalVal = 0;

                    return finalVal;
                }
                catch
                {
                    return 0;
                }
            }
        }

        private static void DisposeCounters()
        {
            if (_counters != null)
            {
                foreach (var c in _counters)
                {
                    try { c.Dispose(); } catch { }
                }
                _counters.Clear();
            }
        }
    }
}
