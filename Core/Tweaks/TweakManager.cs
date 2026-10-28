using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OptiWin.Core.Tweaks
{
    public static class TweakManager
    {
        public static List<TweakModel> GetAllTweaks()
        {
            var tweaks = new List<TweakModel>
            {
                // ==================== RENDIMIENTO & GAMING ====================
                new TweakModel
                {
                    Id = "perf_network_throttle",
                    Title = "Desactivar estrangulamiento de red (Network Throttling)",
                    Description = "Elimina la limitación artificial de Windows a paquetes de red de juegos y descargas.",
                    DetailedExplanation = "Windows reserva hasta un 20% del ancho de banda y prioriza tráfico multimedia aplicando un algoritmo de estrangulamiento a paquetes no multimedia.\n\nAl desactivarlo, la tarjeta de red procesa paquetes al 100% sin esperas ni demoras, reduciendo la fluctuación (jitter) y el ping en partidas multijugador.",
                    RegistryOrServiceDetails = "Clave: HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\nValor: NetworkThrottlingIndex = 0xFFFFFFFF (4294967295)",
                    Category = TweakCategory.Rendimiento,
                    CheckAction = () => RegistryHelper.CheckDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF)),
                    ApplyAction = (enable) =>
                    {
                        if (enable)
                        {
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF));
                            AppLogger.Success("Network Throttling desactivado (máximo rendimiento de red).");
                        }
                        else
                        {
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 10);
                            AppLogger.Info("Network Throttling restaurado a 10.");
                        }
                    }
                },
                new TweakModel
                {
                    Id = "perf_system_responsiveness",
                    Title = "Prioridad máxima de CPU para juegos (System Responsiveness)",
                    Description = "Asigna el 100% de CPU a la app en primer plano en lugar de reservar el 20% para tareas secundarias.",
                    DetailedExplanation = "Por defecto, el programador de Windows reserva el 20% de los ciclos de CPU para tareas de fondo del sistema.\n\nAl cambiar este valor a 0, se le otorga la totalidad de ciclos a la ventana activa o juego, evitando caídas repentinas de FPS cuando el sistema realiza comprobaciones en segundo plano.",
                    RegistryOrServiceDetails = "Clave: HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\nValor: SystemResponsiveness = 0",
                    Category = TweakCategory.Rendimiento,
                    CheckAction = () => RegistryHelper.CheckDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0),
                    ApplyAction = (enable) =>
                    {
                        int val = enable ? 0 : 20;
                        RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", val);
                        AppLogger.Success($"System Responsiveness ajustado a {(enable ? "0% (máximo rendimiento)" : "20% (por defecto)")}.");
                    }
                },
                new TweakModel
                {
                    Id = "perf_gpu_priority_games",
                    Title = "Prioridad alta de GPU para juegos",
                    Description = "Configura el programador de GPU de Windows en prioridad alta (8) para el proceso de juegos.",
                    DetailedExplanation = "Modifica los parámetros del Multimedia Scheduler de Windows para forzar que el controlador gráfico dé prioridad absoluta de renderizado al proceso del juego frente a los componentes de escritorio de Windows (DWM).",
                    RegistryOrServiceDetails = "Clave: HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\Tasks\\Games\nValores: GPU Priority = 8, Priority = 6, Scheduling Category = High, SFIO Priority = High",
                    Category = TweakCategory.Rendimiento,
                    CheckAction = () => RegistryHelper.CheckDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8),
                    ApplyAction = (enable) =>
                    {
                        if (enable)
                        {
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8);
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6);
                            RegistryHelper.SetString(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "High");
                            RegistryHelper.SetString(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "SFIO Priority", "High");
                            AppLogger.Success("Prioridad alta de GPU configurada.");
                        }
                        else
                        {
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 2);
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 2);
                            RegistryHelper.SetString(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "Medium");
                            AppLogger.Info("Prioridad de GPU restaurada a estándar.");
                        }
                    }
                },
                new TweakModel
                {
                    Id = "perf_gamedvr_disable",
                    Title = "Desactivar Xbox Game Bar y GameDVR",
                    Description = "Evita la grabación constante en segundo plano, eliminando caídas de FPS y stuttering en juegos.",
                    DetailedExplanation = "GameDVR mantiene un búfer de video circular grabando permanentemente en memoria lo que juegas. Esto genera sobrecarga de codificación en GPU y micro-stuttering en monitores de alta tasa de refresco (144Hz o más).\n\nDesactivarlo libera recursos gráficos de inmediato.",
                    RegistryOrServiceDetails = "Claves:\nHKCU\\System\\GameConfigStore -> GameDVR_Enabled = 0\nHKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\GameDVR -> AppCaptureEnabled = 0\nHKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\GameDVR -> AllowGameDVR = 0",
                    Category = TweakCategory.Rendimiento,
                    CheckAction = () => RegistryHelper.CheckDword(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 0),
                    ApplyAction = (enable) =>
                    {
                        int val = enable ? 0 : 1;
                        RegistryHelper.SetDword(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", val);
                        RegistryHelper.SetDword(RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", val);
                        RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", val);
                        AppLogger.Success($"Xbox GameDVR {(enable ? "desactivado (elimina stutter)" : "activado")}.");
                    }
                },
                new TweakModel
                {
                    Id = "perf_auto_end_tasks",
                    Title = "Cierre rápido de apps al apagar (AutoEndTasks)",
                    Description = "Fuerza el cierre inmediato de aplicaciones colgadas al apagar o reiniciar el equipo.",
                    DetailedExplanation = "Cuando apagas o reinicias Windows, el sistema espera 20 segundos para que las aplicaciones en segundo plano cierren solas. Al activar AutoEndTasks y reducir el timeout a 2 segundos, el equipo se apaga o reinicia de forma casi instantánea.",
                    RegistryOrServiceDetails = "Clave: HKCU\\Control Panel\\Desktop\nValores: AutoEndTasks = \"1\", HungAppTimeout = \"1000\", WaitToKillAppTimeout = \"2000\"",
                    Category = TweakCategory.Rendimiento,
                    CheckAction = () => RegistryHelper.CheckString(RegistryHive.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks", "1"),
                    ApplyAction = (enable) =>
                    {
                        if (enable)
                        {
                            RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks", "1");
                            RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Desktop", "HungAppTimeout", "1000");
                            RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Desktop", "WaitToKillAppTimeout", "2000");
                            AppLogger.Success("AutoEndTasks activado (apagado ultrarrápido).");
                        }
                        else
                        {
                            RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Desktop", "AutoEndTasks", "0");
                            AppLogger.Info("AutoEndTasks restaurado.");
                        }
                    }
                },
                new TweakModel
                {
                    Id = "perf_menu_delay",
                    Title = "Animaciones y menús emergentes instantáneos (0 ms)",
                    Description = "Reduce a cero el retraso de apertura de menús contextuales y elementos flotantes.",
                    DetailedExplanation = "Windows añade un retraso predeterminado de 400 milisegundos a todos los menús contextuales (clic derecho) para esperar si el cursor se desplaza. Con 0 ms, los menús y listas desplegables aparecen de inmediato al hacer clic.",
                    RegistryOrServiceDetails = "Clave: HKCU\\Control Panel\\Desktop\nValor: MenuShowDelay = \"0\"",
                    Category = TweakCategory.Rendimiento,
                    CheckAction = () => RegistryHelper.CheckString(RegistryHive.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", "0"),
                    ApplyAction = (enable) =>
                    {
                        string val = enable ? "0" : "400";
                        RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", val);
                        AppLogger.Success($"MenuShowDelay ajustado a {val} ms.");
                    }
                },
                new TweakModel
                {
                    Id = "sys_long_paths",
                    Title = "Habilitar soporte de rutas largas (> 260 caracteres)",
                    Description = "Elimina la restricción MAX_PATH histórica de Windows.",
                    DetailedExplanation = "Windows conserva por compatibilidad una limitación máxima de 260 caracteres en rutas de archivos. Esto genera errores graves al clonar repositorios Git o carpetas con dependencias profundas como 'node_modules'.\n\nAl activarlo, Windows admite rutas de hasta 32.767 caracteres.",
                    RegistryOrServiceDetails = "Clave: HKLM\\SYSTEM\\CurrentControlSet\\Control\\FileSystem\nValor: LongPathsEnabled = 1",
                    Category = TweakCategory.Rendimiento,
                    CheckAction = () => RegistryHelper.CheckDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", 1),
                    ApplyAction = (enable) =>
                    {
                        int val = enable ? 1 : 0;
                        RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", val);
                        AppLogger.Success($"Soporte de rutas largas {(enable ? "habilitado" : "deshabilitado")}.");
                    }
                },
                new TweakModel
                {
                    Id = "sys_sticky_keys",
                    Title = "Desactivar atajo molesto de Teclas Especiales (Sticky Keys)",
                    Description = "Evita la ventana emergente al pulsar Shift cinco veces consecutivas.",
                    DetailedExplanation = "En juegos que usan la tecla Shift para correr o agacharse, presionar Shift 5 veces abre una molesta ventana modal de accesibilidad de Windows que minimiza el juego. Este tweak desactiva la activación accidental sin afectar el funcionamiento normal de la tecla Shift.",
                    RegistryOrServiceDetails = "Clave: HKCU\\Control Panel\\Accessibility\\StickyKeys\nValor: Flags = \"506\"",
                    Category = TweakCategory.Rendimiento,
                    CheckAction = () => RegistryHelper.CheckString(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", "506"),
                    ApplyAction = (enable) =>
                    {
                        string flags = enable ? "506" : "510";
                        RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", flags);
                        AppLogger.Success($"Atajo de Teclas Especiales {(enable ? "desactivado" : "activado")}.");
                    }
                },

                // ==================== PRIVACIDAD & TELEMETRÍA ====================
                new TweakModel
                {
                    Id = "priv_diagtrack",
                    Title = "Desactivar Telemetría de Windows (DiagTrack)",
                    Description = "Detiene y deshabilita el servicio que recolecta hábitos de uso y diagnósticos continuos.",
                    DetailedExplanation = "El servicio 'DiagTrack' (Connected User Experiences and Telemetry) recolecta periódicamente eventos del sistema, búsquedas y uso de programas y los envía a Microsoft. Suele generar picos de uso del 100% de disco en el Administrador de Tareas a través de 'CompatTelRunner.exe'.\n\nDesactivarlo ahorra ciclos de CPU y lectura continua de disco.",
                    RegistryOrServiceDetails = "Servicio: DiagTrack (Experiencias del usuario y telemetría asociadas)\nClave: HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection -> AllowTelemetry = 0",
                    Category = TweakCategory.Privacidad,
                    CheckAction = () => ServiceHelper.IsServiceDisabled("DiagTrack"),
                    ApplyAction = (enable) =>
                    {
                        if (enable)
                        {
                            ServiceHelper.StopAndDisableService("DiagTrack");
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
                        }
                        else
                        {
                            ServiceHelper.EnableAndStartService("DiagTrack");
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 1);
                        }
                    }
                },
                new TweakModel
                {
                    Id = "priv_copilot",
                    Title = "Desactivar Microsoft Copilot en Windows 11",
                    Description = "Elimina la integración de Copilot de la barra de tareas y procesos en segundo plano.",
                    DetailedExplanation = "Deshabilita por completo la función de Microsoft Copilot integrada en Windows 11. Elimina su botón de la barra de tareas y previene la ejecución de sus procesos en segundo plano basados en Edge WebView.",
                    RegistryOrServiceDetails = "Clave: HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot y HKLM\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot\nValor: TurnOffWindowsCopilot = 1",
                    Category = TweakCategory.Privacidad,
                    CheckAction = () => RegistryHelper.CheckDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1),
                    ApplyAction = (enable) =>
                    {
                        int val = enable ? 1 : 0;
                        RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", val);
                        RegistryHelper.SetDword(RegistryHive.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", val);
                        AppLogger.Success($"Copilot {(enable ? "desactivado" : "habilitado")}.");
                    }
                },
                new TweakModel
                {
                    Id = "priv_browser_telemetry",
                    Title = "Desactivar telemetría de navegadores (Edge y Chrome)",
                    Description = "Impide que Chrome y Edge envíen diagnósticos y métricas de navegación a Google y Microsoft.",
                    DetailedExplanation = "Google Chrome y Microsoft Edge envían reportes constantes de rendimiento, cierres inesperados y estadísticas de uso a sus respectivos servidores en cada sesión.\n\nEste tweak activa las directivas empresariales oficiales de privacidad para ambos navegadores.",
                    RegistryOrServiceDetails = "Claves:\nHKLM\\SOFTWARE\\Policies\\Microsoft\\Edge -> MetricsReportingEnabled = 0\nHKLM\\SOFTWARE\\Policies\\Google\\Chrome -> MetricsReportingEnabled = 0",
                    Category = TweakCategory.Privacidad,
                    CheckAction = () => RegistryHelper.CheckDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "MetricsReportingEnabled", 0),
                    ApplyAction = (enable) =>
                    {
                        int val = enable ? 0 : 1;
                        RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "MetricsReportingEnabled", val);
                        RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Google\Chrome", "MetricsReportingEnabled", val);
                        AppLogger.Success($"Telemetría de Edge y Chrome {(enable ? "desactivada" : "activada")}.");
                    }
                },
                new TweakModel
                {
                    Id = "priv_error_reporting",
                    Title = "Desactivar Reportes de Errores a Microsoft (WerSvc)",
                    Description = "Evita el envío de volcados de memoria y logs de fallos de apps a servidores externos.",
                    DetailedExplanation = "Cuando un programa se congela o cierra de forma inesperada, el servicio WerSvc (Windows Error Reporting) genera un volcado de memoria (crash dump) y retiene el cierre de la app mientras intenta subir la información a Microsoft.\n\nDesactivarlo hace que los programas que fallan se cierren al instante sin bloquear el sistema.",
                    RegistryOrServiceDetails = "Servicio: WerSvc (Servicio de informe de errores de Windows)\nClave: HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Error Reporting -> Disabled = 1",
                    Category = TweakCategory.Privacidad,
                    CheckAction = () => ServiceHelper.IsServiceDisabled("WerSvc"),
                    ApplyAction = (enable) =>
                    {
                        if (enable)
                        {
                            ServiceHelper.StopAndDisableService("WerSvc");
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", 1);
                        }
                        else
                        {
                            ServiceHelper.EnableAndStartService("WerSvc", ServiceHelper.ServiceStartMode.Manual);
                            RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", 0);
                        }
                    }
                },
                new TweakModel
                {
                    Id = "priv_bing_search",
                    Title = "Desactivar Búsquedas de Bing en Menú Inicio",
                    Description = "Hace que el menú Inicio solo busque archivos y programas locales, sin retrasos web.",
                    DetailedExplanation = "Por defecto, cada letra que escribes en la caja de búsqueda del menú Inicio se envía por internet a los servidores de Bing para sugerir resultados web, noticias y publicidad.\n\nAl desactivarlo, el menú Inicio solo busca tus programas, archivos y carpetas locales de forma inmediata.",
                    RegistryOrServiceDetails = "Claves:\nHKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Search -> BingSearchEnabled = 0, CortanaConsent = 0\nHKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search -> DisableWebSearch = 1",
                    Category = TweakCategory.Privacidad,
                    CheckAction = () => RegistryHelper.CheckDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0),
                    ApplyAction = (enable) =>
                    {
                        int val = enable ? 0 : 1;
                        RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", val);
                        RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", val);
                        RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", enable ? 1 : 0);
                        AppLogger.Success($"Búsqueda web en Inicio {(enable ? "desactivada" : "activada")}.");
                    }
                }
            };

            return tweaks;
        }
    }
}
