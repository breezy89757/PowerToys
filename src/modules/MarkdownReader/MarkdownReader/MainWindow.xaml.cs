// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.IO;
using ManagedCommon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace MarkdownReader
{
    public sealed partial class MainWindow : WindowEx
    {
        public ObservableCollection<Models.TocItem> TocItems { get; } = new ObservableCollection<Models.TocItem>();

        private string currentMarkdown;
        private string currentFilePath;

        public MainWindow()
            : this(null)
        {
        }

        public MainWindow(string filePath)
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);

            currentFilePath = filePath;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await MarkdownWebView.EnsureCoreWebView2Async();

            // If a file path was provided, load it; otherwise, show demo content
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                LoadMarkdownFromFile(currentFilePath);
            }
            else
            {
                LoadWelcomePage();
            }
        }

        private void LoadWelcomePage()
        {
            currentMarkdown = @"
# Welcome to PowerToys Markdown Reader

This is a persistent viewer with:
* **Dynamic TOC** navigation
* **Mermaid.js** support
* **Shell integration** (Right-click to open)

## How to use
1. Drag a Markdown file here (Future feature)
2. Or right-click a `.md` file in Explorer and select Open.
";

            this.Title = "Markdown Reader - Welcome";
            LoadMarkdown(currentMarkdown, "Welcome");
        }

        private void LoadMarkdownFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Logger.LogError($"File not found: {filePath}");
                    LoadWelcomePage();
                    return;
                }

                // Read the markdown file
                currentMarkdown = File.ReadAllText(filePath);
                currentFilePath = filePath;

                // Update window title with filename
                string fileName = Path.GetFileName(filePath);
                this.Title = $"{fileName} - Markdown Reader";

                // Load the markdown content
                LoadMarkdown(currentMarkdown, filePath);

                Logger.LogInfo($"Successfully loaded file: {filePath}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading file {filePath}: {ex.Message}");

                // Show error and fall back to demo content
                LoadWelcomePage();
                this.Title = "Markdown Reader - Error Loading File";
            }
        }

        private void LoadMarkdown(string markdown, string filePath)
        {
            TocItems.Clear();
            var toc = Helpers.MarkdownParser.ExtractTableOfContents(markdown);
            foreach (var item in toc)
            {
                TocItems.Add(item);
            }

            string html = Helpers.MarkdownParser.ParseMarkdown(markdown, filePath);
            MarkdownWebView.NavigateToString(html);
        }

        private async void TocListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TocListView.SelectedItem is Models.TocItem selectedItem)
            {
                string script = $"document.getElementById('{selectedItem.Id}')?.scrollIntoView({{ behavior: 'smooth', block: 'start' }});";
                await MarkdownWebView.CoreWebView2.ExecuteScriptAsync(script);
            }
        }
    }
}
