using System;
using Eto.Forms;
using Eto.Drawing;

namespace Launcher
{
    public partial class MainWindow : Form
    {
		private DynamicLayout _layout1;
        private WebView _webview1;
        private Label _label1;
        private ProgressBar _progress1;
        private DropDown _combo1;
        private Button _buttonPlay, _buttonSettings;

        private void CreateContent()
        {
            Title = "Hearthstone Mod Launcher";
            Width = 900;
            Height = 500;
            Resizable = false;

			_layout1 = new DynamicLayout();

            // Background web view
            _webview1 = new WebView();
            _webview1.Width = Width;
            _webview1.Height = Height;
			_layout1.Add(_webview1, true, true);

            // Foreground controls
            var center1 = new CenterControl();
            center1.DefaultPadding = new Padding(4);
            center1.Width = Width;

            var dynamic1 = new DynamicLayout();
            dynamic1.DefaultSpacing = new Size(10, 0);
            dynamic1.BeginHorizontal();

            // Downloading section
            var center2 = new CenterControl();
            _label1 = new Label();
            _label1.Text = "Ready";
            center2.Add(_label1);
            _progress1 = new ProgressBar();
            _progress1.MinValue = 0;
            _progress1.MaxValue = 100;
            _progress1.Value = 100;
            center2.Add(_progress1);
            center2.Finish();
            dynamic1.Add(center2, true, true);

            // Play section
            var center3 = new CenterControl();

            var hbox = new DynamicLayout();
            hbox.DefaultSpacing = new Size(4, 0);
            hbox.BeginHorizontal();

            _combo1 = new DropDown();
            _combo1.Items.Add("Latest");
            _combo1.SelectedIndex = 0;
            hbox.Add(_combo1, true);

            _buttonSettings = new Button();
            _buttonSettings.Image = Bitmap.FromResource("Launcher.Resources.Settings.png");
            _buttonSettings.ImagePosition = ButtonImagePosition.Overlay;
            _buttonSettings.Width = 0;
            // TODO: Add some settings
            // hbox.Add(_buttonSettings);

            center3.Add(hbox);

            _buttonPlay = new Button();
            _buttonPlay.Width = 150;
            _buttonPlay.Text = "Install";
            center3.Add(_buttonPlay);

            center3.Finish();
            dynamic1.Add(center3, false, true);

            center1.Add(dynamic1);

            center1.Finish();
			//_layout1.Add(center1, true, false);

            Content = _layout1;
        }
    }
}
