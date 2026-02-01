// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI.Xaml;

namespace MarkdownReader.Models
{
    public class TocItem
    {
        public string Title { get; set; }

        public int Level { get; set; }

        public string Id { get; set; }

        public Thickness IndentMargin => new Thickness((Level - 1) * 12, 4, 0, 4);
    }
}
