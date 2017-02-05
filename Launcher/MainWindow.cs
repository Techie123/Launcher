using System;
using Eto.Drawing;
using Eto.Forms;

namespace Launcher
{
    public partial class MainWindow : Form, IView
    {
        public MainWindow()
        {
            CreateContent();
        }

        private void _webview1_DocumentTitleChanged(object sender, WebViewTitleEventArgs e)
        {
            if (e.Title.StartsWith("PlayClicked"))
                Controller.PlayActivated();
            else if (e.Title.StartsWith("CloseClicked"))
                Application.Instance.Quit();
            else if (e.Title.StartsWith("MinimizeClicked"))
                Minimize();
        }

        #region IView

        public void Attach(string html, VersionInfo[] versions)
        {
            _webview1.LoadHtml(html, new Uri("file://" + AppDomain.CurrentDomain.BaseDirectory));
            _webview1.DocumentTitleChanged += _webview1_DocumentTitleChanged;
        }

        public void Invoke(Action action)
        {
            Application.Instance.Invoke(action);
        }

        public void SetPlayText(string text)
        {
            _webview1.ExecuteScript("setplaytext('" + text + "');");
        }

        public void SetProgress(int progress)
        {
            _webview1.ExecuteScript("setprogress(" + progress + ");");
        }

        public void SetStatus(Status status)
        {
            switch (status)
            {
                case Status.Ready:
                    _webview1.ExecuteScript("setplayenabled(true);");
                    break;
                case Status.Downloading:
                case Status.Installing:
                    _webview1.ExecuteScript("setplayenabled(false)");
                    break;
            }
        }

        public void SetStatusText(string text)
        {
            _webview1.ExecuteScript("setstatustext('" + text + "');");
        }

        public void ShowError(string message)
        {
            MessageBox.Show(this, message, MessageBoxType.Error);
        }

        #endregion
    }
}
