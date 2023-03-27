using Grpc.Net.Client;

using Rpcpb;

namespace Avalanche.Cli.Tasks;

public class RunAvalancheNetworkRunnerCommand : Command
{
    public RunAvalancheNetworkRunnerCommand(AppConfig config, CancellationTokenSource cancel) : base(false, config,
        cancel)
    {
    }

    public override async Task<bool> RunCommand()
    {
        ControlService.ControlServiceClient service = new ControlService.ControlServiceClient(
            GrpcChannel.ForAddress($"http://localhost:{_config.AvalancheNetworkRunnerPort}")
        );
        StartRequest startRequest = new StartRequest();
        startRequest.BlockchainSpecs.Add(new BlockchainSpec
        {
            BlockchainAlias = _config.SubnetName, VmName = _config.SubnetName, Genesis = _config.GenesisPath
        });
        startRequest.PluginDir = _config.AvalanchePluginsPath;
        startRequest.ExecPath = _config.AvalancheGoPath;
        startRequest.NumNodes = 5;
        await service.StartAsync(startRequest);
        return true;
    }
}