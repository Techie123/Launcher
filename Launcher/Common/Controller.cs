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
        private const string GameVersions = "http://api.hsmod.com/gameversion";
        private const string Changelog = "http://api.hsmod.com/changelog";

        private static string _gamepath;
        private static VersionInfo[] _versions;
        private static IView _view;
        private static PlayMode _playmode;
        private static Status _status;
        private static int _index, _currentprogress;

        public static void Attach(IView view)
        {
            _view = view;

            _gamepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GameLocation);
            if (!Directory.Exists(_gamepath))
                Directory.CreateDirectory(_gamepath);

            var changelogtext = "";
            var html = "";
            var assembly = typeof(MainWindow).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream("Launcher.index.html"))
            using (var reader = new StreamReader(stream))
                html = reader.ReadToEnd();

            try
            {
                var client = new System.Net.WebClient();
                var changelog = new string[0];
                var ulstarted = false;

                _versions = JsonConvert.DeserializeObject<VersionInfo[]>(client.DownloadString(GameVersions));
                changelog = client.DownloadString(Changelog).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Formate Markdown to HTML
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
            }
            catch
            {
                _view.ShowError("Could not download the metadata, starting in offline mode...");

                _versions = new VersionInfo[0];
            }

            _status = Status.Ready;
            ReloadPlay(false);

            _currentprogress = 100;
            _index = 0;

            html = html.Replace("$PLAYTEXT", _playmode.ToString()).Replace("$CHANGELOG", changelogtext);
            _view.Attach(html, changelogtext, _versions);
        }

        public static void PlayActivated()
        {
            if (_status != Status.Ready)
                return;

            if (_playmode == PlayMode.Install)
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

            if (Global.Linux)
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
            else if (Global.Windows)
                game.StartInfo.FileName = filepath;

            game.Start();
        }

        private static void Install()
        {
            try
            {
                var url = _versions[_index].Linux;

                if (Global.Windows && Global.Is64bit)
                    url = _versions[_index].Windows64;
                else if (Global.Windows && !Global.Is64bit)
                    url = _versions[_index].Windows32;

                _status = Status.Downloading;
                _view.Invoke(() =>
                {
                    _view.SetStatus(_status);
                    _view.SetProgress(0);
                    _view.SetStatusText("Preparing...");
                });

                var client = new MegaApiClient();
                client.LoginAnonymous();

                var temppath = Path.GetTempPath() + Guid.NewGuid().ToString();

                _view.Invoke(() => _view.SetStatusText("Downloading..."));
                using (var fileStream = new FileStream(temppath, FileMode.Create))
                using (var downloadStream = new ProgressionStream(client.Download(new Uri(url)), SetDownloadProgress))
                    downloadStream.CopyTo(fileStream);

                _status = Status.Installing;
                _view.Invoke(() =>
                {
                    _view.SetStatus(_status);
                    _view.SetStatusText("Installing...");
                });

                ZipFile.ExtractToDirectory(temppath, Path.Combine(_gamepath, _versions[_index].Version));
                File.Delete(temppath);
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
                directory = Path.Combine(_gamepath, _versions[_index].Version);

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
                    (Global.Linux && Global.Is64bit && file.EndsWith("x86_64")) ||
                    (Global.Linux && !Global.Is64bit && file.EndsWith("x86")) ||
                    (Global.Windows && file.EndsWith(".exe"))
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

            if (Directory.Exists(Path.Combine(_gamepath, _versions[_index].Version)))
                _playmode = PlayMode.Play;
            else
                _playmode = PlayMode.Install;

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
