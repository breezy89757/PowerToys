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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MarkdownReader
{
    public sealed partial class MainWindow : WindowEx
    {
        private record struct ParsedData(string Html, string Title, List<Models.TocItem> Toc);

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
            // 1. Start WebView2 Initialization (IO/IPC bound)
            // We capture the operation but don't await immediately, allowing concurrency.
            var webViewInit = MarkdownWebView.EnsureCoreWebView2Async();

            // 2. Start Content Loading & Parsing (IO/CPU bound)
            // This runs in parallel with WebView initialization.
            var contentTask = LoadContentAsync();

            try
            {
                // 3. Wait for both to complete
                await webViewInit;
                var data = await contentTask;

                // 4. Update UI
                ApplyParsedData(data);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Startup initialization failed: {ex.Message}");
            }
        }

        private async Task<ParsedData> LoadContentAsync()
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                return await LoadMarkdownFromFileAsync(currentFilePath);
            }
            else
            {
                return LoadWelcomePage();
            }
        }

        private ParsedData LoadWelcomePage()
        {
            string markdown = @"
# Welcome to PowerToys Markdown Reader

This is a persistent viewer with:
* **Dynamic TOC** navigation
* **Mermaid.js** support
* **Shell integration** (Right-click to open)

## How to use
1. Drag a Markdown file here (Future feature)
2. Or right-click a `.md` file in Explorer and select Open.
";
            
            // For the welcome page, parsing is fast enough to do synchronously, 
            // but we use the shared helper for consistency.
            return ParseMarkdownContent(markdown, "Welcome", "Markdown Reader - Welcome");
        }

        private async Task<ParsedData> LoadMarkdownFromFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Logger.LogError($"File not found: {filePath}");
                    return LoadWelcomePage();
                }

                // Offload file reading and parsing to background thread to avoid blocking UI
                // while WebView is initializing.
                return await Task.Run(async () =>
                {
                    string markdown = await File.ReadAllTextAsync(filePath);
                    string fileName = Path.GetFileName(filePath);
                    string title = $"{fileName} - Markdown Reader";
                    
                    return ParseMarkdownContent(markdown, filePath, title);
                });
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading file {filePath}: {ex.Message}");
                var fallback = LoadWelcomePage();
                // Override title to show error
                return fallback with { Title = "Markdown Reader - Error Loading File" };
            }
        }

        private ParsedData ParseMarkdownContent(string markdown, string filePath, string title)
        {
            // CPU-bound work
            var toc = Helpers.MarkdownParser.ExtractTableOfContents(markdown);
            string html = Helpers.MarkdownParser.ParseMarkdown(markdown, filePath);
            
            return new ParsedData(html, title, toc);
        }

        private void ApplyParsedData(ParsedData data)
        {
            // Update Title
            this.Title = data.Title;

            // Update TOC
            TocItems.Clear();
            foreach (var item in data.Toc)
            {
                TocItems.Add(item);
            }

            // Render content
            MarkdownWebView.NavigateToString(data.Html);
            
            // Keep track for potential reloads (optional, based on original logic)
            // currentMarkdown = ...; // If needed for other features
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
