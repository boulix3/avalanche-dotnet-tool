using System.Reflection.Metadata;
using System.Text;
using Avalanche.Cli.Binaries;
using Avalanche.Cli.Commands;
using Avalanche.Cli.Tasks;
using OneOf.Types;
using Spectre.Console;

namespace Avalanche.Cli;

public static class Program
{
    private static int Failure(Exception e)
    {
        Log.Error(e, "An unexpected exception occurred!");
        return failure;
    }
    private const int failure = 1;
    private const int success = 0;

    /// <summary>
    ///     My example app
    /// </summary>
    /// <param name="configFile">The configuration file to use</param>
    /// <param name="argument">
    ///     The command to use :
    ///     Run:    Run your VM locally : Builds your project,
    ///     runs avalanche-network-runner and registers your subnet.
    ///     Build:  Runs your build command, and copies
    ///     the binary to the AvalancheGo plugins folder
    ///     using the VMId as the file name.
    ///     BUIDL:  Same as Build, but for 1337s
    ///     Update: Downloads the binaries' latest versions
    ///     (AvalancheGo and avalanche-network-runner)
    ///     Init:   Initialize with an example config file
    /// </param>
    public static async Task<int> Main(CommandType argument = CommandType.Run,
        string configFile = AppConfig.DefaultPath)
    {
        try
        {
            AnsiConsole.Write(
                new FigletText("Avalanche Dotnet Cli")
                    .Centered()
                    .Color(Color.Green));
            FileInfo configFileInfo = new FileInfo(configFile);
            Result<AppConfig> settings = Config.ReadConfigFile(configFileInfo, argument);
            if (argument == CommandType.Init)
            {
                return InitializeConfig(configFileInfo);
            }
            Result<BinaryType> platform = Platform.PlatformType;
            platform.Switch(
                x => Console.WriteLine(x.ToString()),
                x => Console.WriteLine(x.Value)
            );

            Result<Success> validationResult = ValidateStartup(settings, platform);
            if (!validationResult.Success)
            {
                return Failure(new ArgumentException(validationResult.ErrorMsg));
            }

            AppConfig? config = settings.AsT0;
            BinaryManager.CreateFolders(config);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            TaskRunner taskRunner = new TaskRunner(config, tokenSource);
            await taskRunner.Run();
            return success;
        }
        catch (Exception e)
        {
            return Failure(e);
        }
    }

    private static int InitializeConfig(FileInfo configFileInfo)
    {
        var subnetName = AnsiConsole.Ask("Subnet name", "MySubnet");
        var vmBuildCommand = AnsiConsole.Ask("Build command", $"dotnet publish {subnetName}.csproj -o /tmp/{subnetName}");
        var vmBinaryPath = AnsiConsole.Ask("Build command output (binary path)", $"/tmp/{subnetName}/{subnetName}");
        var genesisPath = AnsiConsole.Ask("Genesis path", $"genesis.json");
        var avalancheNetworkRunnerPort = AnsiConsole.Ask("Avalanche network runner port", 8080);
        var basePath = AnsiConsole.Ask("Avalanche base path", "~/.Avalanche/");
        var avalanchePluginsPath = AnsiConsole.Ask("Avalanche plugins path", "~/.avalanchego/plugins");
        var enableSeq = AnsiConsole.Ask("Enable Seq logging", false);
        SeqConfig? seq = null;
        if (enableSeq)
        {
            var seqUrl = AnsiConsole.Ask("Seq url", "http://localhost:18080/");
            var seqToken = AnsiConsole.Ask("Seq token", "1234567801234567890");
            seq = new SeqConfig(seqUrl, seqToken);
        }
        var config = new AppConfig(subnetName, vmBuildCommand, vmBinaryPath, genesisPath, avalancheNetworkRunnerPort,
            basePath, avalanchePluginsPath, seq);
        var result = Config.WriteConfigFile(configFileInfo, config);
        if (result.Success)
        {
            Log.Debug($"Configuration file has been written to [green]{configFileInfo.FullName}");
            Log.Debug($"Run with [green underline]Avalanche run --config-file {configFileInfo.FullName}");
        }
        return result.Success ? success : failure;
    }

    

    private static Result<Success> ValidateStartup(params IResult[] arguments)
    {
        StringBuilder errors = new();
        foreach (IResult item in arguments)
        {
            if (!item.Success)
            {
                errors.AppendLine(item.ErrorMsg);
            }
        }

        return errors.Length > 0 ? new Error<string>(errors.ToString()) : new Success();
    }
}