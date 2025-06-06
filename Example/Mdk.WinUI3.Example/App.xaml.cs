﻿using System;
using System.IO;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mdk.WinUI3.Example
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += (sender, e) =>
            {
                var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Mdk.WinUI3.Example.Crash.log");
                using var writer = new StreamWriter(logPath, true);
                writer.WriteLine($"[{DateTime.Now}]");
                writer.WriteLine($"Message: {e.Exception.Message}");
                writer.WriteLine($"StackTrace: ");
                writer.WriteLine(e.Exception.StackTrace);
            };
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window? m_window;
    }
}
