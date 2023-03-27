using Avalanche.Cli.Commands;

namespace Avalanche.Cli.Tasks;

public class TaskRunner
{
    private readonly CancellationTokenSource _cancel;
    private readonly Command[] commands;

    public TaskRunner(AppConfig config, CancellationTokenSource cancel)
    {
        _cancel = cancel;
        commands = InitCommands(config, _cancel);
    }

    public static Command[] InitCommands(AppConfig config, CancellationTokenSource cancel)
    {
        return config.Command switch
        {
            CommandType.Build or CommandType.Buidl => new Command[] { new BuildCommand(config, cancel) },
            CommandType.Update => new Command[] { new DownloadLatestReleaseCommand(config, cancel) },
            CommandType.Run => new Command[]
            {
                new CheckBinariesCommand(config, cancel), new StartAvalancheNetworkRunnerCommand(config, cancel),
                new BuildCommand(config, cancel), new RunAvalancheNetworkRunnerCommand(config, cancel)
            },
            _ => throw new NotImplementedException($"TaskRunner - Unhandled command {config.Command}")
        };
    }

    public async Task Run()
    {
        List<Task<bool>> tasks = new();
        foreach (Command command in commands)
        {
            Task<bool> task = command.Run();
            tasks.Add(task);
            if (command.WaitForCompletion)
            {
                bool commandResult = await task;
                if (!commandResult)
                {
                    _cancel.Cancel();
                    break;
                }
            }
        }

        await Task.WhenAll(tasks);
        _cancel.Cancel();
    }
}