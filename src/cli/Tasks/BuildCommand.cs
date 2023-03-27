using System.Security.Cryptography;
using System.Text;

using CliWrap;

using NBitcoin.DataEncoders;

namespace Avalanche.Cli.Tasks;

public class BuildCommand : Command
{
    public BuildCommand(AppConfig config, CancellationTokenSource cancel) : base(true, config, cancel)
    {
    }

    public override async Task<bool> RunCommand()
    {
        string pluginDest = GetPluginFullPath(_config.AvalanchePluginsPath, _config.SubnetName);
        if (File.Exists(_config.VmBinaryPath))
        {
            File.Delete(_config.VmBinaryPath);
        }

        if (File.Exists(pluginDest))
        {
            File.Delete(pluginDest);
        }

        bool result = await RunBuild();
        if (!result)
        {
            return false;
        }

        if (!File.Exists(_config.VmBinaryPath))
        {
            Log.Error(
                $"Unable to find build output binary - Build should generate binary in path [red]{_config.VmBinaryPath}[/] (see config file [green]{_config.ConfigFilePath}[/])");
            return false;
        }

        Log.Markup($"Subnet [bold green]{_config.SubnetName}[/] : ");
        Log.Markup($"Vm plugin binary deployed to [bold green] {pluginDest} [/]");
        File.Copy(_config.VmBinaryPath, pluginDest);
        return result;
    }

    private async Task<bool> RunBuild()
    {
        string[]? buildCommandParts = _config.VmBuildCommand?.Split(" ");
        if (buildCommandParts?.Length > 0)
        {
            CliWrap.Command cmd = CliWrap.Cli.Wrap(buildCommandParts[0]);
            if (buildCommandParts.Length > 1)
            {
                cmd = cmd.WithArguments(buildCommandParts.Skip(1));
            }

            cmd = cmd.WithStandardOutputPipe(PipeTarget.ToDelegate(Log.Debug))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(Log.Error));
            await cmd.ExecuteAsync(_cancel.Token);
            return true;
        }

        return false;
    }

    public static string GetPluginFullPath(string pluginDir, string subnetName)
    {
        return Path.Combine(pluginDir, ToSubnetId(subnetName));
    }

    public static string ToSubnetId(string subnetName)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(subnetName);
        if (bytes.Length < 32)
        {
            byte[] newBytes = new byte[32];
            bytes.CopyTo(newBytes, 0);
            bytes = newBytes;
        }

        byte[] hash = SHA256.HashData(bytes);
        bytes = bytes.Concat(hash.Skip(hash.Length - 4)).ToArray();
        Base58Encoder encoder = new Base58Encoder();
        string? result = encoder.EncodeData(bytes);
        return result;
    }
    
}