using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;
using Newtonsoft.Json;

namespace Launcher
{
    public static class Controller
    {
        private const string GameLocation = "HSMod";
        private const string LauncherLocation = "Launcher";
        private const string UpdateLocation = "Update";
        private const string CacheLocation = "Cache";
        private const string GameVersions = "http://api.hsmod.com/gameversion";
        private const string LauncherVersion = "http://api.hsmod.com/launcherversion";
        private const string Changelog = "http://api.hsmod.com/changelog";

        private static string _basepath, _gamepath, _updatepath;
        private static VersionInfo[] _gameversions;
        private static VersionInfo _launcherversion;
        private static IView _view;
        private static PlayMode _playmode;
        private static Status _status;
        private static int _index, _currentprogress;

        public static void Attach(IView view)
        {
            _view = view;

            var curdir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new char[] { '/', '\\' });
            _basepath = Path.GetDirectoryName(curdir);
            _gamepath = Path.Combine(_basepath, GameLocation);
            _updatepath = Path.Combine(_basepath, UpdateLocation);

            if (Path.GetFileName(curdir) == UpdateLocation)
            {
                // Update self
                var newdir = Path.Combine(_basepath, LauncherLocation);

                while (true)
                {
                    try
                    {
                        if (Directory.Exists(newdir))
                            Directory.Delete(newdir, true);
                        Helper.CopyDirectory(curdir, newdir);
                        break;
                    }
                    catch { }
                }

                if(Global.IsUnix)
                    Process.Start("mono", Path.Combine(newdir, "Launcher.exe"));
                else
                    Process.Start(Path.Combine(newdir, "Launcher.exe"));
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                // Delete old update cache if found
                while (true)
                {
                    try
                    {
                        if (Directory.Exists(_updatepath))
                            Directory.Delete(_updatepath, true);
                        break;
                    }
                    catch { }
                }
            }

            var assembly = typeof(MainWindow).GetTypeInfo().Assembly;
            var html = "";
            var changeloglocation = Path.Combine(curdir, CacheLocation, "changelog.txt");
            var changelogtext = "";
            var changelog = new string[0];
            var ulstarted = false;

            // Read default HTML / CSS / JavaScript
            using (var stream = assembly.GetManifestResourceStream("Launcher.index.html"))
            using (var reader = new StreamReader(stream))
                html = reader.ReadToEnd();

            // Download newest Launcher Info / Game Version Info / Changelog
            try
            {
                var client = new System.Net.WebClient();
                _gameversions = JsonConvert.DeserializeObject<VersionInfo[]>(client.DownloadString(GameVersions));

				_launcherversion = JsonConvert.DeserializeObject<VersionInfo>(client.DownloadString(LauncherVersion));

                if (!Directory.Exists(Path.Combine(curdir, CacheLocation)))
                    Directory.CreateDirectory(Path.Combine(curdir, CacheLocation));

                File.WriteAllText(changeloglocation, client.DownloadString(Changelog));
            }
            catch
            {
                _gameversions = new VersionInfo[1];
                _launcherversion = new VersionInfo();
                _launcherversion.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
                _view.ShowError("Could not download the metadata, starting in offline mode...");

				if (!Directory.Exists(_gamepath) || Directory.GetDirectories(_gamepath).Length == 0)
                {
                    _view.ShowError("No game version found, exiting...");
                    Process.GetCurrentProcess().Kill();
                }
                else
                    _gameversions[0] = new VersionInfo { Version = Directory.GetDirectories(_gamepath)[0] };
            }

            // Try and read changelog
            try
            {
                if (File.Exists(changeloglocation))
                    changelog = File.ReadAllLines(changeloglocation);
            }
            catch { }

            // Formate changelog markdown to HTML
            for (int i = 0; i < changelog.Length; i++)
            {
                var line = changelog[i];
                line = line.Trim(new char[] { ' ', ':' });
                line = line.Replace("---", "");

                if (line.StartsWith("# "))
                    line = "<h1>" + line.Substring(2) + "</h1>";
                else if (line.StartsWith("## "))
                {
                    line = (ulstarted ? "</ul>" : "") + "<h4>" + line.Substring(3) + "</h4>";
                    ulstarted = false;
                }
                else if (line.StartsWith("* "))
                {
                    line = ((!ulstarted) ? "<ul>" : "") + "<li>" + line.Substring(2) + "</li>";
                    if (!ulstarted)
                        ulstarted = true;
                }
                else if (line.StartsWith("!["))
                {
                    var src = line.Remove(line.Length - 1).Split('(')[1];
                    line = "<img src='" + src + "' />";
                }

                changelog[i] = line;
            }

            changelogtext = string.Join("", changelog) + (ulstarted ? "</ul>" : "");

            _status = Status.Ready;
            ReloadPlay(false);

            _currentprogress = 100;
            _index = 0;

            html = html.Replace("$PLAYTEXT", _playmode.ToString()).Replace("$CHANGELOG", changelogtext);
            _view.Attach(html, _gameversions);
        }

        public static void PlayActivated()
        {
            if (_status != Status.Ready)
                return;

            if (_playmode == PlayMode.Install || _playmode == PlayMode.Update)
            {
                var task = new Task(Install);
                task.Start();
            }
            else if (_playmode == PlayMode.Play)
                PlayGame();
        }

        public static void SetSelectedVersion(int index)
        {
            _index = index;
            ReloadPlay();
        }

