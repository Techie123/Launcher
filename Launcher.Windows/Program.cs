using Eto;
using Eto.Forms;
using System;

namespace Launcher.Windows
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Global.Windows = true;
            Global.Is64bit = Environment.Is64BitOperatingSystem;

			var app = new Application(Platforms.Wpf);
            var win = new MainWindow();
            Controller.Attach(win);

            app.Run(win);
        }
    }
}
