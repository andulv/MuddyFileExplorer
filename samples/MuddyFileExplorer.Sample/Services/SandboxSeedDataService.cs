using System.Text;

namespace MuddyFileExplorer.Sample.Services;

public sealed class SandboxSeedDataService(IWebHostEnvironment environment)
{
    public string RootPath { get; } = Path.Combine(environment.ContentRootPath, "App_Data", "FileExplorerSandbox");

    public void EnsureSeedData()
    {
        Directory.CreateDirectory(RootPath);

        WriteIfMissing("README.md", "# Mud FileExplorer sample\n\nThis sandbox is safe to rename, move, upload into, and delete from.\n");
        WriteIfMissing("notes.txt", "Compact MudBlazor file explorer sample data.\n");
        WriteIfMissing("Documents/roadmap.md", "# Roadmap\n\n- Dense grid\n- Upload queue\n- Context actions\n");
        WriteIfMissing("Documents/specification.pdf", "PDF placeholder for file type display.\n");
        WriteIfMissing("Documents/minutes-2026-05.txt", "Meeting minutes – discussed roadmap priorities and upload UX.\n");
        WriteIfMissing("Images/photo-list.txt", "Imagine thumbnails in a later version; v1 stays compact.\n");
        WriteIfMissing("Images/banner.txt", "Placeholder for a banner image file.\n");
        WriteIfMissing("Code/example.json", "{\n  \"component\": \"MuddyFileExplorer\",\n  \"dense\": true\n}\n");
        WriteIfMissing("Code/appsettings.json", "{\n  \"Logging\": {\n    \"LogLevel\": {\n      \"Default\": \"Information\"\n    }\n  }\n}\n");
        EnsureMarkdownSeedData();
    }

    private void EnsureMarkdownSeedData()
    {
        Directory.CreateDirectory(Path.Combine(RootPath, "Markdown"));

        WriteIfMissing("Markdown/01-architecture-long.md", BuildLongMarkdown(
            "Architecture Overview",
            "This long markdown sample exercises larger file rendering with a predictable layout.",
            "Architecture topics"));
        WriteIfMissing("Markdown/02-operations-long.md", BuildLongMarkdown(
            "Operations Runbook",
            "This long markdown sample gives the explorer a second large document near the top of the list.",
            "Operations checklist"));

        for (var index = 3; index <= 100; index++)
        {
            WriteIfMissing($"Markdown/{index:D2}-sample-note.md", BuildSmallMarkdown(index));
        }
    }

    private static string BuildSmallMarkdown(int index) =>
        $$"""
        # Sample Note {{index:D2}}

        This is compact markdown seed content for list virtualization checks.
        - Item {{index:D2}} stays intentionally short.
        - It gives the sample explorer many lightweight documents.
        - The content is safe to edit or delete during demos.

        Updated: 2026-05
        """;

    private static string BuildLongMarkdown(string title, string intro, string sectionPrefix)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {title}");
        builder.AppendLine();
        builder.AppendLine(intro);
        builder.AppendLine();

        for (var section = 1; section <= 6; section++)
        {
            builder.AppendLine($"## {sectionPrefix} {section}");
            builder.AppendLine();

            for (var item = 1; item <= 18; item++)
            {
                var absoluteItem = ((section - 1) * 18) + item;
                builder.AppendLine($"- Line {absoluteItem:D3}: Sample markdown content for scrolling, sorting, and preview behavior.");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private void WriteIfMissing(string relativePath, string content)
    {
        var path = Path.Combine(RootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (!File.Exists(path))
        {
            File.WriteAllText(path, content);
        }
    }
}
