// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.IO;
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
    }
}
