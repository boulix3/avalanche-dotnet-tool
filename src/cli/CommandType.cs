namespace Avalanche.Cli.Commands;

public enum CommandType
{
    /// <summary>
    ///     Run your VM locally : Builds your project, runs avalanche-network-runner and registers your subnet.
    /// </summary>
    Run,

    /// <summary>
    ///     Runs your build command, and copies the binary to the avalanchego plugins folder using the VMId as the file name.
    /// </summary>
    Build,

    /// <summary>
    ///     Same as Build, but for 1337s only
    /// </summary>
    Buidl,

    /// <summary>
    ///     Downloads the binaries' latest versions (avalanchego and avalanche-network-runner)
    /// </summary>
    Update,

    /// <summary>
    ///     Initialize with an example config file
    /// </summary>
    Init
}