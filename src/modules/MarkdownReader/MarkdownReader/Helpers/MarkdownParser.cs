// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.PowerToys.FilePreviewCommon;

namespace MarkdownReader.Helpers
{
    public static class MarkdownParser
    {
        public static string ParseMarkdown(string markdownContent, string filePath)
        {
            string theme = "light";
            try
            {
                var uiSettings = new Windows.UI.ViewManagement.UISettings();
                var backgroundColor = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                theme = backgroundColor.ToString(CultureInfo.InvariantCulture) == "#FF000000" ? "dark" : "light";
            }
            catch
            {
            }

            string html = MarkdownHelper.MarkdownHtml(markdownContent, theme, filePath, null);

            string mermaidScript = @"
<script type=""module"">
import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
mermaid.initialize({ startOnLoad: true });
</script>
";
            html = html.Replace("</body>", mermaidScript + "</body>", StringComparison.Ordinal);

            return html;
        }

        public static List<Models.TocItem> ExtractTableOfContents(string markdownContent)
        {
            var tocItems = new List<Models.TocItem>();

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseAutoIdentifiers()
                .Build();

            var document = Markdown.Parse(markdownContent, pipeline);

            var headings = document.Descendants<HeadingBlock>();

            foreach (var heading in headings)
            {
                var inlineContent = heading.Inline?.FirstChild;
                var titleText = inlineContent?.ToString() ?? string.Empty;

                var id = heading.GetAttributes()?.Id ?? GenerateId(titleText);

                tocItems.Add(new Models.TocItem
                {
                    Title = titleText,
                    Level = heading.Level,
                    Id = id,
                });
            }

            return tocItems;
        }

        private static string GenerateId(string text)
        {
            return text.ToLower(CultureInfo.InvariantCulture)
                       .Replace(" ", "-", StringComparison.Ordinal)
                       .Replace("'", string.Empty, StringComparison.Ordinal);
        }
    }
}
