using CliWrap;

namespace Avalanche.Cli.Tasks;

public class StartAvalancheNetworkRunnerCommand : Command
{
    public StartAvalancheNetworkRunnerCommand(AppConfig config, CancellationTokenSource cancel) : base(false, config,
        cancel)
    {
    }

    public override async Task<bool> RunCommand()
    {
        CliWrap.Command cmd = CliWrap.Cli.Wrap(_config.AvalancheNetworkRunnerPath);
        var logsFolderPath = _config.LogsFolderPath;
        cmd = cmd.WithArguments(

            $"""server --port=":{_config.AvalancheNetworkRunnerPort}" --grpc-gateway-port=":{_config.AvalancheNetworkRunnerPort + 1}" --log-level debug --log-dir="{logsFolderPath}" """);

        cmd = cmd.WithStandardOutputPipe(PipeTarget.ToDelegate(Log.Debug))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(Log.Error));
        await cmd.ExecuteAsync(_cancel.Token);
        _cancel.Cancel();
        return true;
    }
}