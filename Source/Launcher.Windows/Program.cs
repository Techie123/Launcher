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
            Global.IsWindows = true;
            Global.Is64bit = Environment.Is64BitOperatingSystem;

            var app = new Application(Platform.Detect);
            var win = new MainWindow();
            Controller.Attach(win);

            app.Run(win);
        }
    }
}
