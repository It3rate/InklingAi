namespace AIEnv.Clients
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Timers;
    using System.Runtime.InteropServices;
    using System.IO.Compression;
    using AIEnv.Utils;
    using AIEnv.Clients.StableDiffusion;

    public class VEnv
    {
        private static readonly VEnv _venv = new VEnv();
        public static VEnv Instance => _venv;

        // requires finding python install
        private string VenvPath = IOUtils.DataFolder + @".venv" + Path.DirectorySeparatorChar;
        private string PythonMain => @"Main.py";

        private bool _isLoading = false;
        private Process? _venvProcess;
        private StableDiffusionClient? _client;
        private Timer IsAliveTimer = new Timer();
        public VEnv()
        {
            IsAliveTimer.Interval = 3000;
            IsAliveTimer.Elapsed += IsAliveTimer_Tick;
        }

        public void Activate(StableDiffusionClient client, bool forceActivate = false)
        {
            _client = client;
            IsAliveTimer.Start();

            Task.Run(async () =>
            {
                var alreadyRunning = await _client.IsAlive();
                if (!alreadyRunning)
                {
                    if (!_isLoading && (Properties.AIEnv.Default.AutoStartGenerator || forceActivate))
                    {
                        _isLoading = true;
                        EnsureStableDiffusionService();
                        while (pythonRestarts > 0)
                        {
                            Close(true);
                            EnsureStableDiffusionService();
                        }
                        _isLoading = false;
                    }
                    else
                    {
                        Trace.WriteLine("Back end manually turned off.");
                    }
                }
            });
        }

        private async void IsAliveTimer_Tick(object? sender, EventArgs e)
        {
            if (_client != null)
            {
                bool isAlive = await _client.IsAlive();
                if (isAlive)
                {
                    IsAliveTimer.Stop();
                    EnsureModelsLoaded();
                }
            }
            else
            {
                IsAliveTimer.Stop();
            }
        }

        private bool _hasClosed = false;
        private int pythonRestarts = 2;
        public void Close(bool forceExit = false)
        {
            if (Properties.AIEnv.Default.CloseGeneratorOnExit || forceExit)
            {
                if (_consoleWriter != null)
                {
                    _consoleWriter.Close();
                    _consoleWriter.Dispose();
                }
                if (_logWriter != null)
                {
                    _logWriter.Close();
                    _logWriter.Dispose();
                }
                if (_venvProcess != null && !_hasClosed)
                {
                    try
                    {
                        if (!_venvProcess.HasExited)
                        {
                            SendMessage(_venvProcess.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
                _isLoading = false;
                _hasClosed = true;
                _venvProcess = null;
            }
        }

        private StreamWriter? _logWriter;
        private StreamWriter? _consoleWriter;

        private void WriteToConsoleAndLog(string msg)
        {
            _logWriter?.WriteLine(msg);
            _logWriter?.Flush();
            _consoleWriter?.WriteLine(msg);
            _consoleWriter?.Flush();
        }

        private int EnsureStableDiffusionService()
        {
            int result = 0;
            bool redirectOutput = false;
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = redirectOutput,
                RedirectStandardError = redirectOutput,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                Arguments = "python",
                FileName = "cmd.exe",
                //Verb = "runas", // This will prompt the user with UAC
                WorkingDirectory = IOUtils.CurrentDirectory,
            };

            using (_venvProcess = Process.Start(startInfo))
            {
                if (_venvProcess != null)
                {
                    ShowWindowAsync(_venvProcess.MainWindowHandle, SW_SHOWMINIMIZED); // no effect if not using useshellexecute
                    _venvProcess.EnableRaisingEvents = true;


                    //using var sw = _venvProcess.StandardInput;
                    _consoleWriter = _venvProcess.StandardInput;
                    _logWriter = new StreamWriter(IOUtils.SDLogFilePath, false);
                    if (_consoleWriter.BaseStream.CanWrite)
                    {
                        if (redirectOutput)
                        {
                            _venvProcess.OutputDataReceived += (sender, e) =>
                            {
                                if (!string.IsNullOrEmpty(e.Data))
                                {
                                    var s = e.Data;
                                    _logWriter?.WriteLine(s);
                                }
                            };

                            _venvProcess.ErrorDataReceived += (sender, e) =>
                            {
                                if (!string.IsNullOrEmpty(e.Data))
                                {
                                    _logWriter?.WriteLine(e.Data);
                                }
                            };
                            _venvProcess.BeginOutputReadLine();
                            _venvProcess.BeginErrorReadLine();
                        }

                        WriteToConsoleAndLog(IOUtils.DataDrive);

                        WriteToConsoleAndLog($"if not exist \"{IOUtils.DataFolder}\" mkdi \"{IOUtils.DataFolder}\"");
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.DataFolder}python\" mkdir \"{IOUtils.DataFolder}python\"");
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.SDDataFolder}\" mkdir \"{IOUtils.SDDataFolder}\"");
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.SDModelsFolder}\" mkdir \"{IOUtils.SDModelsFolder}\"");
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.SDWorkingFolder}\" mkdir \"{IOUtils.SDWorkingFolder}\"");
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.SDImageFolder}\" mkdir \"{IOUtils.SDImageFolder}\"");
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.SDUploadsFolder}\" mkdir \"{IOUtils.SDUploadsFolder}\"");

                        // Add folders for embedding parsing (copy test files if needed)
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.DataFolder}{IOUtils.TextRoot}\" mkdir \"{IOUtils.DataFolder}{IOUtils.TextRoot}\"");
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.DataFolder}{IOUtils.TextData}\" mkdir \"{IOUtils.DataFolder}{IOUtils.TextData}\"");
                        WriteToConsoleAndLog($"if not exist \"{IOUtils.DataFolder}{IOUtils.TextParsed}\" mkdir \"{IOUtils.DataFolder}{IOUtils.TextParsed}\"");

                        ShowWindowAsync(_venvProcess.MainWindowHandle, SW_SHOWMINIMIZED);

                        if (VenvPath == "" || !Directory.Exists(VenvPath))
                        {
                            WriteToConsoleAndLog($"cd \"{IOUtils.DataFolder}\"");

                            // copy models if there are any (may only deploy in publish file)
                            //var modelSource = $"\"{IOUtils.CurrentDirectory}{IOUtils.SDModelFromRoot}\"";
                            //var modelDest = $"\"{IOUtils.RootFolder}{IOUtils.SDModelFromRoot}\"";
                            // /M to move, these can be large so avoid having 2 copies
                            // todo: eventually move all python to own open repo and install from github
                            //WriteToConsoleAndLog($"if exist {modelSource} xcopy {modelSource} {modelDest} /E /C /I /Y /M");

                            var pythonPath = PythonUtils.FindPython();
                            if (pythonPath == null || pythonPath.StartsWith("**"))
                            {
                                _logWriter?.WriteLine(pythonPath);
                                _logWriter?.WriteLine($"Installing python version {PythonUtils.PythonVersion}.");
                                var installResponse = Task.Run(() => PythonUtils.InstallPython()).GetAwaiter().GetResult();
                                var response = installResponse?.ToString() ?? "Error installing.";

                                pythonPath = PythonUtils.FindPython();
                                if (pythonPath == null || pythonPath.StartsWith("**"))
                                {
                                    // try default location as path is probably not updated.
                                    var defaultLocation = "C:\\Program Files\\Python310\\python.exe";
                                    if (File.Exists(defaultLocation))
                                    {
                                        pythonPath = defaultLocation;
                                    }
                                    else
                                    {
                                        //_logWriter?.WriteLine($"Couldn't find Python. Try running again. {response}.");
                                        _logWriter?.Flush();
                                        return pythonRestarts--;
                                    }
                                }
                                else
                                {
                                    _logWriter?.WriteLine($"Local python version installed to {pythonPath}.");
                                }
                                return pythonRestarts--;
                            }

                            WriteToConsoleAndLog(@"pip install virtualenv");
                            WriteToConsoleAndLog($"\"{pythonPath}\" -m venv .venv");
                        }



                        if (pythonRestarts > 1) return pythonRestarts--;


                        pythonRestarts = 0;
                        WriteToConsoleAndLog($"cd \"{IOUtils.DataFolder}\"");
                        Directory.SetCurrentDirectory(IOUtils.DataFolder);

                        // always copy python files as they may be updated. May want a setting to turn this off if user edits them?
                        //WriteToConsoleAndLog($"xcopy \"{IOUtils.CurrentDirectory}python\" \"{IOUtils.RootFolder}python\" /E /C /I /Y");
                        ExtractAndOverwrite($"{IOUtils.CurrentDirectory}python.zip", $"{IOUtils.DataFolder}");

                        WriteToConsoleAndLog(@".venv\Scripts\activate.bat");

                        WriteToConsoleAndLog(@"pip install --no-cache-dir -r python\requirements.txt");

                        WriteToConsoleAndLog(@"python python/" + PythonMain);
                    }
                    _venvProcess.WaitForExit();
                    Close();
                }
            }
            return result;
        }


        private void EnsureModelsLoaded()
        {

            //Task.Run(async () =>
            //{
            //    await InklingAI.GenInstance.EnsureModelsLoaded();
            //});

        }
        public static void ExtractAndOverwrite(string zipPath, string extractPath)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var filename = Path.GetFileName(entry.FullName);
                        if (filename != "")
                        {
                            string completeFileName = Path.Combine(extractPath, entry.FullName);

                            string? directory = Path.GetDirectoryName(completeFileName);
                            if (directory != null && !Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            entry.ExtractToFile(completeFileName, true); // overwrite = true

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const int SW_HIDE = 0;
        const int SW_SHOWMINIMIZED = 2;
        const uint WM_CLOSE = 0x0010;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        public static void SendWindowToBack(IntPtr hWnd)
        {
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();




    }

    class PythonUtils
    {
        public static string PythonVersion = "3.10.11";
        public async static Task<string> InstallPython()
        {
            string result;
            var installerPath = IOUtils.DataFolder + "pythonInstaller" + IOUtils.Sep;
            Directory.CreateDirectory(installerPath);
            string installerUrl = $"https://www.python.org/ftp/python/{PythonVersion}/python-{PythonVersion}-amd64.exe";
            string pythonInstallDownload = $"{installerPath}pythonInstaller.exe";


            var client = new HttpClient();
            byte[] data = await client.GetByteArrayAsync(installerUrl);
            await File.WriteAllBytesAsync(pythonInstallDownload, data);

            int exitCode = -1;
            Process process = new Process();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = pythonInstallDownload,
                    Arguments = $"InstallAllUsers=1 PrependPath=1",
                    UseShellExecute = true,
                };

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Always close the process even if an error occurs.
                if (!process.HasExited)
                    process.Close();
            }
            Directory.Delete(installerPath, true);
            switch (exitCode)
            {
                case 0:
                    result = "Success";
                    var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
                    if (path != null)
                    {
                        Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
                    }

                    break;
                case 1602:
                    result = "** User cancelled python install.";
                    break;
                default:
                    result = $"** Error installing local copy of python.\nPlease try installing python {PythonVersion} at {installerUrl}.\nBe sure to check 'Add python.exe to PATH.' on the first screen.\nThen try rerunning this application.";
                    break;
            }
            return result;
        }

        public static string? FindPython()
        {
            string pythonCommand = "where python";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();

            process.StandardInput.WriteLine(pythonCommand);
            process.StandardInput.WriteLine("exit");
            string output = process.StandardOutput.ReadToEnd();

            string[] pythonPaths = output.Trim().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string? correctPythonPath = null;
            bool startPaths = false;
            foreach (string pythonPath in pythonPaths)
            {
                if (startPaths)
                {
                    process.Start();
                    var pth = $"\"{pythonPath}\" --version";
                    process.StandardInput.WriteLine(pth);
                    process.StandardInput.WriteLine("exit");
                    process.WaitForExit();

                    output = process.StandardOutput.ReadToEnd();

                    if (output.Length > 0)
                    {
                        string versionLine = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(line => line.StartsWith("Python"));
                        if (versionLine == null)
                        {
                            continue;
                        }

                        string version = versionLine.Split(' ')[1].Trim();
                        string[] versionParts = version.Split('.');
                        if (versionParts.Length > 1)
                        {
                            if (versionParts[0] == "3" && double.TryParse(versionParts[1], out double versionNumber) &&
                                versionNumber >= 7 && versionNumber < 11) // pytorch doesn't support python 3.11 as of this writing
                            {
                                correctPythonPath = pythonPath;
                                break;
                            }
                        }

                    }
                }
                else if (pythonPath.EndsWith(pythonCommand))
                {
                    startPaths = true;
                }
                else if (pythonPath.EndsWith("exit"))
                {
                    break;
                }
            }

            if (correctPythonPath == null)
            {
                correctPythonPath = "** Error: Could not find a Python version greater than 3.8 on the system PATH.";
            }

            process.Close();
            return correctPythonPath;
        }

    }
}
