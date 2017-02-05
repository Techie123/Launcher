using Eto.Forms;
using Eto.Drawing;

namespace Launcher
{
    public partial class MainWindow : Form
    {
        private Panel _panelTitlebar;
        private WebView _webview1;

        private void CreateContent()
        {
            Title = "Hearthstone Mod Launcher";
            Icon = Icon.FromResource("Launcher.Resources.Logo.ico");
            Width = 900;
            Height = 500;
            Resizable = false;
            //WindowStyle = WindowStyle.None;

            _webview1 = new WebView();
            _webview1.Width = Width;
            _webview1.Height = Height;

            Content = _webview1;
        }
    }
}
