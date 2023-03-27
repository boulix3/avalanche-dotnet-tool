using System.Text.Json;

using OneOf.Types;

using Avalanche.Cli.Commands;

namespace Avalanche.Cli;

public static class Config
{
    public static string PluginsDir => ReplaceHomePath("~/.avalanchego/plugins/");
    
    public static Result<AppConfig> ReadConfigFile(FileInfo file, CommandType command)
    {
        if (!file.Exists)
        {
            return new Error<string>(
                $"The configuration file {file.FullName} does not exist - To initialize a config file, run with 'Init' option");
        }

        string json = File.ReadAllText(file.FullName);
        json = ReplaceHomePath(json);
        AppConfig? config =
            JsonSerializer.Deserialize<AppConfig>(json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        if (config != null)
        {
            config.Command = command;
            config.ConfigFilePath = file.FullName;
            return config;
        }

        return new Error<string>($"Unable to deserialize configuration file {file.FullName} - Content : {json}");
    }

    public static Result<Success> WriteConfigFile(FileInfo file, AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(
                config, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
            File.WriteAllText(file.FullName, json);
            return new Success();
        }
        catch (Exception e)
        {
            Log.Error(e, "An error has occured while writing config file " + file.FullName);
            return new Error<string>(e.Message);
        }
    }

    public static string ReplaceHomePath(string path)
    {
        return path.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
    }
}