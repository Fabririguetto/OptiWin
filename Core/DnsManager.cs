using System;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace OptiWin.Core
{
    public static class DnsManager
    {
        public enum DnsProvider
        {
            Cloudflare,
            Google,
            Quad9,
            DefaultDHCP
        }

        public static bool SetDns(DnsProvider provider)
        {
            string primary = "";
            string secondary = "";
            string providerName = "";

            switch (provider)
            {
                case DnsProvider.Cloudflare:
                    primary = "1.1.1.1";
                    secondary = "1.0.0.1";
                    providerName = "Cloudflare (1.1.1.1)";
                    break;
                case DnsProvider.Google:
                    primary = "8.8.8.8";
                    secondary = "8.8.4.4";
                    providerName = "Google Public DNS (8.8.8.8)";
                    break;
                case DnsProvider.Quad9:
                    primary = "9.9.9.9";
                    secondary = "149.112.112.112";
                    providerName = "Quad9 Seguro (9.9.9.9)";
                    break;
                case DnsProvider.DefaultDHCP:
                    providerName = "Automático (DHCP de fábrica)";
                    break;
            }

            try
            {
                // Buscar interfaces activas (Ethernet o Wi-Fi)
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up &&
                        (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        string adapterName = nic.Name;

                        if (provider == DnsProvider.DefaultDHCP)
                        {
                            RunNetsh($"interface ip set dns name=\"{adapterName}\" source=dhcp");
                        }
                        else
                        {
                            RunNetsh($"interface ip set dns name=\"{adapterName}\" static {primary} validate=no");
                            if (!string.IsNullOrEmpty(secondary))
                            {
                                RunNetsh($"interface ip add dns name=\"{adapterName}\" {secondary} index=2 validate=no");
                            }
                        }
                    }
                }

                // Vaciar caché DNS local de Windows
                RunCmd("ipconfig /flushdns");

                AppLogger.Success($"Servidor DNS configurado a: {providerName}. Caché DNS vaciada.");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Error configurando servidor DNS {providerName}", ex);
                return false;
            }
        }

        private static void RunNetsh(string args)
        {
            var psi = new ProcessStartInfo("netsh", args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(3000);
        }

        private static void RunCmd(string command)
        {
            var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(3000);
        }
    }
}
