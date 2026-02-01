# Feature: Markdown Reader

- **Author**: TBD
- **Status**: Draft
- **PR**: TBD

## Problem Statement

PowerToys currently offers **Peek**, which provides a "Quick Look" experience for files, including Markdown. However, Peek is transient by design—it is intended for glancing at content, not for reading long-form documentation, specifications, or READMEs. 

When a user wants to read a large Markdown file (e.g., the PowerToys `CONTRIBUTING.md` or a project's documentation), they are forced to open it in a text editor (like VS Code or Notepad) or a browser. Text editors are optimized for *editing*, often showing raw syntax or requiring a split-pane view that reduces reading space. Browsers require a file association change or an extension.

There is no "Adobe Reader for Markdown" on Windows—a native, lightweight app dedicated to the *reading* experience.

## Description

**Markdown Reader** is a new PowerToys module designed to provide a premium reading experience for Markdown files. It functions as a standalone application that can be set as the default handler for `.md` files.

Key features:
- **Standalone Window**: Unlike Peek, it has a taskbar entry and can be minimized/maximized/snapped.
- **Table of Contents (TOC)**: Automatically generated sidebar for navigating headers (#, ##).
- **Tabbed Interface**: Open multiple documents in a single window.
- **Search**: `Ctrl+F` to search within the rendered document.
- **Modern UI**: Built with **WinUI 3** to match the Windows 11 aesthetic (Mica/Acrylic).

## Usage Scenarios

1.  **Documentation Reading**: A developer downloads a library and wants to read the `README.md` and `docs/*.md` without opening their heavy IDE. They double-click the file, and it opens in Markdown Reader with a clear TOC.
2.  **Spec Review**: A PM reviews a feature spec written in Markdown. They keep the Markdown Reader window open on a second monitor while checking the implementation.
3.  **Local Knowledge Base**: A user allows their notes (Obsidian/Logseq) to be read quickly in a "Read-Only" mode without risking accidental edits.

## User Interface Design

### Main Window Layout
The application uses a `NavigationView` (WinUI 3) structure:

```
+-------------------------------------------------------+
|  [Tabs] File1.md | File2.md (+)         [Settings]    |
+-------------------+-----------------------------------+
|  [Sidebar]        |                                   |
|                   |                                   |
|  > Header 1       |  # Header 1                       |
|    - Sub 1.1      |                                   |
|    - Sub 1.2      |  Content rendered via WebView2    |
|  > Header 2       |                                   |
|                   |                                   |
|                   |                                   |
+-------------------+-----------------------------------+
|  [Status Bar] Word Count: 1200 | Encoding: UTF-8      |
+-------------------------------------------------------+
```

## Technical Architecture

### Technology Stack
- **UI Framework**: WinUI 3 (Windows App SDK).
- **Rendering Engine**: `Microsoft.Web.WebView2`.
- **IPC**: Using `System.IO.Pipes` (or standard PowerToys runner arguments) for single-instance enforcement (opening new files in existing window).

### Class Structure

#### 1. Data Models
```csharp
public class MarkdownTabItem
{
    public string FilePath { get; set; }
    public string Title { get; set; }
    public ObservableCollection<TocNode> TableOfContents { get; set; }
}

public class TocNode
{
    public string HeaderText { get; set; }
    public int Level { get; set; }
    public string AnchorId { get; set; }
    public ObservableCollection<TocNode> Children { get; set; }
}
```

#### 2. Rendering Logic
We will reuse the robust Markdown-to-HTML conversion from `Peek`:

```csharp
// Reuse from: PowerToys.FilePreviewCommon
public class MarkdownParser
{
    public string GenerateHtml(string markdownContent) {
        // ... utilizes existing MarkdownHelper.MarkdownHtml logic
        // ... injects custom JS for scroll synchronization
    }
}
```

#### 3. Settings Schema (`settings.json`)
```json
{
  "properties": {
    "theme": { "type": "string", "enum": ["system", "light", "dark"] },
    "fontSize": { "type": "integer", "default": 14 },
    "fontFamily": { "type": "string", "default": "Segoe UI" },
    "showLineNumbers": { "type": "boolean", "default": false },
    "autoSyncTOC": { "type": "boolean", "default": true }
  }
}
```

## Implementation Details

### HTML/JS Bridge
To enable the interactive Table of Contents and Scroll Sync, we will use `WebView2`'s `PostWebMessageAsJson` and `WebMessageReceived`.

- **C# to JS**: `PostWebMessageAsJson({ type: 'scrollTo', target: '#header-1' })`
- **JS to C#**: `window.chrome.webview.postMessage({ type: 'scrollUpdate', currentHeader: '#header-1' })` -> Updates the Sidebar selection.

## External Dependencies
- `Microsoft.Web.WebView2` (Already present in PowerToys).
- `Markdig` (Already present in PowerToys, used by FilePreviewCommon).

## Security
- The WebView2 instance will be sandboxed.
- Scripts will be disabled by default unless required for specific rendering libraries (like Mermaid).
- Navigation to external URLs will be intercepted and opened in the default system browser via `Launcher.LaunchUriAsync`.
