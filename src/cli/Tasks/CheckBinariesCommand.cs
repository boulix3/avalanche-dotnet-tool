namespace Avalanche.Cli.Tasks;

public class CheckBinariesCommand : Command
{
    public CheckBinariesCommand(AppConfig config, CancellationTokenSource cancel) : base(true, config, cancel)
    {
    }

    public override async Task<bool> RunCommand()
    {
        bool avalancheNetworkRunnerExists = CheckFileExists(_config.AvalancheNetworkRunnerPath);
        bool avalancheGoExists = CheckFileExists(_config.AvalancheGoPath);
        if (!avalancheGoExists || !avalancheNetworkRunnerExists)
        {
            DownloadLatestReleaseCommand updateCommand = new DownloadLatestReleaseCommand(_config, _cancel);
            await updateCommand.Run();
            avalancheNetworkRunnerExists = CheckFileExists(_config.AvalancheNetworkRunnerPath);
            avalancheGoExists = CheckFileExists(_config.AvalancheGoPath);
        }

        return avalancheNetworkRunnerExists && avalancheGoExists;
    }

    private static bool CheckFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Log.Error($"File {filePath} does not exist");
            return false;
        }

        return true;
    }
}