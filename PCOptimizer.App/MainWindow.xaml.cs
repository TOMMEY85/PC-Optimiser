using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace PCOptimizer;

public partial class MainWindow : Window
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length; public uint MemoryLoad; public ulong TotalPhys; public ulong AvailPhys;
        public ulong TotalPage; public ulong AvailPage; public ulong TotalVirtual; public ulong AvailVirtual; public ulong AvailExtended;
    }

    [DllImport("kernel32.dll")]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus lpBuffer);

    private static readonly string BackupFile =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PCOptimizer", "backup-v7.json");

    private bool _gaming = true;

    public MainWindow()
    {
        InitializeComponent();
        LoadSystemInfo();
        DetectGames();
    }

    private void LoadSystemInfo()
    {
        SystemText.Text = $"{Environment.OSVersion} • {(Environment.Is64BitOperatingSystem ? "64 bits" : "32 bits")}";
        CpuText.Text = GetCpuName();
        GpuText.Text = GetGpuName();

        var mem = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        RamText.Text = GlobalMemoryStatusEx(ref mem)
            ? $"{mem.TotalPhys / 1024d / 1024d / 1024d:0.0} Go"
            : "Non détectée";

        try
        {
            var root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
            var drive = new DriveInfo(root);
            DiskText.Text = $"{drive.AvailableFreeSpace / 1024d / 1024d / 1024d:0} Go libres";
        }
        catch { DiskText.Text = "Non détecté"; }

        var score = CalculateScore(mem.TotalPhys, GetStartupCount());
        ScoreText.Text = $"{score} / 100";
        ScoreBar.Value = score;
        ScoreHint.Text = score >= 80
            ? "Très bon potentiel. Ton système est déjà bien configuré."
            : score >= 65
                ? "Bon potentiel. Quelques optimisations peuvent encore aider."
                : "Des optimisations sont recommandées pour améliorer la réactivité.";
    }

    private static string GetCpuName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return key?.GetValue("ProcessorNameString")?.ToString()?.Trim() ?? "Processeur détecté";
        }
        catch { return "Processeur détecté"; }
    }

    private static string GetGpuName()
    {
        try
        {
            var output = RunCapture("powershell.exe",
                "-NoProfile -NonInteractive -Command \"(Get-CimInstance Win32_VideoController | Where-Object {$_.Name -notmatch 'Microsoft Basic Display'} | Select-Object -First 1 -ExpandProperty Name)\"");
            return string.IsNullOrWhiteSpace(output) ? "Carte graphique détectée" : output.Trim();
        }
        catch { return "Carte graphique détectée"; }
    }

    private static int GetStartupCount()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            return key?.GetValueNames().Length ?? 0;
        }
        catch { return 0; }
    }

    private static int CalculateScore(ulong ramBytes, int startup)
    {
        var ramGb = ramBytes / 1024d / 1024d / 1024d;
        var score = 60;
        if (ramGb >= 16) score += 20; else if (ramGb >= 8) score += 10; else score -= 10;
        if (startup <= 5) score += 10; else if (startup >= 15) score -= 10;
        return Math.Clamp(score, 0, 100);
    }

    private void SetStatus(string message) => StatusText.Text = message;
    private void Home_Click(object sender, RoutedEventArgs e) => SetStatus("Accueil affiché.");
    private void Analyze_Click(object sender, RoutedEventArgs e) { LoadSystemInfo(); DetectGames(); SetStatus("Analyse du PC terminée."); }
    private void Optimize_Click(object sender, RoutedEventArgs e) => OptimizeAll_Click(sender, e);

    private void Gaming_Click(object sender, RoutedEventArgs e)
    {
        _gaming = true;
        ProfileText.Text = "Gaming";
        ProfileTitle.Text = "Gaming";
        ProfileDescription.Text = "Priorité aux FPS, à la réactivité et à la stabilité.";
        SetStatus("Profil Gaming activé.");
    }

    private void Productivity_Click(object sender, RoutedEventArgs e)
    {
        _gaming = false;
        ProfileText.Text = "Productivité";
        ProfileTitle.Text = "Bureautique / Productivité";
        ProfileDescription.Text = "Priorité à la stabilité et au multitâche.";
        SetStatus("Profil Productivité activé.");
    }

    private void OptimizeAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BackupState();
            RunElevated("powercfg.exe", _gaming ? "/setactive SCHEME_MIN" : "/setactive SCHEME_BALANCED");
            FlushDns();
            SetNoMouseAcceleration();
            SetStatus(_gaming
                ? "Optimisation Gaming appliquée et sauvegardée."
                : "Optimisation Productivité appliquée et sauvegardée.");
        }
        catch (Exception ex) { SetStatus($"Optimisation partielle : {ex.Message}"); }
    }

    private void Power_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BackupState();
            RunElevated("powercfg.exe", "/setactive SCHEME_MIN");
            SetStatus("Mode Performances élevées activé. État sauvegardé.");
        }
        catch (Exception ex) { SetStatus($"Impossible d'activer le mode performances : {ex.Message}"); }
    }

    private void Fortnite_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BackupState();
            var boosted = SetGamePriority(new[] { "FortniteClient-Win64-Shipping" });
            OpenGraphicsSettings();
            SetStatus(boosted > 0
                ? "Fortnite : priorité temporaire appliquée au jeu en cours + paramètres graphiques ouverts."
                : "Fortnite : paramètres graphiques ouverts. Lance le jeu puis utilise le booster.");
        }
        catch (Exception ex) { SetStatus($"Optimisation Fortnite partielle : {ex.Message}"); }
    }

    private void CallOfDuty_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BackupState();
            var boosted = SetGamePriority(new[] { "cod", "ModernWarfare", "cod-Win64-Shipping" });
            OpenGraphicsSettings();
            SetStatus(boosted > 0
                ? "Call of Duty : priorité temporaire appliquée au jeu en cours + paramètres graphiques ouverts."
                : "Call of Duty : paramètres graphiques ouverts. Lance le jeu puis utilise le booster.");
        }
        catch (Exception ex) { SetStatus($"Optimisation Call of Duty partielle : {ex.Message}"); }
    }

    private void GameBooster_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var boosted = SetGamePriority(new[]
            {
                "FortniteClient-Win64-Shipping", "cod", "ModernWarfare",
                "r5apex", "VALORANT-Win64-Shipping", "RocketLeague"
            });

            SetStatus(boosted > 0
                ? $"{boosted} jeu(x) en cours passé(s) en priorité AboveNormal. Le changement est temporaire."
                : "Aucun jeu pris en charge n'est actuellement lancé.");
        }
        catch (Exception ex) { SetStatus($"Booster partiel : {ex.Message}"); }
    }

    private static int SetGamePriority(string[] processNames)
    {
        var count = 0;
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var baseName = Path.GetFileNameWithoutExtension(process.ProcessName);
                if (!processNames.Any(n =>
                        string.Equals(Path.GetFileNameWithoutExtension(n), baseName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                process.PriorityClass = ProcessPriorityClass.AboveNormal;
                count++;
            }
            catch { }
            finally { process.Dispose(); }
        }
        return count;
    }

    private void Mouse_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BackupState();
            SetNoMouseAcceleration();
            SetStatus("Accélération souris désactivée. État sauvegardé.");
        }
        catch (Exception ex) { SetStatus($"Réglage souris refusé : {ex.Message}"); }
    }

    private static void SetNoMouseAcceleration()
    {
        using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse");
        key?.SetValue("MouseSpeed", "0", RegistryValueKind.String);
        key?.SetValue("MouseThreshold1", "0", RegistryValueKind.String);
        key?.SetValue("MouseThreshold2", "0", RegistryValueKind.String);
    }

    private void Network_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            FlushDns();
            var ping = new ProcessStartInfo("ping.exe", "-n 3 1.1.1.1")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(ping);
            var output = process?.StandardOutput.ReadToEnd() ?? string.Empty;
            process?.WaitForExit();

            var match = Regex.Match(output, @"Moyenne = (\\d+)ms", RegexOptions.IgnoreCase);
            SetStatus(match.Success
                ? $"DNS vidé. Latence moyenne : {match.Groups[1].Value} ms."
                : "DNS vidé. Test réseau terminé.");
        }
        catch (Exception ex) { SetStatus($"Diagnostic réseau impossible : {ex.Message}"); }
    }

    private static void FlushDns() => RunElevated("ipconfig.exe", "/flushdns");

    private void Cleanup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var temp = Path.GetTempPath();
            var deleted = 0;

            foreach (var file in Directory.EnumerateFiles(temp))
            {
                try { File.Delete(file); deleted++; } catch { }
            }

            foreach (var dir in Directory.EnumerateDirectories(temp))
            {
                try { Directory.Delete(dir, true); deleted++; } catch { }
            }

            SetStatus($"Nettoyage terminé : {deleted} éléments temporaires traités.");
        }
        catch (Exception ex) { SetStatus($"Nettoyage partiel : {ex.Message}"); }
    }

    private void Graphics_Click(object sender, RoutedEventArgs e) => OpenGraphicsSettings();
    private static void OpenGraphicsSettings() => Start("ms-settings:display-advancedgraphics");

    private void Backup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BackupState();
            SetStatus($"Sauvegarde créée dans : {BackupFile}");
        }
        catch (Exception ex) { SetStatus($"Sauvegarde impossible : {ex.Message}"); }
    }

    private void BackupState()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(BackupFile)!);

        var (guid, name) = GetActivePowerScheme();
        var mouse = new MouseBackup();

        using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse"))
        {
            mouse.MouseSpeed = key?.GetValue("MouseSpeed")?.ToString() ?? "";
            mouse.Threshold1 = key?.GetValue("MouseThreshold1")?.ToString() ?? "";
            mouse.Threshold2 = key?.GetValue("MouseThreshold2")?.ToString() ?? "";
        }

        var state = new BackupState
        {
            PowerGuid = guid,
            PowerName = name,
            Mouse = mouse,
            SavedAt = DateTimeOffset.Now
        };

        File.WriteAllText(
            BackupFile,
            JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);
    }

    private void Restore_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!File.Exists(BackupFile))
            {
                SetStatus("Aucune sauvegarde trouvée. Lance d'abord une optimisation ou une sauvegarde manuelle.");
                return;
            }

            var state = JsonSerializer.Deserialize<BackupState>(
                File.ReadAllText(BackupFile, Encoding.UTF8));

            if (state is null)
            {
                SetStatus("Sauvegarde illisible.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(state.PowerGuid))
                RunElevated("powercfg.exe", $"/setactive {state.PowerGuid}");

            using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse");
            key?.SetValue("MouseSpeed", state.Mouse?.MouseSpeed ?? "", RegistryValueKind.String);
            key?.SetValue("MouseThreshold1", state.Mouse?.Threshold1 ?? "", RegistryValueKind.String);
            key?.SetValue("MouseThreshold2", state.Mouse?.Threshold2 ?? "", RegistryValueKind.String);

            SetStatus($"Restauration terminée — sauvegarde du {state.SavedAt:dd/MM/yyyy HH:mm}.");
        }
        catch (Exception ex) { SetStatus($"Restauration impossible : {ex.Message}"); }
    }

    private void DetectGames()
    {
        var locations = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Epic Games", "Fortnite"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Epic Games", "Fortnite"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Call of Duty"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Call of Duty")
        };

        var fortnite = locations.Any(p => p.EndsWith("Fortnite", StringComparison.OrdinalIgnoreCase) && Directory.Exists(p));
        var cod = locations.Any(p => p.EndsWith("Call of Duty", StringComparison.OrdinalIgnoreCase) && Directory.Exists(p));

        GameStatusText.Text = fortnite || cod
            ? $"Jeux détectés : {(fortnite ? "Fortnite " : "")}{(cod ? "Call of Duty" : "")}".Trim()
            : "Fortnite / Call of Duty non détectés dans les emplacements standards. Les boutons restent disponibles pour les jeux déjà lancés.";
    }

    private static (string Guid, string Name) GetActivePowerScheme()
    {
        try
        {
            var output = RunCapture("powercfg.exe", "/getactivescheme");
            var match = Regex.Match(output, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
            var nameStart = output.IndexOf('(');
            var nameEnd = output.IndexOf(')');
            var name = nameStart >= 0 && nameEnd > nameStart ? output[(nameStart + 1)..nameEnd] : "";
            return (match.Value, name);
        }
        catch { return ("", ""); }
    }

    private static void RunElevated(string file, string arguments)
    {
        using var process = Process.Start(new ProcessStartInfo(file, arguments)
        {
            UseShellExecute = true,
            Verb = "runas"
        });
        process?.WaitForExit();
    }

    private static string RunCapture(string file, string arguments)
    {
        using var process = Process.Start(new ProcessStartInfo(file, arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        });

        return process?.StandardOutput.ReadToEnd() ?? string.Empty;
    }

    private static void Start(string file)
    {
        try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); }
        catch { }
    }

    private sealed class BackupState
    {
        public string PowerGuid { get; set; } = "";
        public string PowerName { get; set; } = "";
        public MouseBackup Mouse { get; set; } = new();
        public DateTimeOffset SavedAt { get; set; }
    }

    private sealed class MouseBackup
    {
        public string MouseSpeed { get; set; } = "";
        public string Threshold1 { get; set; } = "";
        public string Threshold2 { get; set; } = "";
    }
}