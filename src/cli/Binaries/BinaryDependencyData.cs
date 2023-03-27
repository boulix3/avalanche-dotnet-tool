using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Avalanche.Cli.Binaries;

public record BinaryDependencyData(BinaryDependency Key, string FileName, string ArchiveFileName, string Version,
    string DownloadUrl)
{
    internal string PhysicalPath
    {
        get
        {
            string input = ArchiveFileName;
            string output = input.Replace(".tar.gz", "");
            Directory.CreateDirectory(output);
            return Directory.GetDirectories(output).FirstOrDefault() ?? output;
        }
    }

    public static BinaryDependencyData Parse(string latestReleaseJson, BinaryDependency key, string basePath)
    {
        string platformDependentContent = GetRegexPlatformDependantContent(key, Platform.PlatformType.AsT0);
        Regex regex = new Regex("https[^\"]*" + platformDependentContent + "[^\"]*\\.tar\\.gz",
            RegexOptions.NonBacktracking);
        string url = regex.Match(latestReleaseJson).Value;
        string archiveFileName = Path.Combine(basePath, "downloads", url.Split('/').Last());
        string version = url.Split('/')[7];
        string fileName = GetBinaryPath(key, version, basePath);
        return new BinaryDependencyData(key, fileName, archiveFileName, version, url);
    }

    public static string GetRegexPlatformDependantContent(BinaryDependency key, BinaryType binaryType)
    {
        return (key, binaryType) switch
        {
            (_, BinaryType.X64Linux) => "linux[_-]amd64",
            (_, BinaryType.Arm64Linux) => "linux[_-]arm64",
            (BinaryDependency.avalancheGo, BinaryType.X64OSX) => "macos",
            (BinaryDependency.avalancheNetworkRunner, BinaryType.X64OSX) => "darwin[_-]amd64",
            _ => throw new InvalidEnumArgumentException($"Cannot get binary for {key} - {binaryType}")
        };
    }


    internal static string GetBinaryPath(BinaryDependency key, string basePath)
    {
        return GetBinaryPath(key, "latest", basePath);
    }

    internal static string GetBinaryPath(BinaryDependency key, string version, string basePath)
    {
        string binaryFileName = key.GetFileName();
        string fileName = Path.Combine(GetBinaryBasePath(key, basePath), version, binaryFileName);
        return fileName;
    }

    internal static string GetBinaryBasePath(BinaryDependency key, string basePath)
    {
        string binaryFileName = key.GetFileName();
        return Path.Combine(basePath, "bin", binaryFileName);
    }
}