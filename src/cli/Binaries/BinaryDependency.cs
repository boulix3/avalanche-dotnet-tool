using System.ComponentModel;

namespace Avalanche.Cli.Binaries;

public enum BinaryDependency
{
    avalancheGo,
    avalancheNetworkRunner
}

public static class BinaryDependencyExtensions
{
    private static readonly Dictionary<BinaryDependency, string> fileNames = Initialize();

    private static Dictionary<BinaryDependency, string> Initialize()
    {
        return Enum.GetValues<BinaryDependency>().ToDictionary(key => key, key => key switch
        {
            BinaryDependency.avalancheGo => "avalanchego",
            BinaryDependency.avalancheNetworkRunner => "avalanche-network-runner",
            _ => throw new InvalidEnumArgumentException($"missing filename mapping for binary dependency {key}")
        });
    }

    public static string GetFileName(this BinaryDependency item)
    {
        return fileNames[item];
    }
}