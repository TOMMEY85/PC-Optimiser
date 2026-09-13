using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;

namespace PCOptimizer;

public partial class MainWindow : Window
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MemoryStatus { public uint Length; public uint MemoryLoad; public ulong TotalPhys; public ulong AvailPhys; public ulong TotalPage; public ulong AvailPage; public ulong TotalVirtual; public ulong AvailVirtual; public ulong AvailExtended; }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern bool GlobalMemoryStatusEx(ref MemoryStatus lpBuffer);

    private bool _gaming = true;

    public MainWindow()
    {
        InitializeComponent();
        LoadSystemInfo();
    }

    private void LoadSystemInfo()
    {
        SystemText.Text = $"{Environment.OSVersion} • {(Environment.Is64BitOperatingSystem ? "64 bits" : "32 bits")}";
        CpuText.Text = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Processeur détecté";
        var mem = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        RamText.Text = GlobalMemoryStatusEx(ref mem) ? $"{mem.TotalPhys / 1024d / 1024d / 1024d:0.0} Go" : "Non détectée";
        try
        {
            var root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
            var drive = new DriveInfo(root);
            DiskText.Text = $"{drive.AvailableFreeSpace / 1024d / 1024d / 1024d:0} Go libres";
        }
        catch { DiskText.Text = "Non détecté"; }
    }

    private void SetStatus(string message) { StatusText.Text = message; StatusDetail.Text = message; }
    private void Home_Click(object sender, RoutedEventArgs e) => SetStatus("Accueil affiché.");
    private void Analyze_Click(object sender, RoutedEventArgs e) { LoadSystemInfo(); SetStatus("Analyse du PC terminée."); }
    private void Optimize_Click(object sender, RoutedEventArgs e) => OptimizeAll_Click(sender, e);
    private void Gaming_Click(object sender, RoutedEventArgs e) { _gaming = true; ProfileText.Text = "Gaming"; ProfileTitle.Text = "Gaming"; ProfileDescription.Text = "Priorité aux FPS, à la réactivité et à la stabilité."; SetStatus("Profil Gaming activé."); }
    private void Productivity_Click(object sender, RoutedEventArgs e) { _gaming = false; ProfileText.Text = "Productivité"; ProfileTitle.Text = "Bureautique / Productivité"; ProfileDescription.Text = "Priorité à la stabilité et au multitâche."; SetStatus("Profil Productivité activé."); }
    private void Network_Click(object sender, RoutedEventArgs e) => Dns_Click(sender, e);

    private void OptimizeAll_Click(object sender, RoutedEventArgs e)
    {
        try { Run("powercfg.exe", _gaming ? "/setactive SCHEME_MIN" : "/setactive SCHEME_BALANCED"); FlushDns(); SetStatus(_gaming ? "Optimisation Gaming appliquée." : "Optimisation Productivité appliquée."); }
        catch (Exception ex) { SetStatus($"Optimisation partielle : {ex.Message}"); }
    }
    private void Power_Click(object sender, RoutedEventArgs e) { try { Run("powercfg.exe", "/setactive SCHEME_MIN"); SetStatus("Mode Hautes performances activé."); } catch (Exception ex) { SetStatus($"Impossible d'activer le mode performances : {ex.Message}"); } }
    private void GamingOptimize_Click(object sender, RoutedEventArgs e) { try { Run("powercfg.exe", "/setactive SCHEME_MIN"); FlushDns(); SetStatus("Réglages Gaming de base appliqués."); } catch (Exception ex) { SetStatus($"Optimisation Gaming partielle : {ex.Message}"); } }

    private void Cleanup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var temp = Path.GetTempPath(); var deleted = 0;
            foreach (var file in Directory.EnumerateFiles(temp)) { try { File.Delete(file); deleted++; } catch { } }
            foreach (var dir in Directory.EnumerateDirectories(temp)) { try { Directory.Delete(dir, true); deleted++; } catch { } }
            SetStatus($"Nettoyage terminé : {deleted} éléments temporaires traités.");
        }
        catch (Exception ex) { SetStatus($"Nettoyage partiel : {ex.Message}"); }
    }
    private void Dns_Click(object sender, RoutedEventArgs e) { try { FlushDns(); SetStatus("Cache DNS vidé."); } catch (Exception ex) { SetStatus($"Impossible de vider le DNS : {ex.Message}"); } }
    private static void FlushDns() => Run("ipconfig.exe", "/flushdns");

    private void Mouse_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse");
            key?.SetValue("MouseSpeed", "0", RegistryValueKind.String); key?.SetValue("MouseThreshold1", "0", RegistryValueKind.String); key?.SetValue("MouseThreshold2", "0", RegistryValueKind.String);
            SetStatus("Accélération souris Windows désactivée pour la session suivante.");
        }
        catch (Exception ex) { SetStatus($"Réglage souris refusé : {ex.Message}"); }
    }
    private void TaskManager_Click(object sender, RoutedEventArgs e) => Start("taskmgr.exe");
    private void Graphics_Click(object sender, RoutedEventArgs e) => Start("ms-settings:display-advancedgraphics");
    private void Restore_Click(object sender, RoutedEventArgs e) => SetStatus("Aucune restauration automatique n'a été exécutée. Utilise un point de restauration Windows pour revenir en arrière.");
    private static void Start(string file) { try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); } catch { } }
    private static void Run(string file, string arguments) { using var process = Process.Start(new ProcessStartInfo(file, arguments) { UseShellExecute = true, Verb = "runas" }); process?.WaitForExit(); }
}