        private static void Execute(string filepath)
        {
            var game = new Process();

            if (Global.IsLinux)
            {
                // Set game executable permissions
                var chmod = new Process();
                chmod.StartInfo.FileName = "chmod";
                chmod.StartInfo.Arguments = " +x \"" + filepath + "\"";
                chmod.StartInfo.UseShellExecute = false;
                chmod.StartInfo.RedirectStandardOutput = true;
                chmod.Start();
                chmod.StandardOutput.ReadToEnd();

                // Launch game from bash
                game.StartInfo.FileName = "/bin/bash";
                game.StartInfo.Arguments = " -c \"" + filepath + "\"";
                game.StartInfo.UseShellExecute = false;
            }
            else if (Global.IsWindows)
                game.StartInfo.FileName = filepath;

            game.Start();
        }

        private static void Install()
        {
            try
            {
                var launcherurl = _launcherversion.Linux;
                var gameurl = _gameversions[_index].Linux;

                if (Global.IsWindows && Global.Is64bit)
                {
                    launcherurl = _launcherversion.Windows64;
                    gameurl = _gameversions[_index].Windows64;
                }
                else if (Global.IsWindows && !Global.Is64bit)
                {
                    launcherurl = _launcherversion.Windows32;
                    gameurl = _gameversions[_index].Windows32;
                }

                _status = Status.Downloading;
                _view.Invoke(() =>
                {
                    _view.SetStatus(_status);
                    _view.SetProgress(0);
                    _view.SetStatusText("Preparing...");
                });

                var client = new MegaApiClient();
                client.LoginAnonymous();

                if (Assembly.GetEntryAssembly().GetName().Version.ToString() != _launcherversion.Version)
                {
                    _view.Invoke(() => _view.SetStatusText("Downloading Launcher..."));

                    var temppath = Path.GetTempPath() + Guid.NewGuid().ToString();
                    using (var fileStream = new FileStream(temppath, FileMode.Create))
                    using (var downloadStream = new ProgressionStream(client.Download(new Uri(launcherurl)), SetDownloadProgress))
                        downloadStream.CopyTo(fileStream);

                    _status = Status.Installing;
                    _view.Invoke(() =>
                    {
                        _view.SetStatus(_status);
                        _view.SetStatusText("Updating Launcher...");
                    });
                    
                    if (Directory.Exists(_updatepath))
                        Directory.Delete(_updatepath, true);
                    Directory.CreateDirectory(_updatepath);
                    ZipFile.ExtractToDirectory(temppath, _updatepath);

                    File.Delete(temppath);

                    if (Global.IsUnix)
                        Process.Start("mono", Path.Combine(Path.Combine(_basepath, UpdateLocation), "Launcher.exe"));
                    else
                        Process.Start(Path.Combine(Path.Combine(_basepath, UpdateLocation), "Launcher.exe"));
                    Process.GetCurrentProcess().Kill();
                }

                if (!Directory.Exists(Path.Combine(_gamepath, _gameversions[_index].Version)))
                {
                    _view.Invoke(() => _view.SetStatusText("Downloading Game..."));

                    var temppath = Path.GetTempPath() + Guid.NewGuid().ToString();
                    using (var fileStream = new FileStream(temppath, FileMode.Create))
                    using (var downloadStream = new ProgressionStream(client.Download(new Uri(gameurl)), SetDownloadProgress))
                        downloadStream.CopyTo(fileStream);

                    _status = Status.Installing;
                    _view.Invoke(() =>
                    {
                        _view.SetStatus(_status);
                        _view.SetStatusText("Updating Game...");
                    });

                    if (Directory.Exists(_gamepath))
                        Directory.Delete(_gamepath, true);
                    Directory.CreateDirectory(_gamepath);
                    ZipFile.ExtractToDirectory(temppath, Path.Combine(_gamepath, _gameversions[_index].Version));

                    File.Delete(temppath);
                }
            }
            catch(Exception ex)
            {
                _view.Invoke(() => _view.ShowError("An error occured while trying to install the game. More Info:" + Environment.NewLine + ex));
                SetDownloadProgress(100);
            }

            _status = Status.Ready;
            _view.Invoke(() =>
            {
                _view.SetStatus(_status);
                _view.SetStatusText("Ready");
                ReloadPlay();
            });
        }

        private static void PlayGame(string directory = null)
        {
            if (directory == null)
                directory = Path.Combine(_gamepath, _gameversions[_index].Version);

            var files = Directory.GetFiles(directory);
            var filepath = "";

            if (files.Length == 0)
            {
                var dirs = Directory.GetDirectories(directory);

                if (dirs.Length == 0)
                {
                    _view.ShowError("Executable not found");
                    return;
                }

                PlayGame(dirs[0]);
            }

            foreach (var file in files)
            {
                if (
                    (Global.IsLinux && Global.Is64bit && file.EndsWith("x86_64")) ||
                    (Global.IsLinux && !Global.Is64bit && file.EndsWith("x86")) ||
                    (Global.IsWindows && file.EndsWith(".exe"))
                   )
                {
                    filepath = file;
                    break;
                }
            }

            Execute(filepath);
        }

        private static void ReloadPlay(bool reloadview = true)
        {
            if (_status != Status.Ready)
                return;

            var noupdate = Assembly.GetEntryAssembly().GetName().Version.ToString() == _launcherversion.Version;

            if (!Directory.Exists(_gamepath) || Directory.GetDirectories(_gamepath).Length == 0)
                _playmode = PlayMode.Install;
            else if (!Directory.Exists(Path.Combine(_gamepath, _gameversions[_index].Version)) || !noupdate)
                _playmode = PlayMode.Update;
            else
                _playmode = PlayMode.Play;

            if (reloadview)
                _view.SetPlayText(_playmode.ToString());
        }

        private static void SetDownloadProgress(int progress)
        {
            if (_currentprogress != progress)
            {
                _currentprogress = progress;
                _view.Invoke(() => _view.SetProgress(progress));
            }
        }
    }
}
