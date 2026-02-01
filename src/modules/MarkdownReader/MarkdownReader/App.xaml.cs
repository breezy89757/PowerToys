// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using ManagedCommon;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace MarkdownReader
{
    public partial class App : Application
    {
        private Window mainWindow;

        public static string AppFilename { get; set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            string[] cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs != null && cmdArgs.Length >= 2)
            {
                AppFilename = cmdArgs[1];
            }

            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
