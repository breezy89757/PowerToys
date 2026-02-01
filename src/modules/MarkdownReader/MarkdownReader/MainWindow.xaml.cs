// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace MarkdownReader
{
    public sealed partial class MainWindow : WindowEx
    {
        public ObservableCollection<Models.TocItem> TocItems { get; } = new ObservableCollection<Models.TocItem>();

        private string currentMarkdown;

        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await MarkdownWebView.EnsureCoreWebView2Async();

            currentMarkdown = @"
# Markdown Reader Demo

This is a **bold** text and *italic* text.

## Mermaid Diagram

```mermaid
graph TD;
    A[Markdown] -->|Parse| B(HTML);
    B -->|Inject| C{Mermaid.js};
    C -->|Render| D[WebView2];
```

## Table

| Header 1 | Header 2 |
| --- | --- |
| Code | Value |
| A | 1 |
| B | 2 |

## Features

### Sub Feature 1

This is a sub feature.

### Sub Feature 2

Another sub feature with more content.

## Conclusion

That's all for now!
";

            LoadMarkdown(currentMarkdown);
        }

        private void LoadMarkdown(string markdown)
        {
            TocItems.Clear();
            var toc = Helpers.MarkdownParser.ExtractTableOfContents(markdown);
            foreach (var item in toc)
            {
                TocItems.Add(item);
            }

            string html = Helpers.MarkdownParser.ParseMarkdown(markdown, "C:\\FakePath\\Demo.md");
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
