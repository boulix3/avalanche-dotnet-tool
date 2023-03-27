using Avalanche.Cli.Binaries;
using Avalanche.Cli.Commands;

namespace Avalanche.Cli;

public record AppConfig(
    string SubnetName, string VmBuildCommand, string VmBinaryPath, string GenesisPath,
    int AvalancheNetworkRunnerPort, string BasePath,
    string AvalanchePluginsPath, SeqConfig? Seq = null)
{
    internal const string DefaultPath = "Avalanche.config.json";

    public string AvalancheGoPath => BinaryDependencyData.GetBinaryPath(BinaryDependency.avalancheGo, BasePath);

    public string AvalancheNetworkRunnerPath =>
        BinaryDependencyData.GetBinaryPath(BinaryDependency.avalancheNetworkRunner, BasePath);
    public string LogsFolderPath => Path.Combine(BasePath, "data/logs");

    internal CommandType Command { get; set; }
    public string ConfigFilePath { get; set; } = DefaultPath;
}

public record SeqConfig(string Url, string Token);