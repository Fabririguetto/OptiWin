using Microsoft.Win32;
using System;
using System.ServiceProcess;

namespace OptiWin.Core
{
    public static class ServiceHelper
    {
        public enum ServiceStartMode
        {
            Boot = 0,
            System = 1,
            Automatic = 2,
            Manual = 3,
            Disabled = 4
        }

        public static bool IsServiceDisabled(string serviceName)
        {
            try
            {
                var startVal = RegistryHelper.CheckDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Services\{serviceName}", "Start", (int)ServiceStartMode.Disabled);
                return startVal;
            }
            catch
            {
                return false;
            }
        }

        public static bool StopAndDisableService(string serviceName)
        {
            try
            {
                // 1. Detener el servicio si está corriendo
                using (var sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                        AppLogger.Info($"Servicio detenido: {serviceName}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"No se pudo detener el servicio {serviceName} inmediatamente (puede que ya esté detenido): {ex.Message}");
            }

            // 2. Establecer inicio en Deshabilitado (4)
            var regOk = RegistryHelper.SetDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Services\{serviceName}", "Start", (int)ServiceStartMode.Disabled);
            if (regOk)
            {
                AppLogger.Success($"Servicio deshabilitado exitosamente: {serviceName}");
            }
            return regOk;
        }

        public static bool EnableAndStartService(string serviceName, ServiceStartMode mode = ServiceStartMode.Automatic)
        {
            try
            {
                // 1. Establecer en Manual (3) o Automático (2)
                RegistryHelper.SetDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Services\{serviceName}", "Start", (int)mode);

                // 2. Intentar iniciarlo
                using (var sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                    }
                }
                AppLogger.Success($"Servicio restaurado e iniciado: {serviceName}");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"Servicio reconfigurado, pero no pudo arrancarse inmediatamente: {serviceName} ({ex.Message})");
                return true;
            }
        }
    }
}
