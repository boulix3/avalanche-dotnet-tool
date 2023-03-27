using Spectre.Console;
using Spectre.Console.Rendering;

namespace Avalanche.Cli;

public static class Log
{
    internal static void Debug(string arg)
    {
        AnsiConsole.WriteLine(arg);
    }

    internal static void Markup(string arg)
    {
        AnsiConsole.MarkupLine(arg);
    }

    internal static void Error(string title, string content)
    {
        Error(title, new Markup(content));
    }

    internal static void Error(string title, IRenderable content)
    {
        Panel panel = new Panel(content)
        {
            Header = new PanelHeader(
                $"[bold red] {title} [/]", Justify.Center
            ),
            Border = BoxBorder.Double,
            Expand = true
        };
        AnsiConsole.Write(panel);
    }

    internal static void Error(string txt)
    {
        Error("Error", txt);
    }

    internal static void Error(Exception e, string title)
    {
        Error(title, e.GetRenderable());
    }
}