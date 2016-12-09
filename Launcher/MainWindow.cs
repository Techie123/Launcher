using System;
using System.IO;
using System.Reflection;
using Eto.Forms;

namespace Launcher
{
    public partial class MainWindow : Form, IView
    {
        public MainWindow()
        {
            CreateContent();
        }

        public void Attach(string changelog, VersionInfo[] versions)
        {
            var html = "";
            var assembly = typeof(MainWindow).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream("Launcher.index.html"))
                using (var reader = new StreamReader(stream))
                    html = reader.ReadToEnd();

            _webview1.LoadHtml(html.Replace("$CHANGELOG", changelog), new Uri("file://" + AppDomain.CurrentDomain.BaseDirectory));

            _combo1.Items.Clear();
            _combo1.Items.Add("Latest");
            foreach (var version in versions)
                _combo1.Items.Add(version.Version);
            _combo1.SelectedIndex = 0;

            _combo1.SelectedIndexChanged += (sender, e) => Controller.SetSelectedVersion(_combo1.SelectedIndex == 0 ? 0 : _combo1.SelectedIndex - 1);
            _buttonPlay.Click += (sender, e) => Controller.PlayActivated();
        }

        public void Invoke(Action action)
        {
            Application.Instance.Invoke(action);
        }

        public void SetPlayMode(PlayMode mode)
        {
            if (mode == PlayMode.Play)
                _buttonPlay.Text = "Play";
            else if (mode == PlayMode.Install)
                _buttonPlay.Text = "Install";
        }

        public void SetProgress(int progress)
        {
            _progress1.Value = progress;
        }

        public void SetStatus(Status status)
        {
            switch (status)
            {
                case Status.Ready:
                    _buttonPlay.Enabled = true;
                    _combo1.Enabled = true;
                    break;
                case Status.Downloading:
                case Status.Installing:
                    _buttonPlay.Enabled = false;
                    _combo1.Enabled = false;
                    break;
            }
        }

        public void SetStatusText(string text)
        {
            _label1.Text = text;
        }

        public void ShowError(string message)
        {
            MessageBox.Show(this, message, MessageBoxType.Error);
        }
    }
}
