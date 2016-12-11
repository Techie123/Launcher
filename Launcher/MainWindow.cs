using System;
using System.IO;
using System.Reflection;
using Eto.Forms;

namespace Launcher
{
    public partial class MainWindow : Form, IView
    {
        private const int ProgressWidth = 675;

        public MainWindow()
        {
            CreateContent();
        }

        public void Attach(string html, string changelog, VersionInfo[] versions)
        {
            _webview1.LoadHtml(html, new Uri("file://" + AppDomain.CurrentDomain.BaseDirectory));

            _combo1.Items.Clear();
            _combo1.Items.Add("Latest");
            foreach (var version in versions)
                _combo1.Items.Add(version.Version);
            _combo1.SelectedIndex = 0;

            _webview1.DocumentTitleChanged += _webview1_DocumentTitleChanged;
        }

        private void _webview1_DocumentTitleChanged(object sender, WebViewTitleEventArgs e)
        {
            if (e.Title.StartsWith("PlayClicked"))
                Controller.PlayActivated();
        }

        public void Invoke(Action action)
        {
            Application.Instance.Invoke(action);
        }

        public void SetPlayText(string text)
        {
            _webview1.ExecuteScript("document.getElementById('playbutton').innerHTML = '" + text + "';");
        }

        public void SetProgress(int progress)
        {
            int width = ProgressWidth * 675 / 100;

            _webview1.ExecuteScript("document.getElementById('progressbar').style.width = '" + width + "px';");
            _webview1.ExecuteScript("document.getElementById('progresstext').innerHTML = '" + progress + "%';");
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
            _webview1.ExecuteScript("document.getElementById('statustext').innerHTML = '" + text + "';");
        }

        public void ShowError(string message)
        {
            MessageBox.Show(this, message, MessageBoxType.Error);
        }
    }
}
