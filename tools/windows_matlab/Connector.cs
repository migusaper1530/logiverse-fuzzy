using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("LogiVerse MATLAB")]
[assembly: AssemblyDescription("Conector de LogiVerse para Fuzzy Logic Designer")]
[assembly: AssemblyProduct("LogiVerse MATLAB")]
[assembly: AssemblyVersion("1.0.0.0")]

public static class MatlabConnector
{
    private const string ProtocolKey = @"Software\Classes\logiverse-matlab";
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LogiVerseMatlab");
    private static string Config { get { return Path.Combine(Folder, "matlab-path.txt"); } }

    public static string ParseAction(string value)
    {
        Uri uri;
        if (value == null || value.Length > 100 || !Uri.TryCreate(value, UriKind.Absolute, out uri)
            || uri.Scheme != "logiverse-matlab" || uri.UserInfo != "" || uri.Port != -1
            || (uri.AbsolutePath != "" && uri.AbsolutePath != "/")
            || uri.Query != "" || uri.Fragment != ""
            || (uri.Host != "fuzzy" && uri.Host != "cargar"))
            throw new ArgumentException("Solicitud no válida. Usa los botones de LogiVerse.");
        return uri.Host;
    }

    public static string BuildMatlabCommand(string fisPath)
    {
        string action = "fuzzyLogicDesigner;";
        if (fisPath != null)
        {
            if (!File.Exists(fisPath) || !String.Equals(Path.GetExtension(fisPath), ".fis", StringComparison.OrdinalIgnoreCase)
                || fisPath.IndexOfAny(new char[] { '"', '\r', '\n', '\0' }) >= 0)
                throw new ArgumentException("Selecciona un archivo .fis existente.");
            action = "fis = readfis('" + Path.GetFullPath(fisPath).Replace("'", "''") + "'); fuzzyLogicDesigner(fis);";
        }
        return "try; " + action + " catch err; errordlg(err.message, 'LogiVerse - MATLAB'); end;";
    }

    public static ProcessStartInfo StartInfo(string matlab, string fisPath)
    {
        if (!File.Exists(matlab) || !String.Equals(Path.GetFileName(matlab), "matlab.exe", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("No se encuentra matlab.exe. Ejecuta Instalar-conector.cmd y elige tu versión activada.");
        return new ProcessStartInfo {
            FileName = Path.GetFullPath(matlab),
            Arguments = "-wait -r \"" + BuildMatlabCommand(fisPath) + "\"",
            UseShellExecute = false,
            WorkingDirectory = Folder
        };
    }

    private static bool Configure()
    {
        using (var dialog = new OpenFileDialog())
        {
            dialog.Title = "Selecciona el matlab.exe de tu versión activada (carpeta MATLAB / versión / bin)";
            dialog.Filter = "MATLAB (matlab.exe)|matlab.exe";
            dialog.CheckFileExists = true;
            string initial = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MATLAB");
            if (Directory.Exists(initial)) dialog.InitialDirectory = initial;
            if (dialog.ShowDialog() != DialogResult.OK) return false;
            if (!String.Equals(Path.GetFileName(dialog.FileName), "matlab.exe", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Elige matlab.exe dentro de la carpeta bin de tu MATLAB.");
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Config, Path.GetFullPath(dialog.FileName));
        }
        string executable = Application.ExecutablePath;
        using (var key = Registry.CurrentUser.CreateSubKey(ProtocolKey))
        {
            key.SetValue("", "URL:LogiVerse MATLAB");
            key.SetValue("URL Protocol", "");
            using (var command = key.CreateSubKey(@"shell\open\command"))
                command.SetValue("", "\"" + executable + "\" \"%1\"");
        }
        return true;
    }

    [STAThread]
    public static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        try
        {
            if (args.Length != 1) throw new ArgumentException("Usa Instalar-conector.cmd para configurar la conexión con MATLAB.");
            if (args[0] == "--install")
            {
                if (Configure()) MessageBox.Show(
                    "Conexión lista. Vuelve a LogiVerse y pulsa Abrir MATLAB o Cargar archivo. Confirma Abrir LogiVerse MATLAB cuando lo pida el navegador.",
                    "LogiVerse MATLAB", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 0;
            }
            if (args[0] == "--uninstall")
            {
                using (var key = Registry.CurrentUser.OpenSubKey(ProtocolKey))
                {
                    if (key != null) Registry.CurrentUser.DeleteSubKeyTree(ProtocolKey);
                }
                if (File.Exists(Config)) File.Delete(Config);
                return 0;
            }
            string action = ParseAction(args[0]);
            if (!File.Exists(Config) && !Configure()) return 0;
            string fis = null;
            if (action == "cargar")
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Selecciona el sistema .fis para Fuzzy Logic Designer";
                    dialog.Filter = "Sistema difuso MATLAB (*.fis)|*.fis";
                    dialog.CheckFileExists = true;
                    if (dialog.ShowDialog() != DialogResult.OK) return 0;
                    fis = dialog.FileName;
                }
            }
            Directory.CreateDirectory(Folder);
            Process.Start(StartInfo(File.ReadAllText(Config).Trim(), fis));
            return 0;
        }
        catch (Exception error)
        {
            MessageBox.Show(error.Message, "LogiVerse MATLAB", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }
}
