namespace Avalanche.Cli.Binaries;

public static class BinaryManager
{
    public static string GetLatestReleaseJsonPath(string basePath, BinaryDependency binaryType)
    {
        string binaryName = binaryType.GetFileName();
        return $"{basePath}/downloads/{binaryName}-latestrelease.json";
    }

    public static void CreateFolders(AppConfig config)
    {
        string basePath = config.BasePath;
        Directory.CreateDirectory($"{basePath}/downloads");
        Directory.CreateDirectory($"{basePath}/bin");
        Directory.CreateDirectory($"{basePath}/data");
        Directory.CreateDirectory(config.AvalanchePluginsPath);
    }
}