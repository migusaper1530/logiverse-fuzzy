using System;
using System.Diagnostics;
using System.IO;

public static class ConnectorTests
{
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
    }

    public static int Main(string[] args)
    {
        try
        {
            Check(MatlabConnector.ParseAction("logiverse-matlab://fuzzy") == "fuzzy", "Blank designer action");
            Check(MatlabConnector.ParseAction("logiverse-matlab://cargar/") == "cargar", "File picker action");
            foreach (string invalid in new[] { "https://fuzzy", "logiverse-matlab://anything", "logiverse-matlab://fuzzy/path",
                "logiverse-matlab://fuzzy?command=quit", "logiverse-matlab://fuzzy#quit", "logiverse-matlab://x@fuzzy",
                "logiverse-matlab://fuzzy:5000", "logiverse-matlab://cargar?file=C:/secret.fis", "--install extra" })
            {
                bool rejected = false;
                try { MatlabConnector.ParseAction(invalid); } catch (ArgumentException) { rejected = true; }
                Check(rejected, "Rejected untrusted URI: " + invalid);
            }
            string folder = Path.Combine(Path.GetTempPath(), "LogiVerse prueba ' & ñ con espacios " + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            try
            {
                string fis = Path.Combine(folder, "sistema.fis");
                File.WriteAllText(fis, "[System]");
                string code = MatlabConnector.BuildMatlabCommand(fis);
                Check(code.Contains("prueba '' & ñ con espacios"), "MATLAB quotes apostrophes in a local file path");
                Check(code.Contains("readfis('") && code.Contains("fuzzyLogicDesigner(fis)"), "Selected FIS opens in designer");
                Check(MatlabConnector.BuildMatlabCommand(null).Contains("fuzzyLogicDesigner;"), "Blank designer command");
                string executable = Path.GetFullPath(args[0]);
                string output = Path.Combine(folder, "arguments.txt");
                Environment.SetEnvironmentVariable("LOGIVERSE_TEST_ARGUMENTS", output);
                ProcessStartInfo start = MatlabConnector.StartInfo(executable, fis);
                start.WorkingDirectory = folder;
                Check(!start.UseShellExecute, "No shell for MATLAB commands");
                using (var process = Process.Start(start))
                {
                    Check(process.WaitForExit(10000), "Native process finished");
                    Check(process.ExitCode == 0, "Native process succeeded");
                }
                string[] received = File.ReadAllLines(output);
                Check(received.Length == 3 && received[0] == "-wait" && received[1] == "-r" && received[2] == code,
                    "MATLAB receives one complete command with spaces and apostrophes preserved");
            }
            finally { Directory.Delete(folder, true); }
            Console.WriteLine("Windows connector tests passed.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
