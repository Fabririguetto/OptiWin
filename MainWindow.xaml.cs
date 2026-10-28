using OptiWin.Core;
using OptiWin.Core.Profiles;
using OptiWin.Core.Tweaks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace OptiWin
{
    public partial class MainWindow : FluentWindow
    {
        private readonly List<TweakModel> _tweaks = new();
        private readonly DispatcherTimer _monitorTimer = new();
        private List<StartupItem> _startupItems = new();
        private List<BloatwareApp> _bloatwareApps = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Suscribir logger
            AppLogger.OnLog += LogReceived;
            AppLogger.Info("OptiWin 2.0 (Fluent Windows 11) iniciado correctamente.");

            // 2. Verificar privilegios de administrador
            CheckAdminPrivileges();

            // 3. Cargar catálogo de tweaks
            LoadTweaks();

            // 4. Cargar Startup Items y Bloatware
            LoadStartupAndBloatware();

            // 5. Iniciar monitor de hardware en tiempo real (CPU, GPU, RAM, Discos)
            _monitorTimer.Interval = TimeSpan.FromSeconds(2);
            _monitorTimer.Tick += (s, args) => UpdateHardwareUI();
            _monitorTimer.Start();
            UpdateHardwareUI();
        }

        private void CheckAdminPrivileges()
        {
            bool isAdmin = false;
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { }

            if (isAdmin)
            {
                TxtAdminStatus.Text = "Admin (Elevado)";
                TxtAdminStatus.Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#10b981")!;
                AppLogger.Info("Ejecutando con permisos de Administrador completos.");
            }
            else
            {
                TxtAdminStatus.Text = "No Elevado";
                TxtAdminStatus.Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#f87171")!;
                AppLogger.Warning("OptiWin no está ejecutándose como Administrador. Ejecuta como Administrador para habilitar cambios en HKLM y Servicios.");
            }
        }

        private void LoadTweaks()
        {
            _tweaks.Clear();
            _tweaks.AddRange(TweakManager.GetAllTweaks());

            Task.Run(() =>
            {
                foreach (var tweak in _tweaks)
                {
                    tweak.RefreshStatus();
                }

                Dispatcher.Invoke(() =>
                {
                    ListGamingTweaks.ItemsSource = _tweaks.Where(t => t.Category == TweakCategory.Rendimiento).ToList();
                    ListPrivacyTweaks.ItemsSource = _tweaks.Where(t => t.Category == TweakCategory.Privacidad).ToList();
                    AppLogger.Info($"Se cargaron y verificaron {_tweaks.Count} optimizaciones del sistema.");
                });
            });
        }

        private void LoadStartupAndBloatware()
        {
            Task.Run(() =>
            {
                _startupItems = StartupManager.GetStartupItems();
                _bloatwareApps = UwpCleaner.GetKnownBloatware();

                Dispatcher.Invoke(() =>
                {
                    ListStartupItems.ItemsSource = _startupItems;
                    ListBloatwareItems.ItemsSource = _bloatwareApps;
                });
            });
        }

        private void UpdateHardwareUI()
        {
            Task.Run(() =>
            {
                var snap = HardwareMonitor.GetSnapshot();
                var ram = RamOptimizer.GetCurrentRamInfo();

                Dispatcher.Invoke(() =>
                {
                    // Header Badges
                    HeaderCpuText.Text = $"{snap.CpuUsagePercent}%";
                    HeaderGpuText.Text = $"{snap.GpuUsagePercent}%";
                    HeaderRamText.Text = $"{ram.UsagePercent}%";

                    // Dashboard CPU
                    DashCpuPercent.Text = $"{snap.CpuUsagePercent}%";
                    PbDashCpu.Value = snap.CpuUsagePercent;
                    DashCpuName.Text = snap.CpuName;

                    // Dashboard GPU
                    DashGpuPercent.Text = $"{snap.GpuUsagePercent}%";
                    PbDashGpu.Value = snap.GpuUsagePercent;
                    DashGpuCardName.Text = snap.GpuName;

                    // Dashboard RAM
                    DashRamPercent.Text = $"{ram.UsagePercent}%";
                    PbDashRam.Value = ram.UsagePercent;
                    DashRamDetails.Text = $"En uso: {ram.UsedGB} GB / {ram.TotalGB} GB";

                    // Dashboard All Disks
                    ListAllDisks.ItemsSource = snap.AllDisks;

                    // Specs Card
                    DashOsName.Text = snap.OsVersion;
                    DashGpuName.Text = snap.GpuName;
                    DashMachineName.Text = snap.MachineName;
                    DashUptime.Text = snap.UptimeFormatted;

                    // RAM Tab View
                    TxtRamPercentBig.Text = $"{ram.UsagePercent}%";
                    TxtRamDetails.Text = $"En uso: {ram.UsedGB} GB | Libre: {ram.FreeGB} GB | Total: {ram.TotalGB} GB";
                    PbRamUsage.Value = ram.UsagePercent;
                });
            });
        }

        private void TweakToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts && ts.DataContext is TweakModel tweak)
            {
                bool targetState = ts.IsChecked == true;
                AppLogger.Info($"Modificando tweak: '{tweak.Title}' -> {(targetState ? "ACTIVADO" : "DESACTIVADO")}");

                Task.Run(() =>
                {
                    try
                    {
                        tweak.ApplyAction?.Invoke(targetState);
                        tweak.RefreshStatus();
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error($"Error aplicando '{tweak.Title}'", ex);
                    }
                });
            }
        }

        private void BtnInfoTweak_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is TweakModel tweak)
            {
                ModalTweakTitle.Text = tweak.Title;
                ModalTweakExplanation.Text = tweak.DetailedExplanation;
                ModalTweakTechDetails.Text = tweak.RegistryOrServiceDetails;

                if (tweak.HasWarning)
                {
                    ModalTweakWarning.Text = tweak.Warning;
                    ModalWarningBox.Visibility = Visibility.Visible;
                }
                else
                {
                    ModalWarningBox.Visibility = Visibility.Collapsed;
                }

                InfoModalOverlay.Visibility = Visibility.Visible;
            }
        }

        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            InfoModalOverlay.Visibility = Visibility.Collapsed;
        }

        // ================= PERFILES MASIVOS (ACCIONES EN 1 CLIC) =================

        public void BtnInfoGaming_Click(object sender, RoutedEventArgs e)
        {
            ModalTweakTitle.Text = "🎮 Perfil 1-Clic: Modo Gaming de Alto Rendimiento";
            ModalTweakExplanation.Text = "Configura el sistema para exprimir el 100% de la capacidad de tu CPU, tarjeta gráfica y red durante sesiones de juego, eliminando retrasos, latencias artificiales y cuellos de botella.";
            ModalTweakWarning.Text = "Al forzar el plan Ultimate Performance y GPU Priority 8, el procesador y gráfica no entrarán en estados de bajo consumo mientras juegues, aumentando las temperaturas y el consumo de batería en notebooks. Además sobrescribirá los servidores DNS actuales por Cloudflare (1.1.1.1).";
            ModalTweakTechDetails.Text = "• Plan de Energía: HKLM & powercfg -> Scheme 'Ultimate Performance'\n• Prioridad de GPU: HKLM\\...\\Multimedia\\SystemProfile\\Tasks\\Games -> GPU Priority = 8, Priority = 6\n• Network Throttling: Multimedia\\SystemProfile -> NetworkThrottlingIndex = 0xFFFFFFFF\n• System Responsiveness: Multimedia\\SystemProfile -> SystemResponsiveness = 0\n• DNS: Cloudflare IPv4 (1.1.1.1 / 1.0.0.1) & ipconfig /flushdns\n• Memoria RAM: EmptyWorkingSet en todos los procesos activos";
            ModalWarningBox.Visibility = Visibility.Visible;
            InfoModalOverlay.Visibility = Visibility.Visible;
        }

        public void BtnInfoWork_Click(object sender, RoutedEventArgs e)
        {
            ModalTweakTitle.Text = "💼 Perfil 1-Clic: Modo Trabajo y Productividad";
            ModalTweakExplanation.Text = "Optimiza la fluidez y respuesta del entorno de oficina y programación, habilita el soporte de rutas largas (>260 caracteres), elimina la telemetría de navegadores y acelera la búsqueda en Inicio sin Bing.";
            ModalTweakWarning.Text = "Configura los servidores DNS en Google (8.8.8.8). Si tu equipo se conecta a una red corporativa con servidores DNS internos o intranets empresariales, deberás reconfigurarlos manualmente.";
            ModalTweakTechDetails.Text = "• Plan de Energía: Esquema Equilibrado de Windows\n• Búsqueda Local: HKCU\\...\\Search -> BingSearchEnabled = 0, CortanaConsent = 0\n• Soporte Rutas Largas: HKLM\\...\\FileSystem -> LongPathsEnabled = 1\n• Privacidad: DiagTrack detenido/deshabilitado, telemetría Edge/Chrome = 0\n• DNS: Google Public DNS (8.8.8.8 / 8.8.4.4)";
            ModalWarningBox.Visibility = Visibility.Visible;
            InfoModalOverlay.Visibility = Visibility.Visible;
        }

        public void BtnInfoCreator_Click(object sender, RoutedEventArgs e)
        {
            ModalTweakTitle.Text = "🎬 Perfil 1-Clic: Modo Creación de Contenido / Render";
            ModalTweakExplanation.Text = "Diseñado para software de edición de video, renderizado 3D (Blender, Premiere, After Effects, DaVinci) y compilación pesada. Maximiza el cómputo de CPU/GPU y purga la memoria antes de renderizar.";
            ModalTweakWarning.Text = "La liberación masiva de RAM vacía el Working Set de todos los programas en ejecución, lo cual puede producir micro-pausas transitorias de 1-2 segundos mientras las aplicaciones en segundo plano reasignan sus búferes.";
            ModalTweakTechDetails.Text = "• Plan de Energía: Ultimate Performance\n• Prioridad Gráfica: GPU Priority = 8, Scheduling Category = High\n• Responsiveness: SystemResponsiveness = 0 (100% CPU)\n• Soporte Rutas: LongPathsEnabled = 1\n• Memoria RAM: Purga agresiva EmptyWorkingSet para maximizar buffer de render";
            ModalWarningBox.Visibility = Visibility.Visible;
            InfoModalOverlay.Visibility = Visibility.Visible;
        }

        public void BtnInfoRestore_Click(object sender, RoutedEventArgs e)
        {
            ModalTweakTitle.Text = "🔄 Perfil 1-Clic: Restaurar Valores de Fábrica";
            ModalTweakExplanation.Text = "Regresa las directivas del sistema operativo a los estándares predeterminados recomendados por Microsoft, revirtiendo tweaks de registro, servicios y configuración de red modificados.";
            ModalTweakWarning.Text = "Se eliminarán todas las optimizaciones aplicadas previamente. Los adaptadores de red volverán a solicitar DNS dinámicos por DHCP y el plan de energía regresará al estándar Equilibrado.";
            ModalTweakTechDetails.Text = "• Plan de Energía: Restaurar GUID Equilibrado (381b4222-f694-41f0-9685-ff5bb260df2e)\n• Red & DNS: Revertir a automático por DHCP (EnableDHCP) y vaciado de caché DNS\n• Tweaks: Desactivación y restauración de valores por defecto en todo el catálogo";
            ModalWarningBox.Visibility = Visibility.Visible;
            InfoModalOverlay.Visibility = Visibility.Visible;
        }

        private async void BtnProfileGaming_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "¿Deseas activar el MODO GAMING?\n\n" +
                "• Plan Ultimate Performance\n" +
                "• GPU Priority Máxima & Latencia 0\n" +
                "• Network Throttling Desactivado\n" +
                "• DNS Cloudflare (1.1.1.1)\n" +
                "• Vaciado de memoria RAM\n\n" +
                "⚠️ ADVERTENCIA:\n" +
                "Aumentará el consumo de energía y temperaturas en notebooks. Sobrescribirá las configuraciones DNS previas.",
                "Modo Gaming - OptiWin",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (res == MessageBoxResult.Yes)
            {
                await ProfileManager.ApplyGamingProfileAsync(_tweaks);
                LoadTweaks();
                UpdateHardwareUI();
                MessageBox.Show("¡Modo Gaming Activado al 100%!", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnProfileWork_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "¿Deseas activar el MODO TRABAJO?\n\n" +
                "• Plan de energía equilibrado\n" +
                "• Búsqueda local instantánea sin Bing\n" +
                "• Soporte de Rutas Largas habilitado\n" +
                "• Telemetría de Office y Navegadores bloqueada\n" +
                "• DNS Google (8.8.8.8)\n\n" +
                "⚠️ ADVERTENCIA:\n" +
                "Asignará DNS 8.8.8.8. Si tu red requiere DNS corporativo interno, reconfigúralo tras aplicar el perfil.",
                "Modo Trabajo - OptiWin",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (res == MessageBoxResult.Yes)
            {
                await ProfileManager.ApplyWorkProfileAsync(_tweaks);
                LoadTweaks();
                UpdateHardwareUI();
                MessageBox.Show("¡Modo Trabajo y Productividad Activado!", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnProfileCreator_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "¿Deseas activar el MODO CREACIÓN / RENDER?\n\n" +
                "• Máximo procesamiento de CPU y GPU\n" +
                "• Vaciado masivo de RAM previo a renders\n" +
                "• Cierre rápido de procesos colgados\n\n" +
                "⚠️ ADVERTENCIA:\n" +
                "El vaciado profundo de RAM puede generar micro-pausas transitorias de 1-2 segundos en apps de fondo.",
                "Modo Creación - OptiWin",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (res == MessageBoxResult.Yes)
            {
                await ProfileManager.ApplyCreatorProfileAsync(_tweaks);
                LoadTweaks();
                UpdateHardwareUI();
                MessageBox.Show("¡Modo Creación de Contenido Activado!", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnProfileRestore_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "¿Deseas restaurar la configuración original de fábrica de Windows?\n\n" +
                "Se restablecerán los planes de energía, DNS por defecto y ajustes predeterminados.\n\n" +
                "⚠️ ADVERTENCIA:\n" +
                "Se revertirán todas las personalizaciones y optimizaciones aplicadas.",
                "Restaurar Fábrica - OptiWin",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                await ProfileManager.RestoreDefaultsAsync(_tweaks);
                LoadTweaks();
                UpdateHardwareUI();
                MessageBox.Show("¡Valores originales de Windows restaurados!", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ================= DNS MANAGING =================

        private void BtnDnsCloudflare_Click(object sender, RoutedEventArgs e)
        {
            DnsManager.SetDns(DnsManager.DnsProvider.Cloudflare);
            MessageBox.Show("Servidores DNS cambiados a Cloudflare (1.1.1.1 / 1.0.0.1) exitosamente.", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDnsGoogle_Click(object sender, RoutedEventArgs e)
        {
            DnsManager.SetDns(DnsManager.DnsProvider.Google);
            MessageBox.Show("Servidores DNS cambiados a Google (8.8.8.8 / 8.8.4.4) exitosamente.", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDnsQuad9_Click(object sender, RoutedEventArgs e)
        {
            DnsManager.SetDns(DnsManager.DnsProvider.Quad9);
            MessageBox.Show("Servidores DNS cambiados a Quad9 Seguro (9.9.9.9) exitosamente.", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDnsDhcp_Click(object sender, RoutedEventArgs e)
        {
            DnsManager.SetDns(DnsManager.DnsProvider.DefaultDHCP);
            MessageBox.Show("DNS Automático (DHCP) restaurado exitosamente.", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ================= STARTUP & BLOATWARE =================

        private void BtnRemoveStartup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button btn && btn.DataContext is StartupItem item)
            {
                var res = MessageBox.Show($"¿Deseas deshabilitar del inicio '{item.Name}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    StartupManager.RemoveStartupItem(item);
                    LoadStartupAndBloatware();
                }
            }
        }

        private async void BtnUninstallBloatware_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button btn && btn.DataContext is BloatwareApp app)
            {
                var res = MessageBox.Show($"¿Deseas desinstalar la app preinstalada '{app.DisplayName}'?", "Confirmar Desinstalación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    btn.IsEnabled = false;
                    await UwpCleaner.UninstallAppAsync(app);
                    btn.IsEnabled = true;
                    MessageBox.Show($"¡App '{app.DisplayName}' desinstalada!", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // ================= RESTORE POINT =================

        private async void BtnCreateRestorePoint_Click(object sender, RoutedEventArgs e)
        {
            AppLogger.Info("Generando Punto de Restauración del Sistema...");
            bool ok = await RestorePointHelper.CreateRestorePointAsync();
            if (ok)
            {
                MessageBox.Show("¡Punto de Restauración creado con éxito en Windows!", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No se pudo crear el punto de restauración. Asegúrate de ejecutar OptiWin como Administrador y tener la Protección del Sistema habilitada en C:.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ================= DISK CLEANER & RAM =================

        private async void BtnScanJunk_Click(object sender, RoutedEventArgs e)
        {
            BtnScanJunk.IsEnabled = false;
            AppLogger.Info("Iniciando escaneo de archivos basura y cachés...");

            long bytes = await CleanerService.ScanJunkFilesAsync();
            double mb = Math.Round(bytes / (1024.0 * 1024.0), 1);
            if (mb > 1024)
            {
                TxtCleanerSpace.Text = $"{Math.Round(mb / 1024.0, 2)} GB";
            }
            else
            {
                TxtCleanerSpace.Text = $"{mb} MB";
            }

            BtnScanJunk.IsEnabled = true;
        }

        private async void BtnCleanJunk_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "¿Deseas proceder con la limpieza de archivos temporales, cachés y vaciado de papelera de reciclaje?",
                "Confirmación de Limpieza - OptiWin",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            BtnCleanJunk.IsEnabled = false;
            AppLogger.Info("Iniciando proceso de limpieza profunda...");

            long bytesDeleted = await CleanerService.CleanJunkFilesAsync();
            double mb = Math.Round(bytesDeleted / (1024.0 * 1024.0), 1);
            TxtCleanerSpace.Text = "0 MB";

            MessageBox.Show($"¡Limpieza completada con éxito!\nSe recuperaron {mb} MB de espacio en disco.", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnCleanJunk.IsEnabled = true;
        }

        private void BtnFreeRam_Click(object sender, RoutedEventArgs e)
        {
            BtnFreeRam.IsEnabled = false;
            AppLogger.Info("Vaciando Working Set de procesos en memoria...");

            double freedMB = RamOptimizer.FreeMemory();
            UpdateHardwareUI();

            MessageBox.Show($"¡Memoria RAM optimizada!\nSe liberaron aproximadamente {freedMB} MB de memoria.", "OptiWin", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnFreeRam.IsEnabled = true;
        }

        private void BtnRefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            AppLogger.Info("Re-evaluando estado de todo el sistema...");
            LoadTweaks();
            LoadStartupAndBloatware();
            UpdateHardwareUI();
        }

        // ================= NAVIGATION =================

        private void NavChanged(object sender, RoutedEventArgs e)
        {
            if (ViewDashboard == null || ViewGaming == null || ViewPrivacy == null || 
                ViewDns == null || ViewStartup == null || ViewCleaner == null || ViewRam == null) return;

            ViewDashboard.Visibility = NavDashboard.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            ViewGaming.Visibility = NavGaming.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            ViewPrivacy.Visibility = NavPrivacy.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            ViewDns.Visibility = NavDns.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            ViewStartup.Visibility = NavStartup.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            ViewCleaner.Visibility = NavCleaner.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            ViewRam.Visibility = NavRam.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LogReceived(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                TxtLogOutput.AppendText(msg + Environment.NewLine);
                LogScrollViewer.ScrollToEnd();
            });
        }

        private void TxtLogOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogScrollViewer?.ScrollToEnd();
        }
    }
}
