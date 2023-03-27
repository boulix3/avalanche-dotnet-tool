using Spectre.Console;

using Avalanche.Cli.Binaries;

namespace Avalanche.Cli.Tasks;

internal sealed class DownloadLatestReleaseCommand : Command
{
    public DownloadLatestReleaseCommand(AppConfig config, CancellationTokenSource cancel) : base(true, config, cancel)
    {
    }

    public override async Task<bool> RunCommand()
    {
        BinaryDownloader downloader = new BinaryDownloader(_config);
        IEnumerable<BinaryDependencyData> releases = downloader.DownloadLatestReleases().ToBlockingEnumerable();
        IEnumerable<BinaryDependencyData> availableDownloads =
            releases.Where(x => BinaryDownloader.CheckDownloadAvailable(x));
        if (availableDownloads.Any())
        {
            List<BinaryDependencyData> selectedDownloads = PromptDownloadItems(availableDownloads);
            await DownloadItems(selectedDownloads, downloader);
        }
        else
        {
            AnsiConsole.WriteLine("Already up to date");
        }

        return true;
    }

    private static async Task DownloadItems(List<BinaryDependencyData> selectedDownloads, BinaryDownloader downloader)
    {
        await AnsiConsole.Progress()
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(),
                new RemainingTimeColumn(), new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                await Task.WhenAll(selectedDownloads.Select(async item =>
                {
                    ProgressTask task = ctx.AddTask(item.ArchiveFileName,
                        new ProgressTaskSettings { AutoStart = false });

                    await downloader.DownloadLatestBinary(item, task);
                }));
            });
    }

    private static List<BinaryDependencyData> PromptDownloadItems(IEnumerable<BinaryDependencyData> availableDownloads)
    {
        string all = "All";
        string none = "None";
        List<string> choices = new List<string> { all };
        choices.AddRange(availableDownloads.Select(x => x.Key.GetFileName()));
        choices.Add(none);
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]New releases are available. Do you wish to download?[/]")
                .PageSize(4)
                .AddChoices(choices));
        List<BinaryDependencyData> selectedDownloads = new List<BinaryDependencyData>();
        if (choice == all)
        {
            selectedDownloads.AddRange(availableDownloads);
        }
        else if (choice != none)
        {
            selectedDownloads.Add(availableDownloads.First(x => x.Key.GetFileName() == choice));
        }

        return selectedDownloads;
    }
}