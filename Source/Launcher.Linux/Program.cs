﻿using System;
using Eto;
using Eto.Forms;

namespace Launcher.Linux
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Global.IsLinux = true;
            Global.IsUnix = true;
            Global.Is64bit = Environment.Is64BitOperatingSystem;

            var app = new Application(Platform.Detect);
            var win = new MainWindow();
            Controller.Attach(win);

            app.Run(win);
        }
    }
}
