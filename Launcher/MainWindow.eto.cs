using Eto.Forms;
using Eto.Drawing;

namespace Launcher
{
    public partial class MainWindow : Form
    {
        private Panel _panelTitlebar;
        private PixelLayout _pixel1;
        private WebView _webview1;

        private void CreateContent()
        {
            Title = "Hearthstone Mod Launcher";
            Icon = Icon.FromResource("Launcher.Resources.Logo.ico");
            Width = 900;
            Height = 500;
            Resizable = false;
            WindowStyle = WindowStyle.None;

            _pixel1 = new PixelLayout();

            _webview1 = new WebView();
            _webview1.Width = Width;
            _webview1.Height = Height;
            _pixel1.Add(_webview1, 0, 0);

            _panelTitlebar = new Panel();
            _panelTitlebar.Width = Width - 2 * (16 + 5) - 10;
            _panelTitlebar.Height = 30;
            _panelTitlebar.MouseDown += PanelTitlebar_MouseDown;
            _panelTitlebar.MouseUp += PanelTitlebar_MouseUp;
            _panelTitlebar.MouseMove += PanelTitlebar_MouseMove;
            _pixel1.Add(_panelTitlebar, 0, 0);

            Content = _pixel1;
        }
    }
}
