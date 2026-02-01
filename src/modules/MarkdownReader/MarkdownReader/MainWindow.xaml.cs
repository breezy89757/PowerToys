// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace MarkdownReader
{
    public sealed partial class MainWindow : WindowEx
    {
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

            string sampleMarkdown = @"
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
";

            string html = Helpers.MarkdownParser.ParseMarkdown(sampleMarkdown, "C:\\FakePath\\Demo.md");
            MarkdownWebView.NavigateToString(html);
        }
    }
}
